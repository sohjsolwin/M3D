using M3D.Spooling.Client;
using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;
using M3D.Spooling.Core.Controllers.PrintJobs;
using M3D.Spooling.Embedded_Firmware;
using M3D.Spooling.Preprocessors;
using M3D.Spooling.Printer_Profiles;
using M3D.Spooling.Sockets;
using Nito;
using RepetierHost.model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace M3D.Spooling.Core.Controllers
{
  internal class FirmwareController : BaseController, IPublicFirmwareController, IGCodePluginable
  {
    public GCodeType gcodetype = GCodeType.BinaryV2;
    private string mPluginResultAccumulator = "";
    private ThreadSafeVariable<bool> __boundsCheckingEnabled = new ThreadSafeVariable<bool>(true);
    private object threadsync = new object();
    private Deque<GCode> manual_commands = new Deque<GCode>();
    private ThreadSafeVariable<bool> _printer_idle = new ThreadSafeVariable<bool>(true);
    private ThreadSafeVariable<bool> _last_message_clear = new ThreadSafeVariable<bool>(true);
    private ThreadSafeVariable<bool> _resend_last_command = new ThreadSafeVariable<bool>(false);
    private ThreadSafeVariable<int> _auto_resend_count = new ThreadSafeVariable<int>(0);
    private Stopwatch oswResendTimer = new Stopwatch();
    private Stopwatch oswRefreshTimer = new Stopwatch();
    private Stopwatch oswGantryTimer = new Stopwatch();
    private Stopwatch oswPauseTimer = new Stopwatch();
    private Stopwatch oswStopwatchGeneralUse = new Stopwatch();
    private ASCIIEncoding mAsciiEncoder = new ASCIIEncoding();
    private bool m_bDoStartupOnWait = true;
    private bool jobmessage_not_sent = true;
    private string read_accumulator = "";
    private ConcurrentDictionary<string, FirmwareControllerPlugin> m_odRegisteredPlugins;
    private ConcurrentDictionary<string, string> m_odLinkedPluginGCodes;
    private string mPluginToReceiveCommand;
    private GCode mPluginGCodeSent;
    private int m_nWaitCounter;
    private JobController m_oJobController;
    private FirmwareController.SpecialMessage cur_special_message;
    private bool mSpecialMessageSent;
    private GCode m_ogcLastGCodeSent;
    private RequestGCode LastGCodeType;
    private const int MAX_AUTO_RESEND_TRIES = 1;
    private const int WAITS_BEFORE_AUTO_RESEND = 10;
    private bool invalid_z_sent;
    private float beginOfPrintE;
    internal EEPROMMapping eeprom_mapping;

    public FirmwareController(string initial_read_accumulator, PrinterConnection base_printer, PrinterInfo info, Logger logger, ThreadSafeVariable<bool> shared_shutdown, IBroadcastServer broadcastserver, InternalPrinterProfile printerProfile)
      : base(base_printer, info, logger, broadcastserver, printerProfile)
    {
      m_odRegisteredPlugins = new ConcurrentDictionary<string, FirmwareControllerPlugin>();
      m_odLinkedPluginGCodes = new ConcurrentDictionary<string, string>();
      m_oJobController = new JobController((IPublicFirmwareController) this);
      eeprom_mapping = new EEPROMMapping(printerProfile.EEPROMConstants);
      read_accumulator = initial_read_accumulator;
      Status = PrinterStatus.Connecting;
      ExtruderDetails.Temperature = -273f;
      ExtruderDetails.iNozzleSizeMicrons = MyPrinterProfile.AccessoriesConstants.NozzleConstants.iDefaultNozzleSizeMicrons;
      CalibrationDetails.Calibration_Valid = true;
      ExtruderDetails.Z_Valid = true;
    }

    public override void Shutdown()
    {
      if (m_oJobController != null)
      {
        var num = (int)m_oJobController.ForceShutdownNow();
      }
      base.Shutdown();
    }

    private uint ValidateFirmwareVersion(uint version)
    {
      if (version == uint.MaxValue)
      {
        return 0;
      }

      for (uint index = 0; index < 16U; ++index)
      {
        if ((int) version == (int) index * 16777216)
        {
          return 0;
        }
      }
      return version;
    }

    public void OnUnexpectedDisconnect(MessageType reason)
    {
      if (!mSpecialMessageSent)
      {
        BroadcastServer.BroadcastMessage(new SpoolerMessage(reason, MySerialNumber, "null").Serialize());
      }

      Status = PrinterStatus.Error_PrinterNotAlive;
    }

    public void VStopwatchReset()
    {
      if (oswStopwatchGeneralUse == null)
      {
        return;
      }

      oswStopwatchGeneralUse.Reset();
      oswStopwatchGeneralUse.Start();
    }

    public long NStopwatchReturnTime()
    {
      long num = -1;
      if (oswStopwatchGeneralUse != null && oswStopwatchGeneralUse.IsRunning)
      {
        oswStopwatchGeneralUse.Stop();
        num = oswStopwatchGeneralUse.ElapsedMilliseconds;
      }
      return num;
    }

    public PluginResult RegisterPlugin(FirmwareControllerPlugin plugin)
    {
      if (!m_odRegisteredPlugins.TryAdd(plugin.ID, plugin))
      {
        return PluginResult.FAILED_PluginIDAlreadyInUse;
      }

      plugin.RegisterGCodes((IGCodePluginable) this);
      return PluginResult.Success;
    }

    public PluginResult LinkGCodeWithPlugin(string gcode, string pluginID)
    {
      var upperInvariant = gcode.ToUpperInvariant();
      var gcode1 = new GCode(upperInvariant);
      if (!gcode1.hasG && !gcode1.hasM)
      {
        return PluginResult.FAILED_NotaValidMorGCode;
      }

      if (m_odLinkedPluginGCodes.ContainsKey(upperInvariant))
      {
        if (m_odLinkedPluginGCodes[upperInvariant] != pluginID)
        {
          return PluginResult.FAILED_GCODEAlreadyRegistered;
        }
      }
      else if (!m_odLinkedPluginGCodes.TryAdd(upperInvariant, pluginID))
      {
        return PluginResult.FAILED_GCODEAlreadyRegistered;
      }

      return PluginResult.Success;
    }

    public CommandResult RegisterExternalPluginGCodes(string pluginID, string[] gCodeList)
    {
      foreach (var gCode in gCodeList)
      {
        var num = (int)LinkGCodeWithPlugin(gCode, pluginID);
      }
      return CommandResult.Success;
    }

    private void ReadForPlugin(string receivedData)
    {
      if (!string.IsNullOrEmpty(mPluginResultAccumulator))
      {
        mPluginResultAccumulator += "\n";
      }

      mPluginResultAccumulator += receivedData;
      if (!receivedData.StartsWith("ok"))
      {
        return;
      }

      if (m_odRegisteredPlugins.ContainsKey(mPluginToReceiveCommand))
      {
        m_odRegisteredPlugins[mPluginToReceiveCommand].ProcessGCodeResult(mPluginGCodeSent, mPluginResultAccumulator, MyPrinterInfo);
      }
      else
      {
        base_printer.BroadcastPluginMessage(new SpoolerMessage(MessageType.PluginMessage, MySerialNumber, mPluginResultAccumulator, mPluginToReceiveCommand, mPluginGCodeSent.getAscii(false, false)));
      }

      mPluginToReceiveCommand = (string) null;
      mPluginGCodeSent = (GCode) null;
    }

    private void CheckForPluginCommand(GCode manual_gcode)
    {
      mPluginToReceiveCommand = (string) null;
      mPluginGCodeSent = (GCode) null;
      mPluginResultAccumulator = "";
      var key = (string) null;
      if (manual_gcode.hasM)
      {
        key = PrinterCompatibleString.Format("M{0}", (object) manual_gcode.M);
      }
      else if (manual_gcode.hasG)
      {
        key = PrinterCompatibleString.Format("G{0}", (object) manual_gcode.G);
      }

      if (string.IsNullOrEmpty(key) || !m_odLinkedPluginGCodes.ContainsKey(key))
      {
        return;
      }

      mPluginToReceiveCommand = m_odLinkedPluginGCodes[key];
      mPluginGCodeSent = manual_gcode;
    }

    public override void DoConnectionLogic()
    {
      if (Status == PrinterStatus.Bootloader_FirmwareUpdateFailed)
      {
        OnFirmwareUpdateFailed();
      }
      else
      {
        ProcessReads();
        DoWrites();
        base_printer.RequestFastSerialProcessing(IsPrinting && m_oJobController.Info.Status == JobStatus.SavingToSD);
        if (!m_oJobController.Printing)
        {
          return;
        }

        if (Status == PrinterStatus.Firmware_IsWaitingToPause && m_oJobController.MinutesElapsed > 5.0)
        {
          PauseAllMoves();
        }

        if (oswRefreshTimer.Elapsed.Minutes > 10)
        {
          UpdateStatsTime(m_oJobController.GetElapsedReset);
          oswRefreshTimer.Reset();
          oswRefreshTimer.Start();
        }
        if (!IsPaused)
        {
          return;
        }

        CheckPauseTimer();
      }
    }

    private void OnFirmwareUpdateFailed()
    {
      BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.FirmwareErrorCyclePower, MySerialNumber, (string) null).Serialize());
    }

    private string ReadDataFromSerial()
    {
      try
      {
        try
        {
          read_accumulator += ReadExisting();
        }
        catch (TimeoutException ex)
        {
        }
        catch (Exception ex)
        {
          ErrorLogger.LogException("Exception in FirmwareConnection.ReadDataFromSerial " + ex.Message, ex);
        }
        read_accumulator = read_accumulator.Replace('\r', '\n');
      }
      catch (InvalidOperationException ex)
      {
        Status = PrinterStatus.Error_PrinterNotAlive;
      }
      catch (Exception ex)
      {
        ErrorLogger.LogErrorMsg("Exception in FirmwareConnection.ReadDataFromSerial " + ex.Message, "Exception");
      }
      if (!read_accumulator.Contains("\n"))
      {
        return "";
      }

      var length = read_accumulator.IndexOf('\n');
      var str = read_accumulator.Substring(0, length);
      read_accumulator = read_accumulator.Substring(length + 1);
      return str;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessReads()
    {
      try
      {
        var str1 = ReadDataFromSerial();
        Logger.TextType type = Logger.TextType.Read;
        if (string.IsNullOrEmpty(str1))
        {
          return;
        }

        if (str1.StartsWith("ok", StringComparison.InvariantCulture))
        {
          if (!string.IsNullOrEmpty(mPluginToReceiveCommand))
          {
            ReadForPlugin(str1);
          }

          var line_number = ProcessParameters(str1.Substring(2));
          if (LastGCodeType == RequestGCode.HiddenType)
          {
            type = Logger.TextType.None;
          }

          ProcessOK(line_number);
          if (LastGCodeType == RequestGCode.G30_32_30_ZSaveGcode)
          {
            var num = (int)WriteManualCommands("M117", "M573");
          }
        }
        else if (str1.StartsWith("rs", StringComparison.InvariantCulture))
        {
          ProcessResend(ProcessParameters(str1.Substring(2)));
        }
        else if (str1.StartsWith("skip", StringComparison.InvariantCulture))
        {
          ProcessOK(ProcessParameters(str1.Substring(4)));
        }
        else if (str1.StartsWith("wait", StringComparison.InvariantCulture))
        {
          if (!ProcessWait())
          {
            WriteLog(">> Error: Firmware became idle while waiting for a command", Logger.TextType.Feedback);
            type = Logger.TextType.None;
          }
          else
          {
            type = Logger.TextType.Wait;
          }
        }
        else if (str1 == "UNABLE TO INIT HARDWARE. CHECK MICRO MOTION CABLE CONNECTION.")
        {
          BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.MicroMotionControllerFailed, MySerialNumber, (string) null).Serialize());
        }
        else if (str1.Contains("?"))
        {
          GotoBootloader();
        }
        else if (str1.StartsWith("HF:", StringComparison.InvariantCulture))
        {
          if (m_oJobController.HasJob)
          {
            StopJobInternal();
          }

          ClearAllBlockers();
          WriteLog(">> Error: Heater failed", Logger.TextType.Error);
          BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.PrinterError, MySerialNumber, "Error: Heater failed").Serialize());
          type = Logger.TextType.None;
        }
        else if (str1.StartsWith("Error", StringComparison.InvariantCulture) || str1.StartsWith("!!", StringComparison.InvariantCulture))
        {
          if (m_oJobController.HasJob)
          {
            StopJobInternal();
          }

          ClearAllBlockers();
          var str2 = ">> " + ErrorProcessing.TranslateError(str1, out var error_code);
          if (!string.IsNullOrEmpty(str2))
          {
            WriteLog(str2, Logger.TextType.Error);
            SpoolerMessage spoolerMessage;
            if (error_code == 1008)
            {
              ExtruderDetails.Temperature = 0.0f;
              spoolerMessage = new SpoolerMessage(MessageType.PrinterTimeout, MySerialNumber, (string) null);
            }
            else
            {
              spoolerMessage = new SpoolerMessage(MessageType.PrinterError, MySerialNumber, str2);
            }

            if (spoolerMessage != null)
            {
              BroadcastServer.BroadcastMessage(spoolerMessage.Serialize());
            }
          }
          type = Logger.TextType.None;
        }
        else if (str1.StartsWith("e", StringComparison.InvariantCulture))
        {
          var str2 = ProcessSoftError(str1);
          if (!string.IsNullOrEmpty(str2))
          {
            WriteLog(">> " + str2, Logger.TextType.Feedback);
          }

          type = Logger.TextType.None;
        }
        else if (str1.StartsWith("start", StringComparison.InvariantCulture))
        {
          ClearAllBlockers();
        }
        else if (str1.StartsWith("T:", StringComparison.InvariantCulture) || str1.StartsWith("B:", StringComparison.InvariantCulture))
        {
          type = Logger.TextType.Feedback;
          ParseTemperatureInfo(str1);
        }
        else if (!string.IsNullOrEmpty(mPluginToReceiveCommand))
        {
          ReadForPlugin(str1);
        }

        if (type == Logger.TextType.None || !(!IsPrinting | false) && type != Logger.TextType.Error && type != Logger.TextType.Feedback)
        {
          return;
        }

        WriteLog(">> " + str1, type);
      }
      catch (ThreadAbortException ex)
      {
        throw ex;
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("Exception in FirmwareConnection.ProcessREads 1 " + ex.Message, ex);
        WriteLog(string.Format("FirmwareConnection.ProcessReads 1 Exception: {0}", (object) ex.Message), Logger.TextType.Error);
      }
    }

    private int ProcessParameters(string parameters)
    {
      var result1 = int.MinValue;
      try
      {
        var flag1 = false;
        var flag2 = false;
        var result2 = 0;
        var result3 = 0;
        if (string.IsNullOrEmpty(parameters))
        {
          return int.MinValue;
        }

        parameters = parameters.Trim();
        if (parameters.Length < 1)
        {
          return int.MinValue;
        }

        var length = parameters.IndexOf(' ');
        if (int.TryParse(length >= 0 ? parameters.Substring(0, length) : parameters, out result1))
        {
          parameters = length <= 0 ? (string) null : parameters.Substring(length + 1);
        }
        else
        {
          result1 = int.MinValue;
        }

        if (LastGCodeType == RequestGCode.M114GetExtruderLocation)
        {
          ExtruderDetails.position.Reset();
        }

        var unprocessed = parameters;
        for (var nextToken = GetNextToken(ref unprocessed); !string.IsNullOrEmpty(nextToken); nextToken = GetNextToken(ref unprocessed))
        {
          var option = GetOption(nextToken);
          var parameter = GetParameter(nextToken);
          if (option == "ZV")
          {
            LastGCodeType = RequestGCode.M117GetInternalState;
            ParseZValid(parameter);
          }
          else if (option == "PT")
          {
            flag1 = true;
            if (!int.TryParse(parameter, out result2))
            {
              result2 = 0;
              ErrorLogger.LogErrorMsg("Exception in FirmwareConnection.ProcessParameters PT");
              WriteLog("Exception in FirmwareConnection.ProcessParameters PT", Logger.TextType.Error);
            }
          }
          else if (option == "DT")
          {
            flag1 = true;
            if (!int.TryParse(parameter, out result3))
            {
              result3 = 0;
              ErrorLogger.LogErrorMsg("Exception in FirmwareConnection.ProcessParameters DT");
              WriteLog("Exception in FirmwareConnection.ProcessParameters DT", Logger.TextType.Error);
            }
          }
          else if (option == "FIRMWARE_NAME")
          {
            if (!string.IsNullOrEmpty(parameter))
            {
              HardwareDetails.firmware_name = parameter;
            }

            if (Status == PrinterStatus.Connected || Status == PrinterStatus.Connecting)
            {
              oswResendTimer.Stop();
              oswResendTimer.Reset();
            }
          }
          else if (option == "FIRMWARE_VERSION")
          {
            if (!string.IsNullOrEmpty(parameter))
            {
              if (uint.TryParse(parameter, out var result4))
              {
                HardwareDetails.firmware_version = result4;
              }
              else
              {
                HardwareDetails.firmware_version = 0U;
              }
            }
            HardwareDetails.firmware_version = ValidateFirmwareVersion(HardwareDetails.firmware_version);
            if (SpoolerServer.CheckFirmware)
            {
              FirmwareDetails firmware = MyPrinterProfile.ProductConstants.FirmwareList['M'];
              if (SpoolerServer.CHECK_INCOMPATIBLE_FIRMWARE && (int)HardwareDetails.firmware_version != (int) firmware.firmware_version)
              {
                GotoBootloader();
                return result1;
              }
            }
            if (MyPrinterProfile.AccessoriesConstants.SDCardConstants.HasSDCard)
            {
              var num = (int)WriteManualCommands("M27");
            }
          }
          else if (option == "FIRMWARE_URL")
          {
            if (!string.IsNullOrEmpty(parameter))
            {
              HardwareDetails.firmware_url = parameter;
            }
          }
          else if (option == "PROTOCOL_VERSION")
          {
            if (!string.IsNullOrEmpty(parameter))
            {
              HardwareDetails.protocol_version = parameter;
            }
          }
          else if (option == "MACHINE_TYPE")
          {
            if (!string.IsNullOrEmpty(parameter))
            {
              HardwareDetails.machine_type = parameter;
            }
          }
          else if (option == "EXTRUDER_COUNT")
          {
            if (!string.IsNullOrEmpty(parameter))
            {
              if (int.TryParse(parameter, out var result4))
              {
                HardwareDetails.extruder_count = result4;
              }
              else
              {
                HardwareDetails.extruder_count = 1;
              }
            }
          }
          else if (option == "REPETIER_PROTOCOL")
          {
            if (!string.IsNullOrEmpty(parameter))
            {
              if (int.TryParse(parameter, out var result4))
              {
                SetRepetierProtocol(result4);
              }
              else
              {
                SetRepetierProtocol(2);
              }
            }
            if (HardwareDetails.repetier_protocol == 0)
            {
              gcodetype = GCodeType.ASCII;
            }
            else if (HardwareDetails.repetier_protocol == 1)
            {
              gcodetype = GCodeType.BinaryV1;
            }
            else if (HardwareDetails.repetier_protocol == 2)
            {
              gcodetype = GCodeType.BinaryV2;
            }
          }
          else if (option == "X-SERIAL_NUMBER")
          {
            MySerialNumber = new PrinterSerialNumber(parameter);
            logger.ResetWithSerialNumber(MySerialNumber.ToString());
            LoadPersistantData();
            UpdateStatsOnPrinter();
          }
          else if (option == "E")
          {
            if (LastGCodeType == RequestGCode.M114GetExtruderLocation)
            {
              if (float.TryParse(parameter, NumberStyles.Float, (IFormatProvider)PrinterCompatibleString.NUMBER_FORMAT, out var result4))
              {
                ExtruderDetails.position.e = result4;
              }

              flag2 = true;
            }
          }
          else if (option == "X")
          {
            if (LastGCodeType == RequestGCode.M114GetExtruderLocation)
            {
              if (float.TryParse(parameter, NumberStyles.Float, (IFormatProvider)PrinterCompatibleString.NUMBER_FORMAT, out var result4))
              {
                ExtruderDetails.position.pos.x = result4;
              }

              flag2 = true;
            }
          }
          else if (option == "Y")
          {
            if (LastGCodeType == RequestGCode.M114GetExtruderLocation)
            {
              if (float.TryParse(parameter, NumberStyles.Float, (IFormatProvider)PrinterCompatibleString.NUMBER_FORMAT, out var result4))
              {
                ExtruderDetails.position.pos.y = result4;
              }

              ExtruderDetails.ishomed = Trilean.True;
              flag2 = true;
            }
          }
          else if (option == "Z")
          {
            if (LastGCodeType == RequestGCode.M114GetExtruderLocation)
            {
              if (float.TryParse(parameter, NumberStyles.Float, (IFormatProvider)PrinterCompatibleString.NUMBER_FORMAT, out var result4))
              {
                ExtruderDetails.position.pos.z = result4;
              }

              flag2 = true;
            }
          }
          else if (option == "C0")
          {
            SetGantryClips(true);
          }
          else if (option == "C1")
          {
            SetGantryClips(false);
          }
          else
          {
            if (option == "RC" && ushort.TryParse(parameter, out var result4))
            {
              HardwareDetails.LastResetCauseMask = result4;
              foreach (ResetCauseEnum resetCauseEnum in HardwareDetails.LastResetCause)
              {
                var str = resetCauseEnum.ToString();
                for (var startIndex = 1; startIndex < str.Length; ++startIndex)
                {
                  if (char.IsUpper(str[startIndex]))
                  {
                    str = str.Insert(startIndex, " ");
                    ++startIndex;
                  }
                }
                WriteLog(string.Format(" - {0}", (object)str), Logger.TextType.Write);
              }
            }
          }
        }
        if (flag1)
        {
          ReadReturnedEEPROMData(result2, result3);
        }

        if (flag2)
        {
          if (OnGotUpdatedPosition != null)
          {
            OnGotUpdatedPosition((IPublicFirmwareController) this, new PrinterInfo(MyPrinterInfo));
            OnGotUpdatedPosition = (ScriptCallback) null;
          }
        }
      }
      catch (Exception ex)
      {
        if (ex is ThreadAbortException)
        {
          throw ex;
        }

        ErrorLogger.LogException("Exception in FirmwareConnection.ProcessParameters 12 " + ex.Message, ex);
        WriteLog(string.Format("FirmwareConnection.ProcessParameters 10 Exception: {0}", (object) ex.Message), Logger.TextType.Error);
      }
      return result1;
    }

    private void ParseTemperatureInfo(string message)
    {
      foreach (Match match in new Regex("(\\w*):(\\d+\\.?\\d?)").Matches(message))
      {
        if (!float.TryParse(match.Groups[2].Value, NumberStyles.Float, (IFormatProvider)PrinterCompatibleString.NUMBER_FORMAT, out var result))
        {
          result = -273f;
        }

        if ((double) result > 500.0)
        {
          result = 0.0f;
        }

        var str = match.Groups[1].Value;
        if (!(str == "T"))
        {
          if (str == "B" && MyPrinterProfile.AccessoriesConstants.HeatedBedConstants.HasBuiltinHeatedBed)
          {
            AccessoryDetails.BedStatus.BedTemperature = result;
          }
        }
        else
        {
          ExtruderDetails.Temperature = result;
        }
      }
    }

    private void ParseZValid(string param)
    {
      if (!int.TryParse(param, out var result))
      {
        ErrorLogger.LogErrorMsg("Invalid ZValid flag from firmware.");
        WriteLog("Invalid ZValid flag from firmware.", Logger.TextType.Error);
        result = -1;
      }
      if (result < 0)
      {
        return;
      }

      ExtruderDetails.Z_Valid = (uint) result > 0U;
      if (ExtruderDetails.Z_Valid)
      {
        invalid_z_sent = false;
      }
      else
      {
        if (invalid_z_sent)
        {
          return;
        }

        BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.InvalidZ, MySerialNumber, (string) null).Serialize());
        invalid_z_sent = true;
      }
    }

    protected void UpdateStatsTime(float elapsed)
    {
      try
      {
        PersistantDetails.UnsavedPrintTime += elapsed;
        PersistantDetails.hours_used += elapsed;
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException(string.Format("PrinterConnection.UpdateStatsTime Exception: {0}", (object) ex.Message), ex);
        WriteLog(string.Format("PrinterConnection.UpdateStatsTime Exception: {0}", (object) ex.Message), Logger.TextType.Error);
      }
    }

    protected void UpdateStatsOnPrinter()
    {
      if ((double)PersistantDetails.UnsavedPrintTime > 0.0)
      {
        var num = (int)WriteManualCommands(PrinterCompatibleString.Format("M5321 X{0}", (object)PersistantDetails.UnsavedPrintTime));
        PersistantDetails.UnsavedPrintTime = 0.0f;
      }
      SavePersistantData();
    }

    public void SaveJobParamsToPersistantData(PersistantJobData parameters)
    {
      PersistantDetails.SavedJobInformation = parameters != null ? new PersistantJobData(parameters) : (PersistantJobData) null;
      SavePersistantData();
    }

    private void ClearAllBlockers()
    {
      ClearForWait();
      manual_commands.Clear();
    }

    private void ClearForWait()
    {
      ResendLastCommand = false;
      LastMessageClear = true;
      mSpecialMessageSent = false;
    }

    private void ProcessResend(int line_number)
    {
      if (line_number < 0 && !m_ogcLastGCodeSent.hasN)
      {
        ResendLastCommand = true;
      }
      else if (line_number >= 0 && m_ogcLastGCodeSent.hasN)
      {
        var n = m_ogcLastGCodeSent.N;
        if (n == line_number)
        {
          ResendLastCommand = true;
        }
        else
        {
          WriteLog(string.Format("Error::Invalid line number resending {0} expected {1}", (object) line_number, (object) n), Logger.TextType.Error);
        }
      }
      else if (line_number >= 0)
      {
        WriteLog(string.Format("Error::Unexpected line number received with resend."), Logger.TextType.Error);
      }
      else
      {
        WriteLog(string.Format("Error::Expected line number not received with resend."), Logger.TextType.Error);
      }
    }

    private void ProcessOK(int line_number)
    {
      if (mSpecialMessageSent)
      {
        if (cur_special_message == FirmwareController.SpecialMessage.EmergencyStop_M0)
        {
          ExtruderDetails.Temperature = -273f;
          if (MyPrinterProfile.AccessoriesConstants.HeatedBedConstants.HasBuiltinHeatedBed)
          {
            MyPrinterInfo.accessories.BedStatus.BedTemperature = -273f;
          }
        }
        else if (cur_special_message == FirmwareController.SpecialMessage.GoToBootloader)
        {
          Status = PrinterStatus.Error_PrinterNotAlive;
          Thread.Sleep(1500);
        }
        if (cur_special_message == FirmwareController.SpecialMessage.EmergencyStop_M0 || cur_special_message == FirmwareController.SpecialMessage.HaltAllMoves_M1)
        {
          if (HasActiveJob && cur_special_message == FirmwareController.SpecialMessage.HaltAllMoves_M1)
          {
            MoveToSafeLocation();
          }

          StopJobInternal();
          cur_special_message = FirmwareController.SpecialMessage.None;
        }
        else
        {
          cur_special_message = FirmwareController.SpecialMessage.None;
        }

        mSpecialMessageSent = false;
        LastMessageClear = true;
        ResendLastCommand = false;
      }
      else
      {
        if ((!m_ogcLastGCodeSent.hasN || m_ogcLastGCodeSent.N != line_number) && (m_ogcLastGCodeSent.hasN || line_number > 0))
        {
          return;
        }

        ResendLastCommand = false;
        LastMessageClear = true;
      }
    }

    private bool ProcessWait()
    {
      var flag = true;
      PrinterIdle = true;
      if (m_bDoStartupOnWait && !MyPrinterInfo.FirmwareIsInvalid)
      {
        LastMessageClear = true;
        MyPrinterProfile.Scripts.StartupScript((IPublicFirmwareController) this, new PrinterInfo(MyPrinterInfo));
        m_bDoStartupOnWait = false;
      }
      else if (!LastMessageClear)
      {
        if (m_nWaitCounter >= 10)
        {
          TryToAutoResend();
          m_nWaitCounter = 0;
          flag = false;
          if (IsConnecting)
          {
            m_bDoStartupOnWait = true;
          }
        }
        else
        {
          ++m_nWaitCounter;
        }
      }
      if (Status == PrinterStatus.Firmware_PrintingPausedProcessing)
      {
        Status = PrinterStatus.Firmware_PrintingPaused;
        oswPauseTimer.Restart();
      }
      if (!IsPausedorPausing && !IsPrinting && (!IsConnecting && !IsRecovering))
      {
        Status = PrinterStatus.Firmware_Idle;
      }

      return flag;
    }

    private string ProcessSoftError(string errorMsg)
    {
      var str = ErrorProcessing.TranslateError(errorMsg, out var error_code);
      TryToAutoResend();
      return str;
    }

    private void TryToAutoResend()
    {
      if (m_ogcLastGCodeSent != null && m_ogcLastGCodeSent.hasN && m_oJobController.HasJob)
      {
        if (++_auto_resend_count.Value > 1)
        {
          var gcode = new GCode("G4 S0");
          if (m_ogcLastGCodeSent.hasN)
          {
            gcode.N = m_ogcLastGCodeSent.N;
          }

          m_ogcLastGCodeSent = gcode;
        }
        ResendLastCommand = true;
      }
      else
      {
        ClearAllBlockers();
      }
    }

    private string GetNextToken(ref string unprocessed)
    {
      if (string.IsNullOrEmpty(unprocessed))
      {
        return (string) null;
      }

      var str = unprocessed.TrimStart();
      if (str.Length < 1)
      {
        return (string) null;
      }

      var num = str.IndexOf(' ');
      if (num > 0)
      {
        unprocessed = str.Substring(num);
        str = str.Substring(0, num);
      }
      else
      {
        unprocessed = (string) null;
      }

      return str;
    }

    private string GetOption(string token)
    {
      if (token.StartsWith("ok", StringComparison.InvariantCulture) || token.StartsWith("rs", StringComparison.InvariantCulture) || token.StartsWith("||", StringComparison.InvariantCulture))
      {
        return token.Substring(0, 2);
      }

      var length = token.IndexOf(':');
      if (length > 0)
      {
        return token.Substring(0, length);
      }

      return token;
    }

    private string GetParameter(string token)
    {
      if (token.StartsWith("ok", StringComparison.InvariantCulture) || token.StartsWith("rs", StringComparison.InvariantCulture) || token.StartsWith("||", StringComparison.InvariantCulture))
      {
        var num = token.IndexOf(' ');
        if (num > 0)
        {
          return token.Substring(num + 1).Trim();
        }

        return "";
      }
      var num1 = token.IndexOf(':');
      if (num1 > 0)
      {
        return token.Substring(num1 + 1);
      }

      return "";
    }

    public void RequestEEPROMMapping()
    {
      Status = PrinterStatus.Connecting;
      WriteLog("<-Loading Printer Data", Logger.TextType.Error);
      SortedList<int, EepromAddressInfo> allData = MyPrinterProfile.EEPROMConstants.GetAllData();
      var firmwareReadableEeprom = (int)MyPrinterProfile.EEPROMConstants.EndOfFirmwareReadableEEPROM;
      foreach (KeyValuePair<int, EepromAddressInfo> keyValuePair in allData)
      {
        EepromAddressInfo eepromAddressInfo = keyValuePair.Value;
        if ((int) eepromAddressInfo.EepromAddr <= firmwareReadableEeprom)
        {
          var num = (int)WriteManualCommands("M619 S" + eepromAddressInfo.EepromAddr + " T" + (object) eepromAddressInfo.Size);
        }
      }
    }

    protected void ReadReturnedEEPROMData(int PT, int DT)
    {
      if (!eeprom_mapping.SetValue(MyPrinterProfile.EEPROMConstants.GetEepromInfoFromLocation(PT), DT))
      {
        throw new IndexOutOfRangeException();
      }

      CheckUpdatedPTValue(PT);
      if (PT < MyPrinterProfile.EEPROMConstants.PrinterReadyIndex || Status != PrinterStatus.Connecting)
      {
        return;
      }

      SetToReady();
      BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.PrinterConnected, MySerialNumber, "null").Serialize());
      WriteLog("->Printer Data Received", Logger.TextType.Error);
    }

    private void CheckUpdatedPTValue(int eepromAddress)
    {
      if (eepromAddress == eeprom_mapping.GetLocationFromName("BacklashY"))
      {
        ProcessBacklashEEPROM();
      }
      else if (eepromAddress == eeprom_mapping.GetLocationFromName("FilamentSize"))
      {
        ProcessFilamentDataFromEEPROM();
      }
      else if (eepromAddress == eeprom_mapping.GetLocationFromName("FilamentUID"))
      {
        ProcessFilamentUIDFromEEPROM();
      }
      else if (eepromAddress == eeprom_mapping.GetLocationFromName("ZCalibrationZO"))
      {
        ProcessBedOffsetDataFromEEPROM();
      }
      else if (eepromAddress == eeprom_mapping.GetLocationFromName("BedCompensationVersion"))
      {
        ProcessBedCompensationDataFromEEPROM();
      }
      else if (eepromAddress == eeprom_mapping.GetLocationFromName("BacklashSpeed"))
      {
        ProcessBacklashSpeedEEPROM();
      }
      else if (eeprom_mapping.HasKey("EnabledFeatures") && eepromAddress == eeprom_mapping.GetLocationFromName("EnabledFeatures"))
      {
        ProcessEnabledFeatures();
      }
      else if (eeprom_mapping.HasKey("CalibrationOffset") && eepromAddress == eeprom_mapping.GetLocationFromName("CalibrationOffset"))
      {
        ProcessCalibrationOffset();
      }
      else
      {
        if (!eeprom_mapping.HasKey("NozzleSizeExtrusionWidth") || eepromAddress != eeprom_mapping.GetLocationFromName("NozzleSizeExtrusionWidth"))
        {
          return;
        }

        ProcessNozzleSizeExtrusionWidth();
      }
    }

    public void ProcessEEPROMData()
    {
      ProcessFilamentDataFromEEPROM();
      ProcessFilamentUIDFromEEPROM();
      ProcessBedCompensationDataFromEEPROM();
      ProcessBedOffsetDataFromEEPROM();
      ProcessBacklashEEPROM();
      ProcessBacklashSpeedEEPROM();
      ProcessEnabledFeatures();
    }

    private void ProcessEnabledFeatures()
    {
      if (!eeprom_mapping.HasKey("EnabledFeatures"))
      {
        return;
      }

      printerInfo.supportedFeatures.FeaturesBitField = eeprom_mapping.GetUInt32("EnabledFeatures");
      printerInfo.supportedFeatures.UsesSupportedFeatures = true;
    }

    public void ProcessCalibrationOffset()
    {
      if (!eeprom_mapping.HasKey("CalibrationOffset"))
      {
        return;
      }

      printerInfo.calibration.CALIBRATION_OFFSET = eeprom_mapping.GetFloat("CalibrationOffset");
      printerInfo.calibration.UsesCalibrationOffset = true;
    }

    public void ProcessNozzleSizeExtrusionWidth()
    {
      if (!eeprom_mapping.HasKey("NozzleSizeExtrusionWidth"))
      {
        return;
      }

      var nozzleSizeMicrons1 = MyPrinterProfile.AccessoriesConstants.NozzleConstants.iMinimumNozzleSizeMicrons;
      var nozzleSizeMicrons2 = MyPrinterProfile.AccessoriesConstants.NozzleConstants.iMaximumNozzleSizeMicrons;
      var nozzleSizeMicrons3 = MyPrinterProfile.AccessoriesConstants.NozzleConstants.iDefaultNozzleSizeMicrons;
      var iNozzleWidthMicrons = (int)eeprom_mapping.GetUInt16("NozzleSizeExtrusionWidth");
      if (iNozzleWidthMicrons < nozzleSizeMicrons1 || iNozzleWidthMicrons > nozzleSizeMicrons2)
      {
        iNozzleWidthMicrons = nozzleSizeMicrons3;
        var num = (int)SetNozzleWidth(iNozzleWidthMicrons);
      }
      printerInfo.extruder.iNozzleSizeMicrons = iNozzleWidthMicrons;
    }

    public void ProcessFilamentDataFromEEPROM()
    {
      var uint32 = eeprom_mapping.GetUInt32("FilamentColorID");
      if (uint32 == uint.MaxValue)
      {
        KnownFilament.filament_type = FilamentSpool.TypeEnum.NoFilament;
        KnownFilament.filament_temperature = 0;
        KnownFilament.filament_color_code = FilamentSpool.DefaultColorCode;
        KnownFilament.estimated_filament_length_printed = 0.0f;
      }
      else
      {
        KnownFilament.filament_color_code = uint32;
        var alignedByte1 = (int)eeprom_mapping.GetAlignedByte("FilamentTypeID");
        KnownFilament.filament_location = (alignedByte1 & 192) >> 6 != 1 ? FilamentSpool.Location.External : FilamentSpool.Location.Internal;
        var num = alignedByte1 & -193;
        KnownFilament.filament_type = !Enum.IsDefined(typeof (FilamentSpool.TypeEnum), (object) num) ? FilamentSpool.TypeEnum.NoFilament : (FilamentSpool.TypeEnum) num;
        KnownFilament.filament_temperature = (int)eeprom_mapping.GetAlignedByte("FilamentTemperature") + 100;
        KnownFilament.estimated_filament_length_printed = eeprom_mapping.GetFloat("FilamentAmount");
        var alignedByte2 = eeprom_mapping.GetAlignedByte("FilamentSize");
        if (!Enum.IsDefined(typeof (FilamentSpool.SizeEnum), (object) (int) alignedByte2))
        {
          return;
        }

        if (alignedByte2 == (ushort) 1)
        {
          KnownFilament.filament_size = FilamentSpool.SizeEnum.Pro;
        }
        else
        {
          KnownFilament.filament_size = FilamentSpool.SizeEnum.Micro;
        }
      }
    }

    public void ProcessFilamentUIDFromEEPROM()
    {
      KnownFilament.filament_uid = eeprom_mapping.GetUInt32("FilamentUID");
    }

    public void ProcessBedCompensationDataFromEEPROM()
    {
      CalibrationDetails.Calibration_Valid = false;
      CalibrationDetails.CORNER_HEIGHT_BACK_RIGHT = eeprom_mapping.GetFloat("BedCompensationBackRight");
      CalibrationDetails.CORNER_HEIGHT_BACK_LEFT = eeprom_mapping.GetFloat("BedCompensationBackLeft");
      CalibrationDetails.CORNER_HEIGHT_FRONT_RIGHT = eeprom_mapping.GetFloat("BedCompensationFrontRight");
      CalibrationDetails.CORNER_HEIGHT_FRONT_LEFT = eeprom_mapping.GetFloat("BedCompensationFrontLeft");
      CalibrationDetails.G32_VERSION = (int)eeprom_mapping.GetAlignedByte("BedCompensationVersion");
      if (CalibrationDetails.G32_VERSION == (int) byte.MaxValue)
      {
        CalibrationDetails.G32_VERSION = 0;
      }

      var flag1 = CalibrationDetails.CORNER_HEIGHT_BACK_RIGHT < -3.0 || CalibrationDetails.CORNER_HEIGHT_BACK_RIGHT > 3.0 || (CalibrationDetails.CORNER_HEIGHT_BACK_LEFT < -3.0 || CalibrationDetails.CORNER_HEIGHT_BACK_LEFT > 3.0) || (CalibrationDetails.CORNER_HEIGHT_FRONT_LEFT < -3.0 || CalibrationDetails.CORNER_HEIGHT_FRONT_LEFT > 3.0 || CalibrationDetails.CORNER_HEIGHT_FRONT_RIGHT < -3.0) || CalibrationDetails.CORNER_HEIGHT_FRONT_RIGHT > 3.0;
      var num = Math.Abs(CalibrationDetails.CORNER_HEIGHT_BACK_LEFT) >= 1.40129846432482E-45 || Math.Abs(CalibrationDetails.CORNER_HEIGHT_BACK_RIGHT) >= 1.40129846432482E-45 || Math.Abs(CalibrationDetails.CORNER_HEIGHT_FRONT_LEFT) >= 1.40129846432482E-45 ? 0 : Math.Abs(CalibrationDetails.CORNER_HEIGHT_FRONT_RIGHT) < 1.40129846432482E-45 ? 1 : 0;
      var flag2 = float.IsNaN(CalibrationDetails.CORNER_HEIGHT_BACK_LEFT) || float.IsNaN(CalibrationDetails.CORNER_HEIGHT_BACK_RIGHT) || float.IsNaN(CalibrationDetails.CORNER_HEIGHT_FRONT_LEFT) || float.IsNaN(CalibrationDetails.CORNER_HEIGHT_FRONT_RIGHT);
      if (((num == 0 || !MyPrinterProfile.OptionsConstants.VerifyGantryNonZeroValues ? (!flag1 ? 0 : ("Pro" != MyPrinterProfile.ProfileName ? 1 : 0)) : 1) | (flag2 ? 1 : 0)) != 0)
      {
        BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.BedOrientationMustBeCalibrated, MySerialNumber, (string) null).Serialize());
      }
      else if (flag1 && "Pro" == MyPrinterProfile.ProfileName)
      {
        ResetG32Values();
        RequestUpdatedG32ValuesFromFirmware();
      }
      else
      {
        CalibrationDetails.Calibration_Valid = true;
      }
    }

    public void ProcessBedOffsetDataFromEEPROM()
    {
      CalibrationDetails.CORNER_HEIGHT_BACK_RIGHT_OFFSET = eeprom_mapping.GetFloat("ZCalibrationBRO");
      CalibrationDetails.CORNER_HEIGHT_BACK_LEFT_OFFSET = eeprom_mapping.GetFloat("ZCalibrationBLO");
      CalibrationDetails.CORNER_HEIGHT_FRONT_RIGHT_OFFSET = eeprom_mapping.GetFloat("ZCalibrationFRO");
      CalibrationDetails.CORNER_HEIGHT_FRONT_LEFT_OFFSET = eeprom_mapping.GetFloat("ZCalibrationFLO");
      CalibrationDetails.ENTIRE_Z_HEIGHT_OFFSET = eeprom_mapping.GetFloat("ZCalibrationZO");
      ZeroIfNan(ref CalibrationDetails.CORNER_HEIGHT_BACK_RIGHT_OFFSET);
      ZeroIfNan(ref CalibrationDetails.CORNER_HEIGHT_BACK_LEFT_OFFSET);
      ZeroIfNan(ref CalibrationDetails.CORNER_HEIGHT_FRONT_RIGHT_OFFSET);
      ZeroIfNan(ref CalibrationDetails.CORNER_HEIGHT_FRONT_LEFT_OFFSET);
      ZeroIfNan(ref CalibrationDetails.ENTIRE_Z_HEIGHT_OFFSET);
    }

    private void ZeroIfNan(ref float fl)
    {
      if (!float.IsNaN(fl))
      {
        return;
      }

      fl = 0.0f;
    }

    public void ProcessBacklashEEPROM()
    {
      var flag = false;
      CalibrationDetails.BACKLASH_X = eeprom_mapping.GetFloat("BacklashX");
      CalibrationDetails.BACKLASH_Y = eeprom_mapping.GetFloat("BacklashY");
      if (float.IsNaN(CalibrationDetails.BACKLASH_X) || (double)CalibrationDetails.BACKLASH_X > 2.0)
      {
        flag = true;
      }

      if (float.IsNaN(CalibrationDetails.BACKLASH_Y) || (double)CalibrationDetails.BACKLASH_Y > 2.0)
      {
        flag = true;
      }

      if (!flag)
      {
        return;
      }

      BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.BacklashOutOfRange, MySerialNumber, (string) null).Serialize());
    }

    public void ProcessBacklashSpeedEEPROM()
    {
      CalibrationDetails.BACKLASH_SPEED = eeprom_mapping.GetFloat("BacklashSpeed");
      if (!float.IsNaN(CalibrationDetails.BACKLASH_SPEED) && (double)CalibrationDetails.BACKLASH_SPEED > 1.0)
      {
        return;
      }

      CalibrationDetails.BACKLASH_SPEED = MyPrinterProfile.SpeedLimitConstants.DefaultBacklashSpeed;
      EepromAddressInfo eepromInfo = MyPrinterProfile.EEPROMConstants.GetEepromInfo("BacklashSpeed");
      var num = (int)WriteManualCommands(PrinterCompatibleString.Format("M618 S{0} P{1} T{2}", eepromInfo.EepromAddr, EEPROMMapping.FloatToBinaryInt(CalibrationDetails.BACKLASH_SPEED), eepromInfo.Size));
    }

    private void ResetG32Values()
    {
      EepromAddressInfo eepromInfo1 = MyPrinterProfile.EEPROMConstants.GetEepromInfo("BedCompensationBackRight");
      EepromAddressInfo eepromInfo2 = MyPrinterProfile.EEPROMConstants.GetEepromInfo("BedCompensationBackLeft");
      EepromAddressInfo eepromInfo3 = MyPrinterProfile.EEPROMConstants.GetEepromInfo("BedCompensationFrontLeft");
      EepromAddressInfo eepromInfo4 = MyPrinterProfile.EEPROMConstants.GetEepromInfo("BedCompensationFrontRight");
      if (eepromInfo1 == null || eepromInfo2 == null || (eepromInfo3 == null || eepromInfo4 == null))
      {
        return;
      }

      var num = (int)WriteManualCommands(PrinterCompatibleString.Format("M618 S{0} P0 T{1}", eepromInfo1.EepromAddr, eepromInfo1.Size), PrinterCompatibleString.Format("M618 S{0} P0 T{1}", eepromInfo2.EepromAddr, eepromInfo2.Size), PrinterCompatibleString.Format("M618 S{0} P0 T{1}", eepromInfo3.EepromAddr, eepromInfo3.Size), PrinterCompatibleString.Format("M618 S{0} P0 T{1}", eepromInfo4.EepromAddr, eepromInfo4.Size));
    }

    public float FloatFromEEPROM(string name)
    {
      return eeprom_mapping.GetFloat(name);
    }

    private void RequestUpdatedG32ValuesFromFirmware()
    {
      EepromAddressInfo eepromInfo1 = MyPrinterProfile.EEPROMConstants.GetEepromInfo("BedCompensationBackRight");
      EepromAddressInfo eepromInfo2 = MyPrinterProfile.EEPROMConstants.GetEepromInfo("BedCompensationBackLeft");
      EepromAddressInfo eepromInfo3 = MyPrinterProfile.EEPROMConstants.GetEepromInfo("BedCompensationFrontLeft");
      EepromAddressInfo eepromInfo4 = MyPrinterProfile.EEPROMConstants.GetEepromInfo("BedCompensationFrontRight");
      EepromAddressInfo eepromInfo5 = MyPrinterProfile.EEPROMConstants.GetEepromInfo("ZCalibrationBLO");
      EepromAddressInfo eepromInfo6 = MyPrinterProfile.EEPROMConstants.GetEepromInfo("ZCalibrationBRO");
      EepromAddressInfo eepromInfo7 = MyPrinterProfile.EEPROMConstants.GetEepromInfo("ZCalibrationFRO");
      EepromAddressInfo eepromInfo8 = MyPrinterProfile.EEPROMConstants.GetEepromInfo("ZCalibrationFLO");
      EepromAddressInfo eepromInfo9 = MyPrinterProfile.EEPROMConstants.GetEepromInfo("CalibrationOffset");
      EepromAddressInfo eepromInfo10 = MyPrinterProfile.EEPROMConstants.GetEepromInfo("BedCompensationVersion");
      if (eepromInfo1 != null && eepromInfo2 != null && (eepromInfo3 != null && eepromInfo4 != null))
      {
        var num1 = (int)WriteManualCommands("M619 S" + eepromInfo1.EepromAddr + " T" + eepromInfo1.Size, "M619 S" + eepromInfo2.EepromAddr + " T" + eepromInfo2.Size, "M619 S" + eepromInfo3.EepromAddr + " T" + eepromInfo3.Size, "M619 S" + eepromInfo4.EepromAddr + " T" + (object) eepromInfo4.Size);
      }
      if (eepromInfo5 != null && eepromInfo6 != null && (eepromInfo7 != null && eepromInfo8 != null))
      {
        var num2 = (int)WriteManualCommands("M619 S" + eepromInfo5.EepromAddr + " T" + eepromInfo5.Size, "M619 S" + eepromInfo6.EepromAddr + " T" + eepromInfo6.Size, "M619 S" + eepromInfo7.EepromAddr + " T" + eepromInfo7.Size, "M619 S" + eepromInfo8.EepromAddr + " T" + eepromInfo8.Size);
      }
      if (MyPrinterInfo.calibration.UsesCalibrationOffset && eepromInfo9 != null)
      {
        var num3 = (int)WriteManualCommands("M619 S" + eepromInfo9.EepromAddr + " T" + eepromInfo9.Size);
      }
      if (eepromInfo10 == null)
      {
        return;
      }

      var num4 = (int)WriteManualCommands("M619 S" + (object) eepromInfo10.EepromAddr + " T" + (object) eepromInfo10.Size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DoWrites()
    {
      if (cur_special_message != FirmwareController.SpecialMessage.None && !mSpecialMessageSent)
      {
        if (Status == PrinterStatus.Firmware_Homing || Status == PrinterStatus.Firmware_Calibrating)
        {
          return;
        }

        var gcode = new GCode();
        if (cur_special_message == FirmwareController.SpecialMessage.EmergencyStop_M0)
        {
          gcode.Parse("M0");
        }
        else if (cur_special_message == FirmwareController.SpecialMessage.HaltAllMoves_M1)
        {
          gcode.Parse("M1");
        }
        else if (cur_special_message == FirmwareController.SpecialMessage.GetPosition_M114)
        {
          gcode.Parse("M114");
        }
        else if (cur_special_message == FirmwareController.SpecialMessage.GoToBootloader)
        {
          gcode.Parse("M115 S628");
        }

        if (!WriteToSerial(TranslateGCode(gcode)))
        {
          OnUnexpectedDisconnect(MessageType.UnexpectedDisconnect);
          WriteLog("Disconnected because of a serial write error", Logger.TextType.Error);
        }
        mSpecialMessageSent = true;
      }
      else
      {
        if (mSpecialMessageSent)
        {
          return;
        }

        UpdateJobStatus();
        SendNextCommand();
      }
    }

    private void UpdateJobStatus()
    {
      if (m_oJobController.HasJob)
      {
        MyPrinterInfo.current_job = m_oJobController.Info;
        if (!m_oJobController.Printing)
        {
          if (!m_oJobController.Processed)
          {
            return;
          }

          if (m_oJobController.HasWarnings)
          {
            if (!jobmessage_not_sent)
            {
              return;
            }

            jobmessage_not_sent = false;
            BroadcastServer.BroadcastMessage(new SpoolerMessage(m_oJobController.GetNextWarning(), MySerialNumber, (string) null).Serialize());
          }
          else
          {
            beginOfPrintE = GetCurrentFilament().estimated_filament_length_printed;
            var num1 = (int)m_oJobController.Start(out List<string> start_gcode);
            if (start_gcode != null && start_gcode.Count > 0)
            {
              var num2 = (int)WriteManualCommands(start_gcode.ToArray());
            }
            WriteLog("<< Print Job Started " + DateTime.Now.ToShortTimeString(), Logger.TextType.Error);
            if (m_oJobController.Status != JobStatus.SavingToFile)
            {
              BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.JobStarted, MySerialNumber, m_oJobController.JobName).Serialize());
            }

            if (!IsPausedorPausing)
            {
              Status = PrinterStatus.Firmware_Printing;
            }

            oswRefreshTimer.Reset();
            oswRefreshTimer.Start();
          }
        }
        else if (m_oJobController.Done)
        {
          BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.JobComplete, MySerialNumber, m_oJobController.JobName).Serialize());
          StopJobInternal();
        }
        else
        {
          m_oJobController.Update();
        }
      }
      else
      {
        MyPrinterInfo.current_job = (JobInfo) null;
      }
    }

    public void ClearWarning()
    {
      if (!m_oJobController.HasJob || m_oJobController.Status != JobStatus.Queued || !m_oJobController.HasWarnings)
      {
        return;
      }

      m_oJobController.ClearCurrentWarning();
      jobmessage_not_sent = true;
    }

    public override CommandResult WriteManualCommands(params string[] commands)
    {
      if (commands.Length < 1)
      {
        return CommandResult.Failed_Exception;
      }

      if (MyPrinterInfo.FirmwareIsInvalid)
      {
        BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.FirmwareMustBeUpdated, MySerialNumber, (string) null).Serialize());
        lock (manual_commands)
        {
          manual_commands.Clear();
        }

        return CommandResult.Failed_Exception;
      }
      foreach (var command in commands)
      {
        var newgcode = (GCode) null;
        try
        {
          newgcode = new GCode(command);
        }
        catch (Exception ex)
        {
          WriteLog(ex.Message, Logger.TextType.Error);
        }
        if (newgcode == null)
        {
          return CommandResult.Failed_Exception;
        }

        if (IsPausedorPausing && newgcode.hasG && (newgcode.G == (ushort) 28 || newgcode.G == (ushort) 32 || newgcode.G == (ushort) 30))
        {
          lock (manual_commands)
          {
            manual_commands.Clear();
          }

          var num = (int)base_printer.ReleaseLock(base_printer.MyLock);
          return CommandResult.Failed_G28_G30_G32_NotAllowedWhilePaused;
        }
        if (newgcode.hasM && newgcode.hasS && (newgcode.M == (ushort) 115 && newgcode.S == 628))
        {
          GotoBootloader();
          return CommandResult.Success;
        }
        if (newgcode.hasM && newgcode.M <= (ushort) 1)
        {
          if (newgcode.M == (ushort) 0)
          {
            SendEmergencyStop();
          }
          else if (newgcode.M == (ushort) 1)
          {
            HaltAllMoves();
          }

          return CommandResult.Success;
        }
        if (!CheckGantryClipsBeforeCommand(newgcode))
        {
          return CommandResult.Failed_GantryClipsOrInvalidZ;
        }

        lock (manual_commands)
        {
          manual_commands.AddToBack(newgcode);
        }
      }
      return CommandResult.Success;
    }

    public void AddManualCommandToFront(params string[] commands)
    {
      for (var index = commands.Length - 1; index >= 0; --index)
      {
        lock (manual_commands)
        {
          manual_commands.AddToFront(new GCode(commands[index]));
        }
      }
    }

    private bool CheckGantryClipsBeforeCommand(GCode newgcode)
    {
      if (!MyPrinterProfile.OptionsConstants.CheckGantryClips || PersistantDetails.GantryClipsRemoved || !SpoolerServer.CHECK_GANTRY_CLIPS || (!newgcode.hasM || newgcode.M != (ushort) 104 && newgcode.M != (ushort) 109) && (!newgcode.hasG || newgcode.G == (ushort) 90 || newgcode.G == (ushort) 91))
      {
        return true;
      }

      lock (manual_commands)
      {
        manual_commands.Clear();
      }

      if (!oswGantryTimer.IsRunning || oswGantryTimer.ElapsedMilliseconds > 3000L)
      {
        oswGantryTimer.Restart();
        WriteLog("Gantry clip check has not occurred", Logger.TextType.Error);
        BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.CheckGantryClips, MySerialNumber, (string) null).Serialize());
        var num = (int)base_printer.ReleaseLock(base_printer.MyLock);
      }
      return false;
    }

    public void SetGantryClips(bool clips_are_off)
    {
      oswGantryTimer.Stop();
      oswGantryTimer.Reset();
      PersistantDetails.GantryClipsRemoved = clips_are_off;
      SavePersistantData();
    }

    public void SendEmergencyStop()
    {
      lock (threadsync)
      {
        cur_special_message = FirmwareController.SpecialMessage.EmergencyStop_M0;
        mSpecialMessageSent = false;
        lock (manual_commands)
        {
          manual_commands.Clear();
        }
      }
    }

    private void HaltAllMoves()
    {
      lock (threadsync)
      {
        cur_special_message = FirmwareController.SpecialMessage.HaltAllMoves_M1;
        mSpecialMessageSent = false;
        lock (manual_commands)
        {
          manual_commands.Clear();
        }
      }
    }

    public void AbortPrint()
    {
      if (m_oJobController.HasJob && !m_oJobController.IsSavingToSDOnly)
      {
        BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.JobCanceled, MySerialNumber, m_oJobController.JobName).Serialize());
        HaltAllMoves();
      }
      else
      {
        StopJobInternal();
      }
    }

    private void MoveToSafeLocation()
    {
      if (!m_oJobController.HasJob || m_oJobController.IsSavingToSD && !m_oJobController.IsSimultaneousPrint)
      {
        return;
      }

      var num = (int)WriteManualCommands(GCodeInitializationPreprocessor.GenerateEndGCode(m_oJobController.Details, MyPrinterProfile, m_oJobController.RetractionRequired).ToArray());
    }

    private void StopJobInternal()
    {
      if (!m_oJobController.HasJob)
      {
        return;
      }

      UpdateStatsTime(m_oJobController.GetElapsedReset);
      var num1 = (int)m_oJobController.StopJob(out List<string> end_gcode);
      if (end_gcode != null && end_gcode.Count > 0)
      {
        var num2 = (int)WriteManualCommands(end_gcode.ToArray());
      }
      jobmessage_not_sent = true;
      WriteLog("<< Print Job Stopped " + DateTime.Now.ToShortTimeString(), Logger.TextType.Error);
      Status = PrinterStatus.Firmware_Executing;
      UpdateStatsOnPrinter();
      PushFilamentUsedToPrinter();
      MyPrinterInfo.current_job = (JobInfo) null;
    }

    public CommandResult ContinuePrint()
    {
      if (!IsPaused)
      {
        return CommandResult.Failed_PrinterNotPaused;
      }

      switch (m_oJobController.Resume(out List<string> resume_gcode, KnownFilament))
      {
        case JobController.Result.Success:
          if (resume_gcode != null && resume_gcode.Count > 0)
          {
            var num = (int)WriteManualCommands(resume_gcode.ToArray());
          }
          Status = PrinterStatus.Firmware_Printing;
          break;
        case JobController.Result.FAILED_NoFilament:
          return CommandResult.Failed_PrinterDoesNotHaveFilament;
        case JobController.Result.FAILED_Create:
          break;
        case JobController.Result.FAILED_JobAlreadyStarted:
          break;
        case JobController.Result.FAILED_JobNotStarted:
          break;
        case JobController.Result.FAILED_OutOfBounds:
          break;
        case JobController.Result.FAILED_NotPaused:
          break;
        case JobController.Result.FAILED_IncompatibleFilament:
          break;
        default:
          break;
      }
      return CommandResult.SuccessfullyReceived;
    }

    public CommandResult PausePrint()
    {
      if (!IsPrinting)
      {
        return CommandResult.Failed_PrinterNotPrinting;
      }

      if (m_oJobController.IsSavingToSD)
      {
        return CommandResult.Failed_CannotPauseSavingToSD;
      }

      Status = PrinterStatus.Firmware_IsWaitingToPause;
      return CommandResult.SuccessfullyReceived;
    }

    private void VerifyHomingPosition(ref GCode gcode)
    {
      if (IsPrinting || !BoundsCheckingEnabled)
      {
        return;
      }

      var gcodeList = new List<GCode>();
      var num = MyPrinterInfo.extruder.position.pos.z - MyPrinterProfile.PrinterSizeConstants.BoxTopLimitZ;
      if ((double) num <= 0.0)
      {
        return;
      }

      if (MyPrinterInfo.extruder.inRelativeMode == Trilean.False)
      {
        gcodeList.Add(new GCode("G91"));
      }

      gcodeList.Add(new GCode(PrinterCompatibleString.Format("G0 Z-{0}", (object) num)));
      if (MyPrinterInfo.extruder.inRelativeMode == Trilean.False)
      {
        gcodeList.Add(new GCode("G90"));
      }

      gcodeList.Add(new GCode("G28"));
      gcode = gcodeList[0];
      gcodeList.RemoveAt(0);
      gcodeList.Reverse();
      lock (manual_commands)
      {
        foreach (GCode gcode1 in gcodeList)
        {
          manual_commands.AddToFront(gcode1);
        }
      }
    }

    protected void TrackExtruderPosition(GCode gcode)
    {
      if (gcode.hasG)
      {
        if (gcode.G == (ushort) 90)
        {
          MyPrinterInfo.extruder.inRelativeMode = Trilean.False;
        }
        else if (gcode.G == (ushort) 91)
        {
          MyPrinterInfo.extruder.inRelativeMode = Trilean.True;
        }
        else if (gcode.G == (ushort) 28)
        {
          SetToHomeLocation();
        }
        else if (gcode.G == (ushort) 30 || gcode.G == (ushort) 32)
        {
          SetToHomeLocation();
          MyPrinterInfo.extruder.position.pos.z = MyPrinterProfile.PrinterSizeConstants.ZAfterProbing;
          MyPrinterInfo.extruder.Z_Valid = true;
        }
        else if (gcode.G == (ushort) 33)
        {
          MyPrinterInfo.extruder.position.pos.z = MyPrinterProfile.PrinterSizeConstants.ZAfterG33;
          MyPrinterInfo.extruder.Z_Valid = true;
        }
        else if (gcode.G == (ushort) 92)
        {
          if (gcode.hasE)
          {
            MyPrinterInfo.extruder.position.e = gcode.E;
          }

          if (MyPrinterProfile.OptionsConstants.G92WorksOnAllAxes)
          {
            if (gcode.hasX)
            {
              MyPrinterInfo.extruder.position.pos.x = gcode.X;
            }

            if (gcode.hasY)
            {
              MyPrinterInfo.extruder.position.pos.y = gcode.Y;
            }

            if (gcode.hasZ)
            {
              MyPrinterInfo.extruder.position.pos.z = gcode.Z;
            }
          }
          if (!gcode.hasE && !gcode.hasX && (!gcode.hasY && !gcode.hasZ))
          {
            MyPrinterInfo.extruder.position.e = 0.0f;
            if (MyPrinterProfile.OptionsConstants.G92WorksOnAllAxes)
            {
              MyPrinterInfo.extruder.position.pos.x = 0.0f;
              MyPrinterInfo.extruder.position.pos.y = 0.0f;
              MyPrinterInfo.extruder.position.pos.z = 0.0f;
            }
          }
        }
        else if ((gcode.G == (ushort) 0 || gcode.G == (ushort) 1) && MyPrinterInfo.extruder.inRelativeMode != Trilean.Unknown)
        {
          var op3dInitial = new Vector3D(MyPrinterInfo.extruder.position.pos);
          Vector3D destination = MovementUtility.op3dCalculateDestination(MyPrinterInfo.extruder.ishomed, MyPrinterInfo.extruder.Z_Valid, MyPrinterInfo.extruder.inRelativeMode, gcode, op3dInitial);
          if (!IsPrinting && BoundsCheckingEnabled)
          {
            var vector3D = new Vector3D(0.0f, 0.0f, 0.0f);
            var bDestinationHasBeenClipped = false;
            Vector3D destinationWithClipping = MovementUtility.op3dCalculateDestinationWithClipping(MyPrinterInfo.extruder.ishomed, MyPrinterInfo.extruder.Z_Valid, ref bDestinationHasBeenClipped, destination, MyPrinterInfo.extruder.position.pos, (PrinterProfile)MyPrinterProfile);
            if (bDestinationHasBeenClipped)
            {
              MovementUtility.vGetEffectiveMovementGCode(MyPrinterInfo.extruder.ishomed, MyPrinterInfo.extruder.Z_Valid, MyPrinterInfo.extruder.inRelativeMode, destinationWithClipping, MyPrinterInfo.extruder.position.pos, ref gcode);
            }

            MyPrinterInfo.extruder.position.pos = destinationWithClipping;
          }
          else
          {
            MyPrinterInfo.extruder.position.pos = destination;
          }
        }
      }
      if (!gcode.hasM || gcode.M != (ushort) 0 && gcode.M != (ushort) 1)
      {
        return;
      }

      MyPrinterInfo.extruder.position = new Vector3DE(-1f, -1f, -1f, -1f);
    }

    protected void TrackFilament(GCode gcode)
    {
      if (!gcode.hasG || !gcode.hasE || gcode.G != (ushort) 0 && gcode.G != (ushort) 1)
      {
        return;
      }

      if (MyPrinterInfo.extruder.inRelativeMode == Trilean.False)
      {
        MyPrinterInfo.filament_info.estimated_filament_length_printed = beginOfPrintE + gcode.E;
      }
      else
      {
        if (MyPrinterInfo.extruder.inRelativeMode != Trilean.True)
        {
          return;
        }

        MyPrinterInfo.filament_info.estimated_filament_length_printed += gcode.E;
        beginOfPrintE += gcode.E;
      }
    }

    protected void SetToHomeLocation()
    {
      MyPrinterInfo.extruder.position.pos.x = MyPrinterProfile.PrinterSizeConstants.HomeLocation.x;
      MyPrinterInfo.extruder.position.pos.y = MyPrinterProfile.PrinterSizeConstants.HomeLocation.y;
      if (!float.IsNaN(MyPrinterProfile.PrinterSizeConstants.HomeLocation.z))
      {
        MyPrinterInfo.extruder.position.pos.z = MyPrinterProfile.PrinterSizeConstants.HomeLocation.z;
        MyPrinterInfo.extruder.Z_Valid = true;
      }
      MyPrinterInfo.extruder.ishomed = Trilean.True;
    }

    protected GCode ConvertToSafeEmptyGCode(GCode gcode)
    {
      var gcode1 = new GCode("G4 S0");
      if (gcode.hasN)
      {
        gcode1.N = gcode.N;
      }

      return gcode1;
    }

    private byte[] TranslateGCode(GCode gcode)
    {
      if (gcode.hasG && gcode.G == (ushort) 28 && MyPrinterInfo.extruder.Z_Valid)
      {
        VerifyHomingPosition(ref gcode);
      }

      TrackExtruderPosition(gcode);
      TrackFilament(gcode);
      LastGCodeType = RequestGCode.NonRequestGCode;
      if (!gcode.hasM || gcode.M != (ushort) 5321 && gcode.M != (ushort) 619 && gcode.M != (ushort) 618 && (gcode.M != (ushort) 115 || gcode.hasS && gcode.S != 628))
      {
        var flag = false;
        if (!gcode.hasN || flag || !IsPrinting)
        {
          WriteLog("<< " + gcode.getAscii(true, false), Logger.TextType.Write);
        }
      }
      else
      {
        LastGCodeType = RequestGCode.HiddenType;
      }

      if (gcode.hasM && gcode.M != (ushort) 0)
      {
        if (MyPrinterProfile.VirtualCodes.ProcessVirtualCode(gcode, this))
        {
          if (!gcode.hasN)
          {
            return (byte[]) null;
          }

          var n = gcode.N;
          gcode = new GCode("G4 S0")
          {
            N = n
          };
        }
        if (gcode.M == (ushort) 117)
        {
          LastGCodeType = RequestGCode.M117GetInternalState;
        }
        else if (gcode.M == (ushort) 114)
        {
          LastGCodeType = RequestGCode.M114GetExtruderLocation;
          ExtruderDetails.position.Reset();
        }
        else if (gcode.M == (ushort) 576)
        {
          LastGCodeType = RequestGCode.M576GetFilamentInfo;
        }
        else if (gcode.M == (ushort) 618)
        {
          var s = gcode.S;
          var p = gcode.P;
          EepromAddressInfo infoFromLocation = MyPrinterProfile.EEPROMConstants.GetEepromInfoFromLocation(s);
          if (infoFromLocation != null)
          {
            eeprom_mapping.SetValue(infoFromLocation, p);
            CheckUpdatedPTValue((int) infoFromLocation.EepromAddr);
          }
        }
        else if ((gcode.M == (ushort) 109 || gcode.M == (ushort) 116) && (!IsPrinting && !IsPausedorPausing) && !IsRecovering)
        {
          Status = PrinterStatus.Firmware_WarmingUp;
        }
      }
      if (gcode.hasG)
      {
        if (gcode.G == (ushort) 30 || gcode.G == (ushort) 32 || gcode.G == (ushort) 33)
        {
          var flag1 = true;
          var flag2 = true;
          if (MyPrinterInfo.supportedFeatures.UsesSupportedFeatures)
          {
            var featureSlot1 = MyPrinterProfile.SupportedFeaturesConstants.GetFeatureSlot("Multi Point Automatic Bed Calibration");
            var featureSlot2 = MyPrinterProfile.SupportedFeaturesConstants.GetFeatureSlot("Single Point Bed Height Calibration");
            if (featureSlot1 >= 0)
            {
              flag1 = (uint)MyPrinterInfo.supportedFeatures.GetStatus(featureSlot1) > 0U;
            }

            if (featureSlot2 >= 0)
            {
              flag2 = (uint)MyPrinterInfo.supportedFeatures.GetStatus(featureSlot2) > 0U;
            }
          }
          if (gcode.G == (ushort) 32 && !flag1)
          {
            gcode = ConvertToSafeEmptyGCode(gcode);
            BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.MultiPointCalibrationNotSupported, MySerialNumber, (string) null).Serialize());
          }
          else if (gcode.G == (ushort) 30 && !flag2)
          {
            gcode = ConvertToSafeEmptyGCode(gcode);
            BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.SinglePointCalibrationNotSupported, MySerialNumber, (string) null).Serialize());
          }
          else if (Status != PrinterStatus.Firmware_Printing)
          {
            LastGCodeType = RequestGCode.G30_32_30_ZSaveGcode;
            ExtruderDetails.position.Reset();
            ClearCalibrationErrors();
            if (gcode.G == (ushort) 30)
            {
              SetOffsetInformationZOnly(0.0f);
              RequestUpdatedG32ValuesFromFirmware();
              var num = (int)WriteManualCommands("M117", "M578");
            }
            else if (gcode.G == (ushort) 32)
            {
              SetOffsetInformation(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, false);
              RequestUpdatedG32ValuesFromFirmware();
              var num = (int)WriteManualCommands("M117", "M578");
            }
          }
          if (gcode.G != (ushort) 33 && !IsRecovering)
          {
            Status = PrinterStatus.Firmware_Calibrating;
          }
        }
        if (gcode.G == (ushort) 28 && PrinterStatus.Firmware_PowerRecovery != Status)
        {
          Status = PrinterStatus.Firmware_Homing;
        }
      }
      if (HardwareDetails.repetier_protocol == 0)
      {
        return mAsciiEncoder.GetBytes(gcode.getAscii(true, false) + "\r\n");
      }

      return gcode.getBinary(HardwareDetails.repetier_protocol);
    }

    protected void SendNextCommand()
    {
      var gcode1 = (GCode) null;
      if (ResendLastCommand)
      {
        gcode1 = m_ogcLastGCodeSent;
      }
      else
      {
        var gcode2 = (GCode) null;
        lock (manual_commands)
        {
          if (LastMessageClear)
          {
            if (manual_commands.Count > 0)
            {
              gcode2 = manual_commands.RemoveFromFront();
            }
          }
        }
        if (gcode2 != null)
        {
          gcode1 = gcode2;
        }
        else if (!IsPaused && m_oJobController.HasJob && (m_oJobController.Printing && LastMessageClear))
        {
          gcode1 = m_oJobController.GetNextGCode();
          if (gcode1 != null)
          {
            LastMessageClear = false;
          }
        }
      }
      if (gcode1 == null)
      {
        return;
      }

      if (!gcode1.hasN)
      {
        CheckForPluginCommand(gcode1);
      }

      byte[] command = TranslateGCode(gcode1);
      if (command == null || command.Length == 0)
      {
        return;
      }

      PrinterIdle = false;
      if (!WriteToSerial(command))
      {
        OnUnexpectedDisconnect(MessageType.UnexpectedDisconnect);
        WriteLog("Disconnected because of a serial write error", Logger.TextType.Error);
      }
      else
      {
        m_nWaitCounter = 0;
        LastMessageClear = false;
        m_ogcLastGCodeSent = gcode1;
        oswResendTimer.Reset();
        oswResendTimer.Start();
        ResendLastCommand = false;
      }
    }

    private void CheckPauseTimer()
    {
      if (Status != PrinterStatus.Firmware_PrintingPaused || !oswPauseTimer.IsRunning || oswPauseTimer.ElapsedMilliseconds <= 300000L)
      {
        return;
      }

      var num = (int)WriteManualCommands("G4 S0");
      oswPauseTimer.Restart();
    }

    public void PauseAllMoves()
    {
      if (Status != PrinterStatus.Firmware_IsWaitingToPause || !m_oJobController.Pause(out List<string> pause_gcode, KnownFilament))
      {
        return;
      }

      if (pause_gcode != null && pause_gcode.Count > 0)
      {
        var num = (int)WriteManualCommands(pause_gcode.ToArray());
      }
      Status = PrinterStatus.Firmware_PrintingPaused;
      PushFilamentUsedToPrinter();
    }

    public void KillJobs()
    {
      SendEmergencyStop();
    }

    public void AddPrintJob(string user, JobParams jobParam)
    {
      FirmwareController.NewJobData newJobData;
      newJobData.jobParam = jobParam;
      newJobData.user = user;
      try
      {
        newJobData.jobParam.VerifyOptionsWithPrinter((PrinterProfile)MyPrinterProfile, MyPrinterInfo);
      }
      catch (Exception ex)
      {
        var spoolerMessage = new SpoolerMessage(MessageType.JobNotStarted, MySerialNumber, "The print job can't be started because of incompatible options.");
        BroadcastServer.BroadcastMessage("");
        return;
      }
      ThreadPool.QueueUserWorkItem(new WaitCallback(AddJobWorker), (object) newJobData);
    }

    private void AddJobWorker(object data)
    {
      var newJobData = (FirmwareController.NewJobData) data;
      try
      {
        if (IsWorking || GetJobsCount() > 0)
        {
          BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.CantStartJobPrinterBusy, (string) null).Serialize());
        }
        else
        {
          AddJob(newJobData.jobParam, newJobData.user);
        }
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("Exception in SpoolerServer.AddJobWorker " + ex.Message, ex);
      }
    }

    private void AddJob(JobParams jobParams, string user)
    {
      if (m_oJobController.HasJob)
      {
        return;
      }

      if (jobParams.options.turn_on_fan_before_print)
      {
        var num1 = (int)WriteManualCommands("M106 S1");
      }
      if (!ExtruderDetails.Z_Valid && SpoolerServer.CHECK_BED_CALIBRATION)
      {
        BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.InvalidZ, MySerialNumber, (string) null).Serialize());
      }
      else if (!CalibrationDetails.Calibration_Valid && SpoolerServer.CHECK_BED_CALIBRATION)
      {
        BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.BedOrientationMustBeCalibrated, MySerialNumber, (string) null).Serialize());
      }
      else if (!PersistantDetails.GantryClipsRemoved && SpoolerServer.CHECK_GANTRY_CLIPS)
      {
        BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.CheckGantryClips, MySerialNumber, (string) null).Serialize());
      }
      else
      {
        JobController.Result result;
        try
        {
          result = m_oJobController.InitPrintJob(jobParams, user, 0UL);
        }
        catch (AbstractPreprocessedJob.PreprocessorException ex)
        {
          WriteLog(ex.Message, Logger.TextType.Error);
          BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.PrinterError, MySerialNumber, "Error in preprocessor: " + ex.Message).Serialize());
          var num2 = (int)m_oJobController.ForceShutdownNow();
          return;
        }
        SpoolerMessage spoolerMessage;
        switch (result)
        {
          case JobController.Result.Success:
            return;
          case JobController.Result.FAILED_OutOfBounds:
            spoolerMessage = new SpoolerMessage(MessageType.ModelOutOfPrintableBounds, MySerialNumber, jobParams.jobname);
            break;
          case JobController.Result.FAILED_IncompatibleFilament:
            spoolerMessage = new SpoolerMessage(MessageType.SDPrintIncompatibleFilament, MySerialNumber, jobParams.jobname);
            break;
          default:
            spoolerMessage = new SpoolerMessage(MessageType.JobNotStarted, MySerialNumber, jobParams.jobname);
            break;
        }
        BroadcastServer.BroadcastMessage(spoolerMessage.Serialize());
        var num3 = (int)m_oJobController.ForceShutdownNow();
      }
    }

    internal void ConnectToActiveSDPrint()
    {
      if (m_oJobController.HasJob)
      {
        return;
      }

      m_oJobController.ConnectToRunningSDPrint("");
      Status = PrinterStatus.Firmware_Printing;
    }

    internal CommandResult RecoverySpoolerPrintCallback(JobParams jobParams, ulong ulLineToSkipTo)
    {
      if (m_oJobController.HasJob)
      {
        return CommandResult.Failed_ThePrinterIsPrintingOrPaused;
      }

      return m_oJobController.RecoverySpoolerPrintCallback(jobParams, ulLineToSkipTo);
    }

    private void SetActionOnRestart(PersistantData.RestartAction restart_action, int param)
    {
      PersistantDetails.MyRestartAction = restart_action;
      PersistantDetails.RestartActionParam = param;
      SavePersistantData();
    }

    public void GotoBootloader()
    {
      lock (threadsync)
      {
        broadcast_shutdown = false;
        cur_special_message = FirmwareController.SpecialMessage.GoToBootloader;
        mSpecialMessageSent = false;
        StopJobInternal();
      }
      SetActionOnRestart(PersistantData.RestartAction.ForceStayBootloader, 0);
    }

    public override void UpdateFirmware()
    {
      GotoBootloader();
      SetActionOnRestart(PersistantData.RestartAction.ForceUpdateFirmware, 0);
    }

    public override void SetFanConstants(FanConstValues.FanType fanType)
    {
      GotoBootloader();
      SetActionOnRestart(PersistantData.RestartAction.SetFan, (int) fanType);
    }

    public override void SetExtruderCurrent(ushort current)
    {
      GotoBootloader();
      SetActionOnRestart(PersistantData.RestartAction.SetExtruderCurrent, (int) current);
    }

    public void SetOffsetInformation(BedOffsets Off)
    {
      SetOffsetInformation(Off, true);
    }

    public void SetOffsetInformation(BedOffsets Off, bool check)
    {
      SetOffsetInformation(Off.BL, Off.BR, Off.FR, Off.FL, Off.ZO, check);
    }

    private void SetOffsetInformation(float BL, float BR, float FR, float FL, float ZO, bool check)
    {
      if (check)
      {
        if ((double) Math.Abs(CalibrationDetails.CORNER_HEIGHT_BACK_LEFT_OFFSET - BL) < 1.40129846432482E-45 && (double) Math.Abs(CalibrationDetails.CORNER_HEIGHT_BACK_RIGHT_OFFSET - BR) < 1.40129846432482E-45 && ((double) Math.Abs(CalibrationDetails.CORNER_HEIGHT_FRONT_LEFT_OFFSET - FL) < 1.40129846432482E-45 && (double) Math.Abs(CalibrationDetails.CORNER_HEIGHT_FRONT_RIGHT_OFFSET - FR) < 1.40129846432482E-45) && (double) Math.Abs(CalibrationDetails.ENTIRE_Z_HEIGHT_OFFSET - ZO) < 1.40129846432482E-45)
        {
          return;
        }

        CalibrationDetails.CORNER_HEIGHT_BACK_LEFT_OFFSET = BL;
        CalibrationDetails.CORNER_HEIGHT_BACK_RIGHT_OFFSET = BR;
        CalibrationDetails.CORNER_HEIGHT_FRONT_LEFT_OFFSET = FL;
        CalibrationDetails.CORNER_HEIGHT_FRONT_RIGHT_OFFSET = FR;
        CalibrationDetails.ENTIRE_Z_HEIGHT_OFFSET = ZO;
      }
      var num = (int)WriteManualCommands(PrinterCompatibleString.Format("M577 X{0} Y{1} Z{2} E{3} F{4}", (object) BL, (object) BR, (object) FR, (object) FL, (object) ZO), "M578");
    }

    public void SetOffsetInformationZOnly(float ZO)
    {
      CalibrationDetails.ENTIRE_Z_HEIGHT_OFFSET = ZO;
      var num = (int)WriteManualCommands(PrinterCompatibleString.Format("M577 F{0}", (object) ZO), "M578");
    }

    public bool SetCalibrationOffset(float offset)
    {
      if (!CalibrationDetails.UsesCalibrationOffset)
      {
        return false;
      }

      CalibrationDetails.CALIBRATION_OFFSET = offset;
      var num = (int)WriteManualCommands(PrinterCompatibleString.Format("M577 I{0}", (object) offset), "M578");
      return true;
    }

    public void SetBacklashValues(BacklashSettings backlash)
    {
      var num = (int)WriteManualCommands(PrinterCompatibleString.Format("M571 X{0} Y{1}", (object) backlash.backlash_x, (object) backlash.backlash_y), PrinterCompatibleString.Format("M580 X{0}", (object) backlash.backlash_speed), "M572", "M581");
    }

    public void PrintBacklashPrint(string user)
    {
      FilamentSpool.TypeEnum filamentType = KnownFilament.filament_type;
      var filamentProfile = FilamentProfile.CreateFilamentProfile(KnownFilament, (PrinterProfile)MyPrinterProfile);
      AddJob(new JobParams("backlash_calibration.gcode", "Spooler Inserted Job", "null", filamentType, 0.0f, 0.0f)
      {
        options = {
          dont_use_preprocessors = false,
          autostart_ignorewarnings = true
        },
        preprocessor = filamentProfile.preprocessor,
        filament_temperature = filamentProfile.Temperature
      }, user);
    }

    public void SetFilamentInformation(FilamentSpool filament, bool check = true)
    {
      if (check && KnownFilament == filament)
      {
        return;
      }

      KnownFilament.estimated_filament_length_printed = filament.estimated_filament_length_printed;
      KnownFilament.filament_color_code = filament.filament_color_code;
      KnownFilament.filament_location = filament.filament_location;
      KnownFilament.filament_size = filament.filament_size;
      KnownFilament.filament_temperature = filament.filament_temperature;
      KnownFilament.filament_type = filament.filament_type;
      KnownFilament.filament_uid = filament.filament_uid;
      var filamentType = (int) filament.filament_type;
      var num1 = filament.filament_temperature - 100;
      var filamentColorCode = (int) filament.filament_color_code;
      var filamentLengthPrinted = filament.estimated_filament_length_printed;
      var num2 = filamentType + (int) filament.filament_location * 64;
      var filamentSize = (int) filament.filament_size;
      var num3 = (int)WriteManualCommands(PrinterCompatibleString.Format("M575 P{0} S{1} E{2} T{3} I{4}", (object) num2, (object) filamentColorCode, (object) filamentLengthPrinted, (object) num1, (object) filamentSize));
      SetFilamentUID(filament.filament_uid);
    }

    public void PushFilamentUsedToPrinter()
    {
      FilamentSpool currentFilament = GetCurrentFilament();
      if (!((FilamentSpool) null != currentFilament) || currentFilament.filament_type == FilamentSpool.TypeEnum.NoFilament)
      {
        return;
      }

      SetFilamentInformation(GetCurrentFilament(), false);
    }

    public void SetFilamentUID(uint filamentUID)
    {
      var num = (int)WriteManualCommands(PrinterCompatibleString.Format("M570 P{0}", (object) BitConverter.ToInt32(BitConverter.GetBytes(filamentUID), 0)), "M576");
    }

    public FilamentSpool GetCurrentFilament()
    {
      return new FilamentSpool(KnownFilament);
    }

    private FilamentSpool KnownFilament
    {
      get
      {
        return MyPrinterInfo.filament_info;
      }
    }

    internal void SetBacklash(float X, float Y)
    {
      CalibrationDetails.BACKLASH_X = X;
      CalibrationDetails.BACKLASH_Y = Y;
      eeprom_mapping.SetFloat("BacklashX", X);
      eeprom_mapping.SetFloat("BacklashY", Y);
    }

    internal void SetBacklashSpeed(float F)
    {
      CalibrationDetails.BACKLASH_SPEED = F;
      eeprom_mapping.SetFloat("BacklashSpeed", F);
    }

    public CommandResult SetNozzleWidth(int iNozzleWidthMicrons)
    {
      if (!MyPrinterProfile.AccessoriesConstants.NozzleConstants.bHasInterchangeableNozzle)
      {
        return CommandResult.Failed_FeatureNotAvailableOnPrinter;
      }

      var num = (int)WriteManualCommands(string.Format("M582 S{0}", (object) iNozzleWidthMicrons));
      return CommandResult.Success;
    }

    public void SetToReady()
    {
      if (m_oJobController.HasJob && m_oJobController.Status == JobStatus.Paused)
      {
        Status = PrinterStatus.Firmware_PrintingPaused;
      }
      else
      {
        Status = PrinterStatus.Firmware_Ready;
      }
    }

    public override bool IsWorking
    {
      get
      {
        var num = 0;
        lock (manual_commands)
        {
          num = manual_commands.Count;
        }

        if (num > 0 || HasActiveJob)
        {
          return true;
        }

        if (!LastMessageClear)
        {
          return !m_bDoStartupOnWait;
        }

        return false;
      }
    }

    public override bool HasActiveJob
    {
      get
      {
        return m_oJobController.HasJob;
      }
    }

    public override bool IsPrinting
    {
      get
      {
        lock (threadsync)
        {
          return m_oJobController.HasJob && MyPrinterInfo.Status != PrinterStatus.Firmware_PrintingPaused;
        }
      }
    }

    public bool IsConnecting
    {
      get
      {
        return Status == PrinterStatus.Connecting;
      }
    }

    private bool IsRecovering
    {
      get
      {
        return PrinterStatus.Firmware_PowerRecovery == Status;
      }
    }

    public override int GetJobsCount()
    {
      lock (threadsync)
      {
        return m_oJobController.HasJob ? 1 : 0;
      }
    }

    private bool LastMessageClear
    {
      get
      {
        return _last_message_clear.Value;
      }
      set
      {
        _last_message_clear.Value = value;
        if (!value)
        {
          if (Status == PrinterStatus.Firmware_PrintingPaused)
          {
            Status = PrinterStatus.Firmware_PrintingPausedProcessing;
            oswPauseTimer.Stop();
          }
          else
          {
            if (IsPrinting || IsRecovering || Status != PrinterStatus.Firmware_Ready && Status != PrinterStatus.Firmware_Idle)
            {
              return;
            }

            Status = PrinterStatus.Firmware_Executing;
          }
        }
        else
        {
          if (IsConnecting)
          {
            return;
          }

          _auto_resend_count.Value = 0;
          if (Status == PrinterStatus.Firmware_Calibrating)
          {
            ClearCalibrationErrors();
          }

          if (IsPausedorPausing || IsRecovering)
          {
            return;
          }

          Status = IsPrinting ? PrinterStatus.Firmware_Printing : PrinterStatus.Firmware_Ready;
        }
      }
    }

    private bool ResendLastCommand
    {
      get
      {
        return _resend_last_command.Value;
      }
      set
      {
        _resend_last_command.Value = value;
      }
    }

    private void ClearCalibrationErrors()
    {
      CalibrationDetails.Calibration_Valid = true;
      ExtruderDetails.Z_Valid = true;
      CalibrationDetails.G32_VERSION = (int) byte.MaxValue;
    }

    public void SetRepetierProtocol(int protocol)
    {
      HardwareDetails.repetier_protocol = protocol;
    }

    public override bool Idle
    {
      get
      {
        return PrinterIdle;
      }
    }

    private bool PrinterIdle
    {
      get
      {
        return _printer_idle.Value;
      }
      set
      {
        _printer_idle.Value = value;
      }
    }

    public PrinterInfo CurrentPrinterInfo
    {
      get
      {
        return new PrinterInfo(MyPrinterInfo);
      }
    }

    public bool BoundsCheckingEnabled
    {
      get
      {
        return __boundsCheckingEnabled.Value;
      }
      set
      {
        __boundsCheckingEnabled.Value = value;
      }
    }

    public EEPROMMapping EEPROM
    {
      get
      {
        return eeprom_mapping;
      }
    }

    public ScriptCallback OnGotUpdatedPosition { get; set; }

    private enum SpecialMessage
    {
      None,
      GetPosition_M114,
      EmergencyStop_M0,
      HaltAllMoves_M1,
      GoToBootloader,
    }

    protected struct NewJobData
    {
      public JobParams jobParam;
      public string user;
    }
  }
}
