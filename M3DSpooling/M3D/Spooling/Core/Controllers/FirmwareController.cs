// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Core.Controllers.FirmwareController
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
      this.m_odRegisteredPlugins = new ConcurrentDictionary<string, FirmwareControllerPlugin>();
      this.m_odLinkedPluginGCodes = new ConcurrentDictionary<string, string>();
      this.m_oJobController = new JobController((IPublicFirmwareController) this);
      this.eeprom_mapping = new EEPROMMapping(printerProfile.EEPROMConstants);
      this.read_accumulator = initial_read_accumulator;
      this.Status = PrinterStatus.Connecting;
      this.ExtruderDetails.Temperature = -273f;
      this.ExtruderDetails.iNozzleSizeMicrons = this.MyPrinterProfile.AccessoriesConstants.NozzleConstants.iDefaultNozzleSizeMicrons;
      this.CalibrationDetails.Calibration_Valid = true;
      this.ExtruderDetails.Z_Valid = true;
    }

    public override void Shutdown()
    {
      if (this.m_oJobController != null)
      {
        int num = (int) this.m_oJobController.ForceShutdownNow();
      }
      base.Shutdown();
    }

    private uint ValidateFirmwareVersion(uint version)
    {
      if (version == uint.MaxValue)
        return 0;
      for (uint index = 0; index < 16U; ++index)
      {
        if ((int) version == (int) index * 16777216)
          return 0;
      }
      return version;
    }

    public void OnUnexpectedDisconnect(MessageType reason)
    {
      if (!this.mSpecialMessageSent)
        this.BroadcastServer.BroadcastMessage(new SpoolerMessage(reason, this.MySerialNumber, "null").Serialize());
      this.Status = PrinterStatus.Error_PrinterNotAlive;
    }

    public void vStopwatchReset()
    {
      if (this.oswStopwatchGeneralUse == null)
        return;
      this.oswStopwatchGeneralUse.Reset();
      this.oswStopwatchGeneralUse.Start();
    }

    public long nStopwatchReturnTime()
    {
      long num = -1;
      if (this.oswStopwatchGeneralUse != null && this.oswStopwatchGeneralUse.IsRunning)
      {
        this.oswStopwatchGeneralUse.Stop();
        num = this.oswStopwatchGeneralUse.ElapsedMilliseconds;
      }
      return num;
    }

    public PluginResult RegisterPlugin(FirmwareControllerPlugin plugin)
    {
      if (!this.m_odRegisteredPlugins.TryAdd(plugin.ID, plugin))
        return PluginResult.FAILED_PluginIDAlreadyInUse;
      plugin.RegisterGCodes((IGCodePluginable) this);
      return PluginResult.Success;
    }

    public PluginResult LinkGCodeWithPlugin(string gcode, string pluginID)
    {
      string upperInvariant = gcode.ToUpperInvariant();
      GCode gcode1 = new GCode(upperInvariant);
      if (!gcode1.hasG && !gcode1.hasM)
        return PluginResult.FAILED_NotaValidMorGCode;
      if (this.m_odLinkedPluginGCodes.ContainsKey(upperInvariant))
      {
        if (this.m_odLinkedPluginGCodes[upperInvariant] != pluginID)
          return PluginResult.FAILED_GCODEAlreadyRegistered;
      }
      else if (!this.m_odLinkedPluginGCodes.TryAdd(upperInvariant, pluginID))
        return PluginResult.FAILED_GCODEAlreadyRegistered;
      return PluginResult.Success;
    }

    public CommandResult RegisterExternalPluginGCodes(string pluginID, string[] gCodeList)
    {
      foreach (string gCode in gCodeList)
      {
        int num = (int) this.LinkGCodeWithPlugin(gCode, pluginID);
      }
      return CommandResult.Success;
    }

    private void ReadForPlugin(string receivedData)
    {
      if (!string.IsNullOrEmpty(this.mPluginResultAccumulator))
        this.mPluginResultAccumulator += "\n";
      this.mPluginResultAccumulator += receivedData;
      if (!receivedData.StartsWith("ok"))
        return;
      if (this.m_odRegisteredPlugins.ContainsKey(this.mPluginToReceiveCommand))
        this.m_odRegisteredPlugins[this.mPluginToReceiveCommand].ProcessGCodeResult(this.mPluginGCodeSent, this.mPluginResultAccumulator, this.MyPrinterInfo);
      else
        this.base_printer.BroadcastPluginMessage(new SpoolerMessage(MessageType.PluginMessage, this.MySerialNumber, this.mPluginResultAccumulator, this.mPluginToReceiveCommand, this.mPluginGCodeSent.getAscii(false, false)));
      this.mPluginToReceiveCommand = (string) null;
      this.mPluginGCodeSent = (GCode) null;
    }

    private void CheckForPluginCommand(GCode manual_gcode)
    {
      this.mPluginToReceiveCommand = (string) null;
      this.mPluginGCodeSent = (GCode) null;
      this.mPluginResultAccumulator = "";
      string key = (string) null;
      if (manual_gcode.hasM)
        key = PrinterCompatibleString.Format("M{0}", (object) manual_gcode.M);
      else if (manual_gcode.hasG)
        key = PrinterCompatibleString.Format("G{0}", (object) manual_gcode.G);
      if (string.IsNullOrEmpty(key) || !this.m_odLinkedPluginGCodes.ContainsKey(key))
        return;
      this.mPluginToReceiveCommand = this.m_odLinkedPluginGCodes[key];
      this.mPluginGCodeSent = manual_gcode;
    }

    public override void DoConnectionLogic()
    {
      if (this.Status == PrinterStatus.Bootloader_FirmwareUpdateFailed)
      {
        this.OnFirmwareUpdateFailed();
      }
      else
      {
        this.ProcessReads();
        this.DoWrites();
        this.base_printer.RequestFastSerialProcessing(this.IsPrinting && this.m_oJobController.Info.Status == JobStatus.SavingToSD);
        if (!this.m_oJobController.Printing)
          return;
        if (this.Status == PrinterStatus.Firmware_IsWaitingToPause && this.m_oJobController.MinutesElapsed > 5.0)
          this.PauseAllMoves();
        if (this.oswRefreshTimer.Elapsed.Minutes > 10)
        {
          this.UpdateStatsTime(this.m_oJobController.GetElapsedReset);
          this.oswRefreshTimer.Reset();
          this.oswRefreshTimer.Start();
        }
        if (!this.IsPaused)
          return;
        this.CheckPauseTimer();
      }
    }

    private void OnFirmwareUpdateFailed()
    {
      this.BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.FirmwareErrorCyclePower, this.MySerialNumber, (string) null).Serialize());
    }

    private string ReadDataFromSerial()
    {
      try
      {
        try
        {
          this.read_accumulator += this.ReadExisting();
        }
        catch (TimeoutException ex)
        {
        }
        catch (Exception ex)
        {
          ErrorLogger.LogException("Exception in FirmwareConnection.ReadDataFromSerial " + ex.Message, ex);
        }
        this.read_accumulator = this.read_accumulator.Replace('\r', '\n');
      }
      catch (InvalidOperationException ex)
      {
        this.Status = PrinterStatus.Error_PrinterNotAlive;
      }
      catch (Exception ex)
      {
        ErrorLogger.LogErrorMsg("Exception in FirmwareConnection.ReadDataFromSerial " + ex.Message, "Exception");
      }
      if (!this.read_accumulator.Contains("\n"))
        return "";
      int length = this.read_accumulator.IndexOf('\n');
      string str = this.read_accumulator.Substring(0, length);
      this.read_accumulator = this.read_accumulator.Substring(length + 1);
      return str;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessReads()
    {
      try
      {
        string str1 = this.ReadDataFromSerial();
        Logger.TextType type = Logger.TextType.Read;
        if (string.IsNullOrEmpty(str1))
          return;
        if (str1.StartsWith("ok", StringComparison.InvariantCulture))
        {
          if (!string.IsNullOrEmpty(this.mPluginToReceiveCommand))
            this.ReadForPlugin(str1);
          int line_number = this.ProcessParameters(str1.Substring(2));
          if (this.LastGCodeType == RequestGCode.HiddenType)
            type = Logger.TextType.None;
          this.ProcessOK(line_number);
          if (this.LastGCodeType == RequestGCode.G30_32_30_ZSaveGcode)
          {
            int num = (int) this.WriteManualCommands("M117", "M573");
          }
        }
        else if (str1.StartsWith("rs", StringComparison.InvariantCulture))
          this.ProcessResend(this.ProcessParameters(str1.Substring(2)));
        else if (str1.StartsWith("skip", StringComparison.InvariantCulture))
          this.ProcessOK(this.ProcessParameters(str1.Substring(4)));
        else if (str1.StartsWith("wait", StringComparison.InvariantCulture))
        {
          if (!this.ProcessWait())
          {
            this.WriteLog(">> Error: Firmware became idle while waiting for a command", Logger.TextType.Feedback);
            type = Logger.TextType.None;
          }
          else
            type = Logger.TextType.Wait;
        }
        else if (str1 == "UNABLE TO INIT HARDWARE. CHECK MICRO MOTION CABLE CONNECTION.")
          this.BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.MicroMotionControllerFailed, this.MySerialNumber, (string) null).Serialize());
        else if (str1.Contains("?"))
          this.GotoBootloader();
        else if (str1.StartsWith("HF:", StringComparison.InvariantCulture))
        {
          if (this.m_oJobController.HasJob)
            this.StopJobInternal();
          this.clearAllBlockers();
          this.WriteLog(">> Error: Heater failed", Logger.TextType.Error);
          this.BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.PrinterError, this.MySerialNumber, "Error: Heater failed").Serialize());
          type = Logger.TextType.None;
        }
        else if (str1.StartsWith("Error", StringComparison.InvariantCulture) || str1.StartsWith("!!", StringComparison.InvariantCulture))
        {
          if (this.m_oJobController.HasJob)
            this.StopJobInternal();
          this.clearAllBlockers();
          int error_code;
          string str2 = ">> " + ErrorProcessing.TranslateError(str1, out error_code);
          if (!string.IsNullOrEmpty(str2))
          {
            this.WriteLog(str2, Logger.TextType.Error);
            SpoolerMessage spoolerMessage;
            if (error_code == 1008)
            {
              this.ExtruderDetails.Temperature = 0.0f;
              spoolerMessage = new SpoolerMessage(MessageType.PrinterTimeout, this.MySerialNumber, (string) null);
            }
            else
              spoolerMessage = new SpoolerMessage(MessageType.PrinterError, this.MySerialNumber, str2);
            if (spoolerMessage != null)
              this.BroadcastServer.BroadcastMessage(spoolerMessage.Serialize());
          }
          type = Logger.TextType.None;
        }
        else if (str1.StartsWith("e", StringComparison.InvariantCulture))
        {
          string str2 = this.ProcessSoftError(str1);
          if (!string.IsNullOrEmpty(str2))
            this.WriteLog(">> " + str2, Logger.TextType.Feedback);
          type = Logger.TextType.None;
        }
        else if (str1.StartsWith("start", StringComparison.InvariantCulture))
          this.clearAllBlockers();
        else if (str1.StartsWith("T:", StringComparison.InvariantCulture) || str1.StartsWith("B:", StringComparison.InvariantCulture))
        {
          type = Logger.TextType.Feedback;
          this.ParseTemperatureInfo(str1);
        }
        else if (!string.IsNullOrEmpty(this.mPluginToReceiveCommand))
          this.ReadForPlugin(str1);
        if (type == Logger.TextType.None || !(!this.IsPrinting | false) && type != Logger.TextType.Error && type != Logger.TextType.Feedback)
          return;
        this.WriteLog(">> " + str1, type);
      }
      catch (ThreadAbortException ex)
      {
        throw ex;
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("Exception in FirmwareConnection.ProcessREads 1 " + ex.Message, ex);
        this.WriteLog(string.Format("FirmwareConnection.ProcessReads 1 Exception: {0}", (object) ex.Message), Logger.TextType.Error);
      }
    }

    private int ProcessParameters(string parameters)
    {
      int result1 = int.MinValue;
      try
      {
        bool flag1 = false;
        bool flag2 = false;
        int result2 = 0;
        int result3 = 0;
        if (string.IsNullOrEmpty(parameters))
          return int.MinValue;
        parameters = parameters.Trim();
        if (parameters.Length < 1)
          return int.MinValue;
        int length = parameters.IndexOf(' ');
        if (int.TryParse(length >= 0 ? parameters.Substring(0, length) : parameters, out result1))
          parameters = length <= 0 ? (string) null : parameters.Substring(length + 1);
        else
          result1 = int.MinValue;
        if (this.LastGCodeType == RequestGCode.M114GetExtruderLocation)
          this.ExtruderDetails.position.Reset();
        string unprocessed = parameters;
        for (string nextToken = this.GetNextToken(ref unprocessed); !string.IsNullOrEmpty(nextToken); nextToken = this.GetNextToken(ref unprocessed))
        {
          string option = this.GetOption(nextToken);
          string parameter = this.GetParameter(nextToken);
          if (option == "ZV")
          {
            this.LastGCodeType = RequestGCode.M117GetInternalState;
            this.ParseZValid(parameter);
          }
          else if (option == "PT")
          {
            flag1 = true;
            if (!int.TryParse(parameter, out result2))
            {
              result2 = 0;
              ErrorLogger.LogErrorMsg("Exception in FirmwareConnection.ProcessParameters PT");
              this.WriteLog("Exception in FirmwareConnection.ProcessParameters PT", Logger.TextType.Error);
            }
          }
          else if (option == "DT")
          {
            flag1 = true;
            if (!int.TryParse(parameter, out result3))
            {
              result3 = 0;
              ErrorLogger.LogErrorMsg("Exception in FirmwareConnection.ProcessParameters DT");
              this.WriteLog("Exception in FirmwareConnection.ProcessParameters DT", Logger.TextType.Error);
            }
          }
          else if (option == "FIRMWARE_NAME")
          {
            if (!string.IsNullOrEmpty(parameter))
              this.HardwareDetails.firmware_name = parameter;
            if (this.Status == PrinterStatus.Connected || this.Status == PrinterStatus.Connecting)
            {
              this.oswResendTimer.Stop();
              this.oswResendTimer.Reset();
            }
          }
          else if (option == "FIRMWARE_VERSION")
          {
            if (!string.IsNullOrEmpty(parameter))
            {
              uint result4;
              if (uint.TryParse(parameter, out result4))
                this.HardwareDetails.firmware_version = result4;
              else
                this.HardwareDetails.firmware_version = 0U;
            }
            this.HardwareDetails.firmware_version = this.ValidateFirmwareVersion(this.HardwareDetails.firmware_version);
            if (SpoolerServer.CheckFirmware)
            {
              FirmwareDetails firmware = this.MyPrinterProfile.ProductConstants.FirmwareList['M'];
              if (SpoolerServer.CHECK_INCOMPATIBLE_FIRMWARE && (int) this.HardwareDetails.firmware_version != (int) firmware.firmware_version)
              {
                this.GotoBootloader();
                return result1;
              }
            }
            if (this.MyPrinterProfile.AccessoriesConstants.SDCardConstants.HasSDCard)
            {
              int num = (int) this.WriteManualCommands("M27");
            }
          }
          else if (option == "FIRMWARE_URL")
          {
            if (!string.IsNullOrEmpty(parameter))
              this.HardwareDetails.firmware_url = parameter;
          }
          else if (option == "PROTOCOL_VERSION")
          {
            if (!string.IsNullOrEmpty(parameter))
              this.HardwareDetails.protocol_version = parameter;
          }
          else if (option == "MACHINE_TYPE")
          {
            if (!string.IsNullOrEmpty(parameter))
              this.HardwareDetails.machine_type = parameter;
          }
          else if (option == "EXTRUDER_COUNT")
          {
            if (!string.IsNullOrEmpty(parameter))
            {
              int result4;
              if (int.TryParse(parameter, out result4))
                this.HardwareDetails.extruder_count = result4;
              else
                this.HardwareDetails.extruder_count = 1;
            }
          }
          else if (option == "REPETIER_PROTOCOL")
          {
            if (!string.IsNullOrEmpty(parameter))
            {
              int result4;
              if (int.TryParse(parameter, out result4))
                this.SetRepetierProtocol(result4);
              else
                this.SetRepetierProtocol(2);
            }
            if (this.HardwareDetails.repetier_protocol == 0)
              this.gcodetype = GCodeType.ASCII;
            else if (this.HardwareDetails.repetier_protocol == 1)
              this.gcodetype = GCodeType.BinaryV1;
            else if (this.HardwareDetails.repetier_protocol == 2)
              this.gcodetype = GCodeType.BinaryV2;
          }
          else if (option == "X-SERIAL_NUMBER")
          {
            this.MySerialNumber = new PrinterSerialNumber(parameter);
            this.logger.ResetWithSerialNumber(this.MySerialNumber.ToString());
            this.LoadPersistantData();
            this.UpdateStatsOnPrinter();
          }
          else if (option == "E")
          {
            if (this.LastGCodeType == RequestGCode.M114GetExtruderLocation)
            {
              float result4;
              if (float.TryParse(parameter, NumberStyles.Float, (IFormatProvider) PrinterCompatibleString.NUMBER_FORMAT, out result4))
                this.ExtruderDetails.position.e = result4;
              flag2 = true;
            }
          }
          else if (option == "X")
          {
            if (this.LastGCodeType == RequestGCode.M114GetExtruderLocation)
            {
              float result4;
              if (float.TryParse(parameter, NumberStyles.Float, (IFormatProvider) PrinterCompatibleString.NUMBER_FORMAT, out result4))
                this.ExtruderDetails.position.pos.x = result4;
              flag2 = true;
            }
          }
          else if (option == "Y")
          {
            if (this.LastGCodeType == RequestGCode.M114GetExtruderLocation)
            {
              float result4;
              if (float.TryParse(parameter, NumberStyles.Float, (IFormatProvider) PrinterCompatibleString.NUMBER_FORMAT, out result4))
                this.ExtruderDetails.position.pos.y = result4;
              this.ExtruderDetails.ishomed = Trilean.True;
              flag2 = true;
            }
          }
          else if (option == "Z")
          {
            if (this.LastGCodeType == RequestGCode.M114GetExtruderLocation)
            {
              float result4;
              if (float.TryParse(parameter, NumberStyles.Float, (IFormatProvider) PrinterCompatibleString.NUMBER_FORMAT, out result4))
                this.ExtruderDetails.position.pos.z = result4;
              flag2 = true;
            }
          }
          else if (option == "C0")
            this.SetGantryClips(true);
          else if (option == "C1")
          {
            this.SetGantryClips(false);
          }
          else
          {
            ushort result4;
            if (option == "RC" && ushort.TryParse(parameter, out result4))
            {
              this.HardwareDetails.LastResetCauseMask = result4;
              foreach (ResetCauseEnum resetCauseEnum in this.HardwareDetails.LastResetCause)
              {
                string str = resetCauseEnum.ToString();
                for (int startIndex = 1; startIndex < str.Length; ++startIndex)
                {
                  if (char.IsUpper(str[startIndex]))
                  {
                    str = str.Insert(startIndex, " ");
                    ++startIndex;
                  }
                }
                this.WriteLog(string.Format(" - {0}", (object) str), Logger.TextType.Write);
              }
            }
          }
        }
        if (flag1)
          this.ReadReturnedEEPROMData(result2, result3);
        if (flag2)
        {
          if (this.OnGotUpdatedPosition != null)
          {
            this.OnGotUpdatedPosition((IPublicFirmwareController) this, new PrinterInfo(this.MyPrinterInfo));
            this.OnGotUpdatedPosition = (ScriptCallback) null;
          }
        }
      }
      catch (Exception ex)
      {
        if (ex is ThreadAbortException)
          throw ex;
        ErrorLogger.LogException("Exception in FirmwareConnection.ProcessParameters 12 " + ex.Message, ex);
        this.WriteLog(string.Format("FirmwareConnection.ProcessParameters 10 Exception: {0}", (object) ex.Message), Logger.TextType.Error);
      }
      return result1;
    }

    private void ParseTemperatureInfo(string message)
    {
      foreach (Match match in new Regex("(\\w*):(\\d+\\.?\\d?)").Matches(message))
      {
        float result;
        if (!float.TryParse(match.Groups[2].Value, NumberStyles.Float, (IFormatProvider) PrinterCompatibleString.NUMBER_FORMAT, out result))
          result = -273f;
        if ((double) result > 500.0)
          result = 0.0f;
        string str = match.Groups[1].Value;
        if (!(str == "T"))
        {
          if (str == "B" && this.MyPrinterProfile.AccessoriesConstants.HeatedBedConstants.HasBuiltinHeatedBed)
            this.AccessoryDetails.BedStatus.BedTemperature = result;
        }
        else
          this.ExtruderDetails.Temperature = result;
      }
    }

    private void ParseZValid(string param)
    {
      int result;
      if (!int.TryParse(param, out result))
      {
        ErrorLogger.LogErrorMsg("Invalid ZValid flag from firmware.");
        this.WriteLog("Invalid ZValid flag from firmware.", Logger.TextType.Error);
        result = -1;
      }
      if (result < 0)
        return;
      this.ExtruderDetails.Z_Valid = (uint) result > 0U;
      if (this.ExtruderDetails.Z_Valid)
      {
        this.invalid_z_sent = false;
      }
      else
      {
        if (this.invalid_z_sent)
          return;
        this.BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.InvalidZ, this.MySerialNumber, (string) null).Serialize());
        this.invalid_z_sent = true;
      }
    }

    protected void UpdateStatsTime(float elapsed)
    {
      try
      {
        this.PersistantDetails.UnsavedPrintTime += elapsed;
        this.PersistantDetails.hours_used += elapsed;
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException(string.Format("PrinterConnection.UpdateStatsTime Exception: {0}", (object) ex.Message), ex);
        this.WriteLog(string.Format("PrinterConnection.UpdateStatsTime Exception: {0}", (object) ex.Message), Logger.TextType.Error);
      }
    }

    protected void UpdateStatsOnPrinter()
    {
      if ((double) this.PersistantDetails.UnsavedPrintTime > 0.0)
      {
        int num = (int) this.WriteManualCommands(PrinterCompatibleString.Format("M5321 X{0}", (object) this.PersistantDetails.UnsavedPrintTime));
        this.PersistantDetails.UnsavedPrintTime = 0.0f;
      }
      this.SavePersistantData();
    }

    public void SaveJobParamsToPersistantData(PersistantJobData parameters)
    {
      this.PersistantDetails.SavedJobInformation = parameters != null ? new PersistantJobData(parameters) : (PersistantJobData) null;
      this.SavePersistantData();
    }

    private void clearAllBlockers()
    {
      this.clearForWait();
      this.manual_commands.Clear();
    }

    private void clearForWait()
    {
      this.ResendLastCommand = false;
      this.LastMessageClear = true;
      this.mSpecialMessageSent = false;
    }

    private void ProcessResend(int line_number)
    {
      if (line_number < 0 && !this.m_ogcLastGCodeSent.hasN)
        this.ResendLastCommand = true;
      else if (line_number >= 0 && this.m_ogcLastGCodeSent.hasN)
      {
        int n = this.m_ogcLastGCodeSent.N;
        if (n == line_number)
          this.ResendLastCommand = true;
        else
          this.WriteLog(string.Format("Error::Invalid line number resending {0} expected {1}", (object) line_number, (object) n), Logger.TextType.Error);
      }
      else if (line_number >= 0)
        this.WriteLog(string.Format("Error::Unexpected line number received with resend."), Logger.TextType.Error);
      else
        this.WriteLog(string.Format("Error::Expected line number not received with resend."), Logger.TextType.Error);
    }

    private void ProcessOK(int line_number)
    {
      if (this.mSpecialMessageSent)
      {
        if (this.cur_special_message == FirmwareController.SpecialMessage.EmergencyStop_M0)
        {
          this.ExtruderDetails.Temperature = -273f;
          if (this.MyPrinterProfile.AccessoriesConstants.HeatedBedConstants.HasBuiltinHeatedBed)
            this.MyPrinterInfo.accessories.BedStatus.BedTemperature = -273f;
        }
        else if (this.cur_special_message == FirmwareController.SpecialMessage.GoToBootloader)
        {
          this.Status = PrinterStatus.Error_PrinterNotAlive;
          Thread.Sleep(1500);
        }
        if (this.cur_special_message == FirmwareController.SpecialMessage.EmergencyStop_M0 || this.cur_special_message == FirmwareController.SpecialMessage.HaltAllMoves_M1)
        {
          if (this.HasActiveJob && this.cur_special_message == FirmwareController.SpecialMessage.HaltAllMoves_M1)
            this.MoveToSafeLocation();
          this.StopJobInternal();
          this.cur_special_message = FirmwareController.SpecialMessage.None;
        }
        else
          this.cur_special_message = FirmwareController.SpecialMessage.None;
        this.mSpecialMessageSent = false;
        this.LastMessageClear = true;
        this.ResendLastCommand = false;
      }
      else
      {
        if ((!this.m_ogcLastGCodeSent.hasN || this.m_ogcLastGCodeSent.N != line_number) && (this.m_ogcLastGCodeSent.hasN || line_number > 0))
          return;
        this.ResendLastCommand = false;
        this.LastMessageClear = true;
      }
    }

    private bool ProcessWait()
    {
      bool flag = true;
      this.PrinterIdle = true;
      if (this.m_bDoStartupOnWait && !this.MyPrinterInfo.FirmwareIsInvalid)
      {
        this.LastMessageClear = true;
        this.MyPrinterProfile.Scripts.StartupScript((IPublicFirmwareController) this, new PrinterInfo(this.MyPrinterInfo));
        this.m_bDoStartupOnWait = false;
      }
      else if (!this.LastMessageClear)
      {
        if (this.m_nWaitCounter >= 10)
        {
          this.TryToAutoResend();
          this.m_nWaitCounter = 0;
          flag = false;
          if (this.IsConnecting)
            this.m_bDoStartupOnWait = true;
        }
        else
          ++this.m_nWaitCounter;
      }
      if (this.Status == PrinterStatus.Firmware_PrintingPausedProcessing)
      {
        this.Status = PrinterStatus.Firmware_PrintingPaused;
        this.oswPauseTimer.Restart();
      }
      if (!this.IsPausedorPausing && !this.IsPrinting && (!this.IsConnecting && !this.IsRecovering))
        this.Status = PrinterStatus.Firmware_Idle;
      return flag;
    }

    private string ProcessSoftError(string errorMsg)
    {
      int error_code;
      string str = ErrorProcessing.TranslateError(errorMsg, out error_code);
      this.TryToAutoResend();
      return str;
    }

    private void TryToAutoResend()
    {
      if (this.m_ogcLastGCodeSent != null && this.m_ogcLastGCodeSent.hasN && this.m_oJobController.HasJob)
      {
        if (++this._auto_resend_count.Value > 1)
        {
          GCode gcode = new GCode("G4 S0");
          if (this.m_ogcLastGCodeSent.hasN)
            gcode.N = this.m_ogcLastGCodeSent.N;
          this.m_ogcLastGCodeSent = gcode;
        }
        this.ResendLastCommand = true;
      }
      else
        this.clearAllBlockers();
    }

    private string GetNextToken(ref string unprocessed)
    {
      if (string.IsNullOrEmpty(unprocessed))
        return (string) null;
      string str = unprocessed.TrimStart();
      if (str.Length < 1)
        return (string) null;
      int num = str.IndexOf(' ');
      if (num > 0)
      {
        unprocessed = str.Substring(num);
        str = str.Substring(0, num);
      }
      else
        unprocessed = (string) null;
      return str;
    }

    private string GetOption(string token)
    {
      if (token.StartsWith("ok", StringComparison.InvariantCulture) || token.StartsWith("rs", StringComparison.InvariantCulture) || token.StartsWith("||", StringComparison.InvariantCulture))
        return token.Substring(0, 2);
      int length = token.IndexOf(':');
      if (length > 0)
        return token.Substring(0, length);
      return token;
    }

    private string GetParameter(string token)
    {
      if (token.StartsWith("ok", StringComparison.InvariantCulture) || token.StartsWith("rs", StringComparison.InvariantCulture) || token.StartsWith("||", StringComparison.InvariantCulture))
      {
        int num = token.IndexOf(' ');
        if (num > 0)
          return token.Substring(num + 1).Trim();
        return "";
      }
      int num1 = token.IndexOf(':');
      if (num1 > 0)
        return token.Substring(num1 + 1);
      return "";
    }

    public void RequestEEPROMMapping()
    {
      this.Status = PrinterStatus.Connecting;
      this.WriteLog("<-Loading Printer Data", Logger.TextType.Error);
      SortedList<int, EepromAddressInfo> allData = this.MyPrinterProfile.EEPROMConstants.GetAllData();
      int firmwareReadableEeprom = (int) this.MyPrinterProfile.EEPROMConstants.EndOfFirmwareReadableEEPROM;
      foreach (KeyValuePair<int, EepromAddressInfo> keyValuePair in allData)
      {
        EepromAddressInfo eepromAddressInfo = keyValuePair.Value;
        if ((int) eepromAddressInfo.EepromAddr <= firmwareReadableEeprom)
        {
          int num = (int) this.WriteManualCommands("M619 S" + (object) eepromAddressInfo.EepromAddr + " T" + (object) eepromAddressInfo.Size);
        }
      }
    }

    protected void ReadReturnedEEPROMData(int PT, int DT)
    {
      if (!this.eeprom_mapping.SetValue(this.MyPrinterProfile.EEPROMConstants.GetEepromInfoFromLocation(PT), DT))
        throw new IndexOutOfRangeException();
      this.CheckUpdatedPTValue(PT);
      if (PT < this.MyPrinterProfile.EEPROMConstants.PrinterReadyIndex || this.Status != PrinterStatus.Connecting)
        return;
      this.SetToReady();
      this.BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.PrinterConnected, this.MySerialNumber, "null").Serialize());
      this.WriteLog("->Printer Data Received", Logger.TextType.Error);
    }

    private void CheckUpdatedPTValue(int eepromAddress)
    {
      if (eepromAddress == this.eeprom_mapping.GetLocationFromName("BacklashY"))
        this.ProcessBacklashEEPROM();
      else if (eepromAddress == this.eeprom_mapping.GetLocationFromName("FilamentSize"))
        this.ProcessFilamentDataFromEEPROM();
      else if (eepromAddress == this.eeprom_mapping.GetLocationFromName("FilamentUID"))
        this.ProcessFilamentUIDFromEEPROM();
      else if (eepromAddress == this.eeprom_mapping.GetLocationFromName("ZCalibrationZO"))
        this.ProcessBedOffsetDataFromEEPROM();
      else if (eepromAddress == this.eeprom_mapping.GetLocationFromName("BedCompensationVersion"))
        this.ProcessBedCompensationDataFromEEPROM();
      else if (eepromAddress == this.eeprom_mapping.GetLocationFromName("BacklashSpeed"))
        this.ProcessBacklashSpeedEEPROM();
      else if (this.eeprom_mapping.HasKey("EnabledFeatures") && eepromAddress == this.eeprom_mapping.GetLocationFromName("EnabledFeatures"))
        this.ProcessEnabledFeatures();
      else if (this.eeprom_mapping.HasKey("CalibrationOffset") && eepromAddress == this.eeprom_mapping.GetLocationFromName("CalibrationOffset"))
      {
        this.ProcessCalibrationOffset();
      }
      else
      {
        if (!this.eeprom_mapping.HasKey("NozzleSizeExtrusionWidth") || eepromAddress != this.eeprom_mapping.GetLocationFromName("NozzleSizeExtrusionWidth"))
          return;
        this.ProcessNozzleSizeExtrusionWidth();
      }
    }

    public void ProcessEEPROMData()
    {
      this.ProcessFilamentDataFromEEPROM();
      this.ProcessFilamentUIDFromEEPROM();
      this.ProcessBedCompensationDataFromEEPROM();
      this.ProcessBedOffsetDataFromEEPROM();
      this.ProcessBacklashEEPROM();
      this.ProcessBacklashSpeedEEPROM();
      this.ProcessEnabledFeatures();
    }

    private void ProcessEnabledFeatures()
    {
      if (!this.eeprom_mapping.HasKey("EnabledFeatures"))
        return;
      this.printerInfo.supportedFeatures.FeaturesBitField = this.eeprom_mapping.GetUInt32("EnabledFeatures");
      this.printerInfo.supportedFeatures.UsesSupportedFeatures = true;
    }

    public void ProcessCalibrationOffset()
    {
      if (!this.eeprom_mapping.HasKey("CalibrationOffset"))
        return;
      this.printerInfo.calibration.CALIBRATION_OFFSET = this.eeprom_mapping.GetFloat("CalibrationOffset");
      this.printerInfo.calibration.UsesCalibrationOffset = true;
    }

    public void ProcessNozzleSizeExtrusionWidth()
    {
      if (!this.eeprom_mapping.HasKey("NozzleSizeExtrusionWidth"))
        return;
      int nozzleSizeMicrons1 = this.MyPrinterProfile.AccessoriesConstants.NozzleConstants.iMinimumNozzleSizeMicrons;
      int nozzleSizeMicrons2 = this.MyPrinterProfile.AccessoriesConstants.NozzleConstants.iMaximumNozzleSizeMicrons;
      int nozzleSizeMicrons3 = this.MyPrinterProfile.AccessoriesConstants.NozzleConstants.iDefaultNozzleSizeMicrons;
      int iNozzleWidthMicrons = (int) this.eeprom_mapping.GetUInt16("NozzleSizeExtrusionWidth");
      if (iNozzleWidthMicrons < nozzleSizeMicrons1 || iNozzleWidthMicrons > nozzleSizeMicrons2)
      {
        iNozzleWidthMicrons = nozzleSizeMicrons3;
        int num = (int) this.SetNozzleWidth(iNozzleWidthMicrons);
      }
      this.printerInfo.extruder.iNozzleSizeMicrons = iNozzleWidthMicrons;
    }

    public void ProcessFilamentDataFromEEPROM()
    {
      uint uint32 = this.eeprom_mapping.GetUInt32("FilamentColorID");
      if (uint32 == uint.MaxValue)
      {
        this.KnownFilament.filament_type = FilamentSpool.TypeEnum.NoFilament;
        this.KnownFilament.filament_temperature = 0;
        this.KnownFilament.filament_color_code = FilamentSpool.DefaultColorCode;
        this.KnownFilament.estimated_filament_length_printed = 0.0f;
      }
      else
      {
        this.KnownFilament.filament_color_code = uint32;
        int alignedByte1 = (int) this.eeprom_mapping.GetAlignedByte("FilamentTypeID");
        this.KnownFilament.filament_location = (alignedByte1 & 192) >> 6 != 1 ? FilamentSpool.Location.External : FilamentSpool.Location.Internal;
        int num = alignedByte1 & -193;
        this.KnownFilament.filament_type = !Enum.IsDefined(typeof (FilamentSpool.TypeEnum), (object) num) ? FilamentSpool.TypeEnum.NoFilament : (FilamentSpool.TypeEnum) num;
        this.KnownFilament.filament_temperature = (int) this.eeprom_mapping.GetAlignedByte("FilamentTemperature") + 100;
        this.KnownFilament.estimated_filament_length_printed = this.eeprom_mapping.GetFloat("FilamentAmount");
        ushort alignedByte2 = this.eeprom_mapping.GetAlignedByte("FilamentSize");
        if (!Enum.IsDefined(typeof (FilamentSpool.SizeEnum), (object) (int) alignedByte2))
          return;
        if (alignedByte2 == (ushort) 1)
          this.KnownFilament.filament_size = FilamentSpool.SizeEnum.Pro;
        else
          this.KnownFilament.filament_size = FilamentSpool.SizeEnum.Micro;
      }
    }

    public void ProcessFilamentUIDFromEEPROM()
    {
      this.KnownFilament.filament_uid = this.eeprom_mapping.GetUInt32("FilamentUID");
    }

    public void ProcessBedCompensationDataFromEEPROM()
    {
      this.CalibrationDetails.Calibration_Valid = false;
      this.CalibrationDetails.CORNER_HEIGHT_BACK_RIGHT = this.eeprom_mapping.GetFloat("BedCompensationBackRight");
      this.CalibrationDetails.CORNER_HEIGHT_BACK_LEFT = this.eeprom_mapping.GetFloat("BedCompensationBackLeft");
      this.CalibrationDetails.CORNER_HEIGHT_FRONT_RIGHT = this.eeprom_mapping.GetFloat("BedCompensationFrontRight");
      this.CalibrationDetails.CORNER_HEIGHT_FRONT_LEFT = this.eeprom_mapping.GetFloat("BedCompensationFrontLeft");
      this.CalibrationDetails.G32_VERSION = (int) this.eeprom_mapping.GetAlignedByte("BedCompensationVersion");
      if (this.CalibrationDetails.G32_VERSION == (int) byte.MaxValue)
        this.CalibrationDetails.G32_VERSION = 0;
      bool flag1 = (double) this.CalibrationDetails.CORNER_HEIGHT_BACK_RIGHT < -3.0 || (double) this.CalibrationDetails.CORNER_HEIGHT_BACK_RIGHT > 3.0 || ((double) this.CalibrationDetails.CORNER_HEIGHT_BACK_LEFT < -3.0 || (double) this.CalibrationDetails.CORNER_HEIGHT_BACK_LEFT > 3.0) || ((double) this.CalibrationDetails.CORNER_HEIGHT_FRONT_LEFT < -3.0 || (double) this.CalibrationDetails.CORNER_HEIGHT_FRONT_LEFT > 3.0 || (double) this.CalibrationDetails.CORNER_HEIGHT_FRONT_RIGHT < -3.0) || (double) this.CalibrationDetails.CORNER_HEIGHT_FRONT_RIGHT > 3.0;
      int num = (double) Math.Abs(this.CalibrationDetails.CORNER_HEIGHT_BACK_LEFT) >= 1.40129846432482E-45 || (double) Math.Abs(this.CalibrationDetails.CORNER_HEIGHT_BACK_RIGHT) >= 1.40129846432482E-45 || (double) Math.Abs(this.CalibrationDetails.CORNER_HEIGHT_FRONT_LEFT) >= 1.40129846432482E-45 ? 0 : ((double) Math.Abs(this.CalibrationDetails.CORNER_HEIGHT_FRONT_RIGHT) < 1.40129846432482E-45 ? 1 : 0);
      bool flag2 = float.IsNaN(this.CalibrationDetails.CORNER_HEIGHT_BACK_LEFT) || float.IsNaN(this.CalibrationDetails.CORNER_HEIGHT_BACK_RIGHT) || float.IsNaN(this.CalibrationDetails.CORNER_HEIGHT_FRONT_LEFT) || float.IsNaN(this.CalibrationDetails.CORNER_HEIGHT_FRONT_RIGHT);
      if (((num == 0 || !this.MyPrinterProfile.OptionsConstants.VerifyGantryNonZeroValues ? (!flag1 ? 0 : ("Pro" != this.MyPrinterProfile.ProfileName ? 1 : 0)) : 1) | (flag2 ? 1 : 0)) != 0)
        this.BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.BedOrientationMustBeCalibrated, this.MySerialNumber, (string) null).Serialize());
      else if (flag1 && "Pro" == this.MyPrinterProfile.ProfileName)
      {
        this.ResetG32Values();
        this.RequestUpdatedG32ValuesFromFirmware();
      }
      else
        this.CalibrationDetails.Calibration_Valid = true;
    }

    public void ProcessBedOffsetDataFromEEPROM()
    {
      this.CalibrationDetails.CORNER_HEIGHT_BACK_RIGHT_OFFSET = this.eeprom_mapping.GetFloat("ZCalibrationBRO");
      this.CalibrationDetails.CORNER_HEIGHT_BACK_LEFT_OFFSET = this.eeprom_mapping.GetFloat("ZCalibrationBLO");
      this.CalibrationDetails.CORNER_HEIGHT_FRONT_RIGHT_OFFSET = this.eeprom_mapping.GetFloat("ZCalibrationFRO");
      this.CalibrationDetails.CORNER_HEIGHT_FRONT_LEFT_OFFSET = this.eeprom_mapping.GetFloat("ZCalibrationFLO");
      this.CalibrationDetails.ENTIRE_Z_HEIGHT_OFFSET = this.eeprom_mapping.GetFloat("ZCalibrationZO");
      this.zeroIfNan(ref this.CalibrationDetails.CORNER_HEIGHT_BACK_RIGHT_OFFSET);
      this.zeroIfNan(ref this.CalibrationDetails.CORNER_HEIGHT_BACK_LEFT_OFFSET);
      this.zeroIfNan(ref this.CalibrationDetails.CORNER_HEIGHT_FRONT_RIGHT_OFFSET);
      this.zeroIfNan(ref this.CalibrationDetails.CORNER_HEIGHT_FRONT_LEFT_OFFSET);
      this.zeroIfNan(ref this.CalibrationDetails.ENTIRE_Z_HEIGHT_OFFSET);
    }

    private void zeroIfNan(ref float fl)
    {
      if (!float.IsNaN(fl))
        return;
      fl = 0.0f;
    }

    public void ProcessBacklashEEPROM()
    {
      bool flag = false;
      this.CalibrationDetails.BACKLASH_X = this.eeprom_mapping.GetFloat("BacklashX");
      this.CalibrationDetails.BACKLASH_Y = this.eeprom_mapping.GetFloat("BacklashY");
      if (float.IsNaN(this.CalibrationDetails.BACKLASH_X) || (double) this.CalibrationDetails.BACKLASH_X > 2.0)
        flag = true;
      if (float.IsNaN(this.CalibrationDetails.BACKLASH_Y) || (double) this.CalibrationDetails.BACKLASH_Y > 2.0)
        flag = true;
      if (!flag)
        return;
      this.BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.BacklashOutOfRange, this.MySerialNumber, (string) null).Serialize());
    }

    public void ProcessBacklashSpeedEEPROM()
    {
      this.CalibrationDetails.BACKLASH_SPEED = this.eeprom_mapping.GetFloat("BacklashSpeed");
      if (!float.IsNaN(this.CalibrationDetails.BACKLASH_SPEED) && (double) this.CalibrationDetails.BACKLASH_SPEED > 1.0)
        return;
      this.CalibrationDetails.BACKLASH_SPEED = this.MyPrinterProfile.SpeedLimitConstants.DefaultBacklashSpeed;
      EepromAddressInfo eepromInfo = this.MyPrinterProfile.EEPROMConstants.GetEepromInfo("BacklashSpeed");
      int num = (int) this.WriteManualCommands(PrinterCompatibleString.Format("M618 S{0} P{1} T{2}", (object) eepromInfo.EepromAddr, (object) EEPROMMapping.FloatToBinaryInt(this.CalibrationDetails.BACKLASH_SPEED), (object) eepromInfo.Size));
    }

    private void ResetG32Values()
    {
      EepromAddressInfo eepromInfo1 = this.MyPrinterProfile.EEPROMConstants.GetEepromInfo("BedCompensationBackRight");
      EepromAddressInfo eepromInfo2 = this.MyPrinterProfile.EEPROMConstants.GetEepromInfo("BedCompensationBackLeft");
      EepromAddressInfo eepromInfo3 = this.MyPrinterProfile.EEPROMConstants.GetEepromInfo("BedCompensationFrontLeft");
      EepromAddressInfo eepromInfo4 = this.MyPrinterProfile.EEPROMConstants.GetEepromInfo("BedCompensationFrontRight");
      if (eepromInfo1 == null || eepromInfo2 == null || (eepromInfo3 == null || eepromInfo4 == null))
        return;
      int num = (int) this.WriteManualCommands(PrinterCompatibleString.Format("M618 S{0} P0 T{1}", (object) eepromInfo1.EepromAddr, (object) eepromInfo1.Size), PrinterCompatibleString.Format("M618 S{0} P0 T{1}", (object) eepromInfo2.EepromAddr, (object) eepromInfo2.Size), PrinterCompatibleString.Format("M618 S{0} P0 T{1}", (object) eepromInfo3.EepromAddr, (object) eepromInfo3.Size), PrinterCompatibleString.Format("M618 S{0} P0 T{1}", (object) eepromInfo4.EepromAddr, (object) eepromInfo4.Size));
    }

    public float FloatFromEEPROM(string name)
    {
      return this.eeprom_mapping.GetFloat(name);
    }

    private void RequestUpdatedG32ValuesFromFirmware()
    {
      EepromAddressInfo eepromInfo1 = this.MyPrinterProfile.EEPROMConstants.GetEepromInfo("BedCompensationBackRight");
      EepromAddressInfo eepromInfo2 = this.MyPrinterProfile.EEPROMConstants.GetEepromInfo("BedCompensationBackLeft");
      EepromAddressInfo eepromInfo3 = this.MyPrinterProfile.EEPROMConstants.GetEepromInfo("BedCompensationFrontLeft");
      EepromAddressInfo eepromInfo4 = this.MyPrinterProfile.EEPROMConstants.GetEepromInfo("BedCompensationFrontRight");
      EepromAddressInfo eepromInfo5 = this.MyPrinterProfile.EEPROMConstants.GetEepromInfo("ZCalibrationBLO");
      EepromAddressInfo eepromInfo6 = this.MyPrinterProfile.EEPROMConstants.GetEepromInfo("ZCalibrationBRO");
      EepromAddressInfo eepromInfo7 = this.MyPrinterProfile.EEPROMConstants.GetEepromInfo("ZCalibrationFRO");
      EepromAddressInfo eepromInfo8 = this.MyPrinterProfile.EEPROMConstants.GetEepromInfo("ZCalibrationFLO");
      EepromAddressInfo eepromInfo9 = this.MyPrinterProfile.EEPROMConstants.GetEepromInfo("CalibrationOffset");
      EepromAddressInfo eepromInfo10 = this.MyPrinterProfile.EEPROMConstants.GetEepromInfo("BedCompensationVersion");
      if (eepromInfo1 != null && eepromInfo2 != null && (eepromInfo3 != null && eepromInfo4 != null))
      {
        int num1 = (int) this.WriteManualCommands("M619 S" + (object) eepromInfo1.EepromAddr + " T" + (object) eepromInfo1.Size, "M619 S" + (object) eepromInfo2.EepromAddr + " T" + (object) eepromInfo2.Size, "M619 S" + (object) eepromInfo3.EepromAddr + " T" + (object) eepromInfo3.Size, "M619 S" + (object) eepromInfo4.EepromAddr + " T" + (object) eepromInfo4.Size);
      }
      if (eepromInfo5 != null && eepromInfo6 != null && (eepromInfo7 != null && eepromInfo8 != null))
      {
        int num2 = (int) this.WriteManualCommands("M619 S" + (object) eepromInfo5.EepromAddr + " T" + (object) eepromInfo5.Size, "M619 S" + (object) eepromInfo6.EepromAddr + " T" + (object) eepromInfo6.Size, "M619 S" + (object) eepromInfo7.EepromAddr + " T" + (object) eepromInfo7.Size, "M619 S" + (object) eepromInfo8.EepromAddr + " T" + (object) eepromInfo8.Size);
      }
      if (this.MyPrinterInfo.calibration.UsesCalibrationOffset && eepromInfo9 != null)
      {
        int num3 = (int) this.WriteManualCommands("M619 S" + (object) eepromInfo9.EepromAddr + " T" + (object) eepromInfo9.Size);
      }
      if (eepromInfo10 == null)
        return;
      int num4 = (int) this.WriteManualCommands("M619 S" + (object) eepromInfo10.EepromAddr + " T" + (object) eepromInfo10.Size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DoWrites()
    {
      if (this.cur_special_message != FirmwareController.SpecialMessage.None && !this.mSpecialMessageSent)
      {
        if (this.Status == PrinterStatus.Firmware_Homing || this.Status == PrinterStatus.Firmware_Calibrating)
          return;
        GCode gcode = new GCode();
        if (this.cur_special_message == FirmwareController.SpecialMessage.EmergencyStop_M0)
          gcode.Parse("M0");
        else if (this.cur_special_message == FirmwareController.SpecialMessage.HaltAllMoves_M1)
          gcode.Parse("M1");
        else if (this.cur_special_message == FirmwareController.SpecialMessage.GetPosition_M114)
          gcode.Parse("M114");
        else if (this.cur_special_message == FirmwareController.SpecialMessage.GoToBootloader)
          gcode.Parse("M115 S628");
        if (!this.WriteToSerial(this.TranslateGCode(gcode)))
        {
          this.OnUnexpectedDisconnect(MessageType.UnexpectedDisconnect);
          this.WriteLog("Disconnected because of a serial write error", Logger.TextType.Error);
        }
        this.mSpecialMessageSent = true;
      }
      else
      {
        if (this.mSpecialMessageSent)
          return;
        this.UpdateJobStatus();
        this.SendNextCommand();
      }
    }

    private void UpdateJobStatus()
    {
      if (this.m_oJobController.HasJob)
      {
        this.MyPrinterInfo.current_job = this.m_oJobController.Info;
        if (!this.m_oJobController.Printing)
        {
          if (!this.m_oJobController.Processed)
            return;
          if (this.m_oJobController.HasWarnings)
          {
            if (!this.jobmessage_not_sent)
              return;
            this.jobmessage_not_sent = false;
            this.BroadcastServer.BroadcastMessage(new SpoolerMessage(this.m_oJobController.GetNextWarning(), this.MySerialNumber, (string) null).Serialize());
          }
          else
          {
            this.beginOfPrintE = this.GetCurrentFilament().estimated_filament_length_printed;
            List<string> start_gcode;
            int num1 = (int) this.m_oJobController.Start(out start_gcode);
            if (start_gcode != null && start_gcode.Count > 0)
            {
              int num2 = (int) this.WriteManualCommands(start_gcode.ToArray());
            }
            this.WriteLog("<< Print Job Started " + DateTime.Now.ToShortTimeString(), Logger.TextType.Error);
            if (this.m_oJobController.Status != JobStatus.SavingToFile)
              this.BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.JobStarted, this.MySerialNumber, this.m_oJobController.JobName).Serialize());
            if (!this.IsPausedorPausing)
              this.Status = PrinterStatus.Firmware_Printing;
            this.oswRefreshTimer.Reset();
            this.oswRefreshTimer.Start();
          }
        }
        else if (this.m_oJobController.Done)
        {
          this.BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.JobComplete, this.MySerialNumber, this.m_oJobController.JobName).Serialize());
          this.StopJobInternal();
        }
        else
          this.m_oJobController.Update();
      }
      else
        this.MyPrinterInfo.current_job = (JobInfo) null;
    }

    public void ClearWarning()
    {
      if (!this.m_oJobController.HasJob || this.m_oJobController.Status != JobStatus.Queued || !this.m_oJobController.HasWarnings)
        return;
      this.m_oJobController.ClearCurrentWarning();
      this.jobmessage_not_sent = true;
    }

    public override CommandResult WriteManualCommands(params string[] commands)
    {
      if (commands.Length < 1)
        return CommandResult.Failed_Exception;
      if (this.MyPrinterInfo.FirmwareIsInvalid)
      {
        this.BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.FirmwareMustBeUpdated, this.MySerialNumber, (string) null).Serialize());
        lock (this.manual_commands)
          this.manual_commands.Clear();
        return CommandResult.Failed_Exception;
      }
      foreach (string command in commands)
      {
        GCode newgcode = (GCode) null;
        try
        {
          newgcode = new GCode(command);
        }
        catch (Exception ex)
        {
          this.WriteLog(ex.Message, Logger.TextType.Error);
        }
        if (newgcode == null)
          return CommandResult.Failed_Exception;
        if (this.IsPausedorPausing && newgcode.hasG && (newgcode.G == (ushort) 28 || newgcode.G == (ushort) 32 || newgcode.G == (ushort) 30))
        {
          lock (this.manual_commands)
            this.manual_commands.Clear();
          int num = (int) this.base_printer.ReleaseLock(this.base_printer.MyLock);
          return CommandResult.Failed_G28_G30_G32_NotAllowedWhilePaused;
        }
        if (newgcode.hasM && newgcode.hasS && (newgcode.M == (ushort) 115 && newgcode.S == 628))
        {
          this.GotoBootloader();
          return CommandResult.Success;
        }
        if (newgcode.hasM && newgcode.M <= (ushort) 1)
        {
          if (newgcode.M == (ushort) 0)
            this.SendEmergencyStop();
          else if (newgcode.M == (ushort) 1)
            this.HaltAllMoves();
          return CommandResult.Success;
        }
        if (!this.CheckGantryClipsBeforeCommand(newgcode))
          return CommandResult.Failed_GantryClipsOrInvalidZ;
        lock (this.manual_commands)
          this.manual_commands.AddToBack(newgcode);
      }
      return CommandResult.Success;
    }

    public void AddManualCommandToFront(params string[] commands)
    {
      for (int index = commands.Length - 1; index >= 0; --index)
      {
        lock (this.manual_commands)
          this.manual_commands.AddToFront(new GCode(commands[index]));
      }
    }

    private bool CheckGantryClipsBeforeCommand(GCode newgcode)
    {
      if (!this.MyPrinterProfile.OptionsConstants.CheckGantryClips || this.PersistantDetails.GantryClipsRemoved || !SpoolerServer.CHECK_GANTRY_CLIPS || (!newgcode.hasM || newgcode.M != (ushort) 104 && newgcode.M != (ushort) 109) && (!newgcode.hasG || newgcode.G == (ushort) 90 || newgcode.G == (ushort) 91))
        return true;
      lock (this.manual_commands)
        this.manual_commands.Clear();
      if (!this.oswGantryTimer.IsRunning || this.oswGantryTimer.ElapsedMilliseconds > 3000L)
      {
        this.oswGantryTimer.Restart();
        this.WriteLog("Gantry clip check has not occurred", Logger.TextType.Error);
        this.BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.CheckGantryClips, this.MySerialNumber, (string) null).Serialize());
        int num = (int) this.base_printer.ReleaseLock(this.base_printer.MyLock);
      }
      return false;
    }

    public void SetGantryClips(bool clips_are_off)
    {
      this.oswGantryTimer.Stop();
      this.oswGantryTimer.Reset();
      this.PersistantDetails.GantryClipsRemoved = clips_are_off;
      this.SavePersistantData();
    }

    public void SendEmergencyStop()
    {
      lock (this.threadsync)
      {
        this.cur_special_message = FirmwareController.SpecialMessage.EmergencyStop_M0;
        this.mSpecialMessageSent = false;
        lock (this.manual_commands)
          this.manual_commands.Clear();
      }
    }

    private void HaltAllMoves()
    {
      lock (this.threadsync)
      {
        this.cur_special_message = FirmwareController.SpecialMessage.HaltAllMoves_M1;
        this.mSpecialMessageSent = false;
        lock (this.manual_commands)
          this.manual_commands.Clear();
      }
    }

    public void AbortPrint()
    {
      if (this.m_oJobController.HasJob && !this.m_oJobController.IsSavingToSDOnly)
      {
        this.BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.JobCanceled, this.MySerialNumber, this.m_oJobController.JobName).Serialize());
        this.HaltAllMoves();
      }
      else
        this.StopJobInternal();
    }

    private void MoveToSafeLocation()
    {
      if (!this.m_oJobController.HasJob || this.m_oJobController.IsSavingToSD && !this.m_oJobController.IsSimultaneousPrint)
        return;
      int num = (int) this.WriteManualCommands(GCodeInitializationPreprocessor.GenerateEndGCode(this.m_oJobController.Details, this.MyPrinterProfile, this.m_oJobController.RetractionRequired).ToArray());
    }

    private void StopJobInternal()
    {
      if (!this.m_oJobController.HasJob)
        return;
      this.UpdateStatsTime(this.m_oJobController.GetElapsedReset);
      List<string> end_gcode;
      int num1 = (int) this.m_oJobController.StopJob(out end_gcode);
      if (end_gcode != null && end_gcode.Count > 0)
      {
        int num2 = (int) this.WriteManualCommands(end_gcode.ToArray());
      }
      this.jobmessage_not_sent = true;
      this.WriteLog("<< Print Job Stopped " + DateTime.Now.ToShortTimeString(), Logger.TextType.Error);
      this.Status = PrinterStatus.Firmware_Executing;
      this.UpdateStatsOnPrinter();
      this.PushFilamentUsedToPrinter();
      this.MyPrinterInfo.current_job = (JobInfo) null;
    }

    public CommandResult ContinuePrint()
    {
      if (!this.IsPaused)
        return CommandResult.Failed_PrinterNotPaused;
      List<string> resume_gcode;
      switch (this.m_oJobController.Resume(out resume_gcode, this.KnownFilament))
      {
        case JobController.Result.Success:
          if (resume_gcode != null && resume_gcode.Count > 0)
          {
            int num = (int) this.WriteManualCommands(resume_gcode.ToArray());
          }
          this.Status = PrinterStatus.Firmware_Printing;
          break;
        case JobController.Result.FAILED_NoFilament:
          return CommandResult.Failed_PrinterDoesNotHaveFilament;
      }
      return CommandResult.SuccessfullyReceived;
    }

    public CommandResult PausePrint()
    {
      if (!this.IsPrinting)
        return CommandResult.Failed_PrinterNotPrinting;
      if (this.m_oJobController.IsSavingToSD)
        return CommandResult.Failed_CannotPauseSavingToSD;
      this.Status = PrinterStatus.Firmware_IsWaitingToPause;
      return CommandResult.SuccessfullyReceived;
    }

    private void VerifyHomingPosition(ref GCode gcode)
    {
      if (this.IsPrinting || !this.BoundsCheckingEnabled)
        return;
      List<GCode> gcodeList = new List<GCode>();
      float num = this.MyPrinterInfo.extruder.position.pos.z - this.MyPrinterProfile.PrinterSizeConstants.BoxTopLimitZ;
      if ((double) num <= 0.0)
        return;
      if (this.MyPrinterInfo.extruder.inRelativeMode == Trilean.False)
        gcodeList.Add(new GCode("G91"));
      gcodeList.Add(new GCode(PrinterCompatibleString.Format("G0 Z-{0}", (object) num)));
      if (this.MyPrinterInfo.extruder.inRelativeMode == Trilean.False)
        gcodeList.Add(new GCode("G90"));
      gcodeList.Add(new GCode("G28"));
      gcode = gcodeList[0];
      gcodeList.RemoveAt(0);
      gcodeList.Reverse();
      lock (this.manual_commands)
      {
        foreach (GCode gcode1 in gcodeList)
          this.manual_commands.AddToFront(gcode1);
      }
    }

    protected void TrackExtruderPosition(GCode gcode)
    {
      if (gcode.hasG)
      {
        if (gcode.G == (ushort) 90)
          this.MyPrinterInfo.extruder.inRelativeMode = Trilean.False;
        else if (gcode.G == (ushort) 91)
          this.MyPrinterInfo.extruder.inRelativeMode = Trilean.True;
        else if (gcode.G == (ushort) 28)
          this.SetToHomeLocation();
        else if (gcode.G == (ushort) 30 || gcode.G == (ushort) 32)
        {
          this.SetToHomeLocation();
          this.MyPrinterInfo.extruder.position.pos.z = this.MyPrinterProfile.PrinterSizeConstants.ZAfterProbing;
          this.MyPrinterInfo.extruder.Z_Valid = true;
        }
        else if (gcode.G == (ushort) 33)
        {
          this.MyPrinterInfo.extruder.position.pos.z = this.MyPrinterProfile.PrinterSizeConstants.ZAfterG33;
          this.MyPrinterInfo.extruder.Z_Valid = true;
        }
        else if (gcode.G == (ushort) 92)
        {
          if (gcode.hasE)
            this.MyPrinterInfo.extruder.position.e = gcode.E;
          if (this.MyPrinterProfile.OptionsConstants.G92WorksOnAllAxes)
          {
            if (gcode.hasX)
              this.MyPrinterInfo.extruder.position.pos.x = gcode.X;
            if (gcode.hasY)
              this.MyPrinterInfo.extruder.position.pos.y = gcode.Y;
            if (gcode.hasZ)
              this.MyPrinterInfo.extruder.position.pos.z = gcode.Z;
          }
          if (!gcode.hasE && !gcode.hasX && (!gcode.hasY && !gcode.hasZ))
          {
            this.MyPrinterInfo.extruder.position.e = 0.0f;
            if (this.MyPrinterProfile.OptionsConstants.G92WorksOnAllAxes)
            {
              this.MyPrinterInfo.extruder.position.pos.x = 0.0f;
              this.MyPrinterInfo.extruder.position.pos.y = 0.0f;
              this.MyPrinterInfo.extruder.position.pos.z = 0.0f;
            }
          }
        }
        else if ((gcode.G == (ushort) 0 || gcode.G == (ushort) 1) && this.MyPrinterInfo.extruder.inRelativeMode != Trilean.Unknown)
        {
          Vector3D op3dInitial = new Vector3D(this.MyPrinterInfo.extruder.position.pos);
          Vector3D destination = MovementUtility.op3dCalculateDestination(this.MyPrinterInfo.extruder.ishomed, this.MyPrinterInfo.extruder.Z_Valid, this.MyPrinterInfo.extruder.inRelativeMode, gcode, op3dInitial);
          if (!this.IsPrinting && this.BoundsCheckingEnabled)
          {
            Vector3D vector3D = new Vector3D(0.0f, 0.0f, 0.0f);
            bool bDestinationHasBeenClipped = false;
            Vector3D destinationWithClipping = MovementUtility.op3dCalculateDestinationWithClipping(this.MyPrinterInfo.extruder.ishomed, this.MyPrinterInfo.extruder.Z_Valid, ref bDestinationHasBeenClipped, destination, this.MyPrinterInfo.extruder.position.pos, (PrinterProfile) this.MyPrinterProfile);
            if (bDestinationHasBeenClipped)
              MovementUtility.vGetEffectiveMovementGCode(this.MyPrinterInfo.extruder.ishomed, this.MyPrinterInfo.extruder.Z_Valid, this.MyPrinterInfo.extruder.inRelativeMode, destinationWithClipping, this.MyPrinterInfo.extruder.position.pos, ref gcode);
            this.MyPrinterInfo.extruder.position.pos = destinationWithClipping;
          }
          else
            this.MyPrinterInfo.extruder.position.pos = destination;
        }
      }
      if (!gcode.hasM || gcode.M != (ushort) 0 && gcode.M != (ushort) 1)
        return;
      this.MyPrinterInfo.extruder.position = new Vector3DE(-1f, -1f, -1f, -1f);
    }

    protected void TrackFilament(GCode gcode)
    {
      if (!gcode.hasG || !gcode.hasE || gcode.G != (ushort) 0 && gcode.G != (ushort) 1)
        return;
      if (this.MyPrinterInfo.extruder.inRelativeMode == Trilean.False)
      {
        this.MyPrinterInfo.filament_info.estimated_filament_length_printed = this.beginOfPrintE + gcode.E;
      }
      else
      {
        if (this.MyPrinterInfo.extruder.inRelativeMode != Trilean.True)
          return;
        this.MyPrinterInfo.filament_info.estimated_filament_length_printed += gcode.E;
        this.beginOfPrintE += gcode.E;
      }
    }

    protected void SetToHomeLocation()
    {
      this.MyPrinterInfo.extruder.position.pos.x = this.MyPrinterProfile.PrinterSizeConstants.HomeLocation.x;
      this.MyPrinterInfo.extruder.position.pos.y = this.MyPrinterProfile.PrinterSizeConstants.HomeLocation.y;
      if (!float.IsNaN(this.MyPrinterProfile.PrinterSizeConstants.HomeLocation.z))
      {
        this.MyPrinterInfo.extruder.position.pos.z = this.MyPrinterProfile.PrinterSizeConstants.HomeLocation.z;
        this.MyPrinterInfo.extruder.Z_Valid = true;
      }
      this.MyPrinterInfo.extruder.ishomed = Trilean.True;
    }

    protected GCode ConvertToSafeEmptyGCode(GCode gcode)
    {
      GCode gcode1 = new GCode("G4 S0");
      if (gcode.hasN)
        gcode1.N = gcode.N;
      return gcode1;
    }

    private byte[] TranslateGCode(GCode gcode)
    {
      if (gcode.hasG && gcode.G == (ushort) 28 && this.MyPrinterInfo.extruder.Z_Valid)
        this.VerifyHomingPosition(ref gcode);
      this.TrackExtruderPosition(gcode);
      this.TrackFilament(gcode);
      this.LastGCodeType = RequestGCode.NonRequestGCode;
      if (!gcode.hasM || gcode.M != (ushort) 5321 && gcode.M != (ushort) 619 && gcode.M != (ushort) 618 && (gcode.M != (ushort) 115 || gcode.hasS && gcode.S != 628))
      {
        bool flag = false;
        if (!gcode.hasN || flag || !this.IsPrinting)
          this.WriteLog("<< " + gcode.getAscii(true, false), Logger.TextType.Write);
      }
      else
        this.LastGCodeType = RequestGCode.HiddenType;
      if (gcode.hasM && gcode.M != (ushort) 0)
      {
        if (this.MyPrinterProfile.VirtualCodes.ProcessVirtualCode(gcode, this))
        {
          if (!gcode.hasN)
            return (byte[]) null;
          int n = gcode.N;
          gcode = new GCode("G4 S0");
          gcode.N = n;
        }
        if (gcode.M == (ushort) 117)
          this.LastGCodeType = RequestGCode.M117GetInternalState;
        else if (gcode.M == (ushort) 114)
        {
          this.LastGCodeType = RequestGCode.M114GetExtruderLocation;
          this.ExtruderDetails.position.Reset();
        }
        else if (gcode.M == (ushort) 576)
          this.LastGCodeType = RequestGCode.M576GetFilamentInfo;
        else if (gcode.M == (ushort) 618)
        {
          int s = gcode.S;
          int p = gcode.P;
          EepromAddressInfo infoFromLocation = this.MyPrinterProfile.EEPROMConstants.GetEepromInfoFromLocation(s);
          if (infoFromLocation != null)
          {
            this.eeprom_mapping.SetValue(infoFromLocation, p);
            this.CheckUpdatedPTValue((int) infoFromLocation.EepromAddr);
          }
        }
        else if ((gcode.M == (ushort) 109 || gcode.M == (ushort) 116) && (!this.IsPrinting && !this.IsPausedorPausing) && !this.IsRecovering)
          this.Status = PrinterStatus.Firmware_WarmingUp;
      }
      if (gcode.hasG)
      {
        if (gcode.G == (ushort) 30 || gcode.G == (ushort) 32 || gcode.G == (ushort) 33)
        {
          bool flag1 = true;
          bool flag2 = true;
          if (this.MyPrinterInfo.supportedFeatures.UsesSupportedFeatures)
          {
            int featureSlot1 = this.MyPrinterProfile.SupportedFeaturesConstants.GetFeatureSlot("Multi Point Automatic Bed Calibration");
            int featureSlot2 = this.MyPrinterProfile.SupportedFeaturesConstants.GetFeatureSlot("Single Point Bed Height Calibration");
            if (featureSlot1 >= 0)
              flag1 = (uint) this.MyPrinterInfo.supportedFeatures.GetStatus(featureSlot1) > 0U;
            if (featureSlot2 >= 0)
              flag2 = (uint) this.MyPrinterInfo.supportedFeatures.GetStatus(featureSlot2) > 0U;
          }
          if (gcode.G == (ushort) 32 && !flag1)
          {
            gcode = this.ConvertToSafeEmptyGCode(gcode);
            this.BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.MultiPointCalibrationNotSupported, this.MySerialNumber, (string) null).Serialize());
          }
          else if (gcode.G == (ushort) 30 && !flag2)
          {
            gcode = this.ConvertToSafeEmptyGCode(gcode);
            this.BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.SinglePointCalibrationNotSupported, this.MySerialNumber, (string) null).Serialize());
          }
          else if (this.Status != PrinterStatus.Firmware_Printing)
          {
            this.LastGCodeType = RequestGCode.G30_32_30_ZSaveGcode;
            this.ExtruderDetails.position.Reset();
            this.ClearCalibrationErrors();
            if (gcode.G == (ushort) 30)
            {
              this.SetOffsetInformationZOnly(0.0f);
              this.RequestUpdatedG32ValuesFromFirmware();
              int num = (int) this.WriteManualCommands("M117", "M578");
            }
            else if (gcode.G == (ushort) 32)
            {
              this.SetOffsetInformation(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, false);
              this.RequestUpdatedG32ValuesFromFirmware();
              int num = (int) this.WriteManualCommands("M117", "M578");
            }
          }
          if (gcode.G != (ushort) 33 && !this.IsRecovering)
            this.Status = PrinterStatus.Firmware_Calibrating;
        }
        if (gcode.G == (ushort) 28 && PrinterStatus.Firmware_PowerRecovery != this.Status)
          this.Status = PrinterStatus.Firmware_Homing;
      }
      if (this.HardwareDetails.repetier_protocol == 0)
        return this.mAsciiEncoder.GetBytes(gcode.getAscii(true, false) + "\r\n");
      return gcode.getBinary(this.HardwareDetails.repetier_protocol);
    }

    protected void SendNextCommand()
    {
      GCode gcode1 = (GCode) null;
      if (this.ResendLastCommand)
      {
        gcode1 = this.m_ogcLastGCodeSent;
      }
      else
      {
        GCode gcode2 = (GCode) null;
        lock (this.manual_commands)
        {
          if (this.LastMessageClear)
          {
            if (this.manual_commands.Count > 0)
              gcode2 = this.manual_commands.RemoveFromFront();
          }
        }
        if (gcode2 != null)
          gcode1 = gcode2;
        else if (!this.IsPaused && this.m_oJobController.HasJob && (this.m_oJobController.Printing && this.LastMessageClear))
        {
          gcode1 = this.m_oJobController.GetNextGCode();
          if (gcode1 != null)
            this.LastMessageClear = false;
        }
      }
      if (gcode1 == null)
        return;
      if (!gcode1.hasN)
        this.CheckForPluginCommand(gcode1);
      byte[] command = this.TranslateGCode(gcode1);
      if (command == null || command.Length == 0)
        return;
      this.PrinterIdle = false;
      if (!this.WriteToSerial(command))
      {
        this.OnUnexpectedDisconnect(MessageType.UnexpectedDisconnect);
        this.WriteLog("Disconnected because of a serial write error", Logger.TextType.Error);
      }
      else
      {
        this.m_nWaitCounter = 0;
        this.LastMessageClear = false;
        this.m_ogcLastGCodeSent = gcode1;
        this.oswResendTimer.Reset();
        this.oswResendTimer.Start();
        this.ResendLastCommand = false;
      }
    }

    private void CheckPauseTimer()
    {
      if (this.Status != PrinterStatus.Firmware_PrintingPaused || !this.oswPauseTimer.IsRunning || this.oswPauseTimer.ElapsedMilliseconds <= 300000L)
        return;
      int num = (int) this.WriteManualCommands("G4 S0");
      this.oswPauseTimer.Restart();
    }

    public void PauseAllMoves()
    {
      List<string> pause_gcode;
      if (this.Status != PrinterStatus.Firmware_IsWaitingToPause || !this.m_oJobController.Pause(out pause_gcode, this.KnownFilament))
        return;
      if (pause_gcode != null && pause_gcode.Count > 0)
      {
        int num = (int) this.WriteManualCommands(pause_gcode.ToArray());
      }
      this.Status = PrinterStatus.Firmware_PrintingPaused;
      this.PushFilamentUsedToPrinter();
    }

    public void KillJobs()
    {
      this.SendEmergencyStop();
    }

    public void AddPrintJob(string user, JobParams jobParam)
    {
      FirmwareController.NewJobData newJobData;
      newJobData.jobParam = jobParam;
      newJobData.user = user;
      try
      {
        newJobData.jobParam.VerifyOptionsWithPrinter((PrinterProfile) this.MyPrinterProfile, this.MyPrinterInfo);
      }
      catch (Exception ex)
      {
        SpoolerMessage spoolerMessage = new SpoolerMessage(MessageType.JobNotStarted, this.MySerialNumber, "The print job can't be started because of incompatible options.");
        this.BroadcastServer.BroadcastMessage("");
        return;
      }
      ThreadPool.QueueUserWorkItem(new WaitCallback(this.AddJobWorker), (object) newJobData);
    }

    private void AddJobWorker(object data)
    {
      FirmwareController.NewJobData newJobData = (FirmwareController.NewJobData) data;
      try
      {
        if (this.IsWorking || this.GetJobsCount() > 0)
          this.BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.CantStartJobPrinterBusy, (string) null).Serialize());
        else
          this.AddJob(newJobData.jobParam, newJobData.user);
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("Exception in SpoolerServer.AddJobWorker " + ex.Message, ex);
      }
    }

    private void AddJob(JobParams jobParams, string user)
    {
      if (this.m_oJobController.HasJob)
        return;
      if (jobParams.options.turn_on_fan_before_print)
      {
        int num1 = (int) this.WriteManualCommands("M106 S1");
      }
      if (!this.ExtruderDetails.Z_Valid && SpoolerServer.CHECK_BED_CALIBRATION)
        this.BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.InvalidZ, this.MySerialNumber, (string) null).Serialize());
      else if (!this.CalibrationDetails.Calibration_Valid && SpoolerServer.CHECK_BED_CALIBRATION)
        this.BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.BedOrientationMustBeCalibrated, this.MySerialNumber, (string) null).Serialize());
      else if (!this.PersistantDetails.GantryClipsRemoved && SpoolerServer.CHECK_GANTRY_CLIPS)
      {
        this.BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.CheckGantryClips, this.MySerialNumber, (string) null).Serialize());
      }
      else
      {
        JobController.Result result;
        try
        {
          result = this.m_oJobController.InitPrintJob(jobParams, user, 0UL);
        }
        catch (AbstractPreprocessedJob.PreprocessorException ex)
        {
          this.WriteLog(ex.Message, Logger.TextType.Error);
          this.BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.PrinterError, this.MySerialNumber, "Error in preprocessor: " + ex.Message).Serialize());
          int num2 = (int) this.m_oJobController.ForceShutdownNow();
          return;
        }
        SpoolerMessage spoolerMessage;
        switch (result)
        {
          case JobController.Result.Success:
            return;
          case JobController.Result.FAILED_OutOfBounds:
            spoolerMessage = new SpoolerMessage(MessageType.ModelOutOfPrintableBounds, this.MySerialNumber, jobParams.jobname);
            break;
          case JobController.Result.FAILED_IncompatibleFilament:
            spoolerMessage = new SpoolerMessage(MessageType.SDPrintIncompatibleFilament, this.MySerialNumber, jobParams.jobname);
            break;
          default:
            spoolerMessage = new SpoolerMessage(MessageType.JobNotStarted, this.MySerialNumber, jobParams.jobname);
            break;
        }
        this.BroadcastServer.BroadcastMessage(spoolerMessage.Serialize());
        int num3 = (int) this.m_oJobController.ForceShutdownNow();
      }
    }

    internal void ConnectToActiveSDPrint()
    {
      if (this.m_oJobController.HasJob)
        return;
      this.m_oJobController.ConnectToRunningSDPrint("");
      this.Status = PrinterStatus.Firmware_Printing;
    }

    internal CommandResult RecoverySpoolerPrintCallback(JobParams jobParams, ulong ulLineToSkipTo)
    {
      if (this.m_oJobController.HasJob)
        return CommandResult.Failed_ThePrinterIsPrintingOrPaused;
      return this.m_oJobController.RecoverySpoolerPrintCallback(jobParams, ulLineToSkipTo);
    }

    private void SetActionOnRestart(PersistantData.RestartAction restart_action, int param)
    {
      this.PersistantDetails.MyRestartAction = restart_action;
      this.PersistantDetails.RestartActionParam = param;
      this.SavePersistantData();
    }

    public void GotoBootloader()
    {
      lock (this.threadsync)
      {
        this.broadcast_shutdown = false;
        this.cur_special_message = FirmwareController.SpecialMessage.GoToBootloader;
        this.mSpecialMessageSent = false;
        this.StopJobInternal();
      }
      this.SetActionOnRestart(PersistantData.RestartAction.ForceStayBootloader, 0);
    }

    public override void UpdateFirmware()
    {
      this.GotoBootloader();
      this.SetActionOnRestart(PersistantData.RestartAction.ForceUpdateFirmware, 0);
    }

    public override void SetFanConstants(FanConstValues.FanType fanType)
    {
      this.GotoBootloader();
      this.SetActionOnRestart(PersistantData.RestartAction.SetFan, (int) fanType);
    }

    public override void SetExtruderCurrent(ushort current)
    {
      this.GotoBootloader();
      this.SetActionOnRestart(PersistantData.RestartAction.SetExtruderCurrent, (int) current);
    }

    public void SetOffsetInformation(BedOffsets Off)
    {
      this.SetOffsetInformation(Off, true);
    }

    public void SetOffsetInformation(BedOffsets Off, bool check)
    {
      this.SetOffsetInformation(Off.BL, Off.BR, Off.FR, Off.FL, Off.ZO, check);
    }

    private void SetOffsetInformation(float BL, float BR, float FR, float FL, float ZO, bool check)
    {
      if (check)
      {
        if ((double) Math.Abs(this.CalibrationDetails.CORNER_HEIGHT_BACK_LEFT_OFFSET - BL) < 1.40129846432482E-45 && (double) Math.Abs(this.CalibrationDetails.CORNER_HEIGHT_BACK_RIGHT_OFFSET - BR) < 1.40129846432482E-45 && ((double) Math.Abs(this.CalibrationDetails.CORNER_HEIGHT_FRONT_LEFT_OFFSET - FL) < 1.40129846432482E-45 && (double) Math.Abs(this.CalibrationDetails.CORNER_HEIGHT_FRONT_RIGHT_OFFSET - FR) < 1.40129846432482E-45) && (double) Math.Abs(this.CalibrationDetails.ENTIRE_Z_HEIGHT_OFFSET - ZO) < 1.40129846432482E-45)
          return;
        this.CalibrationDetails.CORNER_HEIGHT_BACK_LEFT_OFFSET = BL;
        this.CalibrationDetails.CORNER_HEIGHT_BACK_RIGHT_OFFSET = BR;
        this.CalibrationDetails.CORNER_HEIGHT_FRONT_LEFT_OFFSET = FL;
        this.CalibrationDetails.CORNER_HEIGHT_FRONT_RIGHT_OFFSET = FR;
        this.CalibrationDetails.ENTIRE_Z_HEIGHT_OFFSET = ZO;
      }
      int num = (int) this.WriteManualCommands(PrinterCompatibleString.Format("M577 X{0} Y{1} Z{2} E{3} F{4}", (object) BL, (object) BR, (object) FR, (object) FL, (object) ZO), "M578");
    }

    public void SetOffsetInformationZOnly(float ZO)
    {
      this.CalibrationDetails.ENTIRE_Z_HEIGHT_OFFSET = ZO;
      int num = (int) this.WriteManualCommands(PrinterCompatibleString.Format("M577 F{0}", (object) ZO), "M578");
    }

    public bool SetCalibrationOffset(float offset)
    {
      if (!this.CalibrationDetails.UsesCalibrationOffset)
        return false;
      this.CalibrationDetails.CALIBRATION_OFFSET = offset;
      int num = (int) this.WriteManualCommands(PrinterCompatibleString.Format("M577 I{0}", (object) offset), "M578");
      return true;
    }

    public void SetBacklashValues(BacklashSettings backlash)
    {
      int num = (int) this.WriteManualCommands(PrinterCompatibleString.Format("M571 X{0} Y{1}", (object) backlash.backlash_x, (object) backlash.backlash_y), PrinterCompatibleString.Format("M580 X{0}", (object) backlash.backlash_speed), "M572", "M581");
    }

    public void PrintBacklashPrint(string user)
    {
      FilamentSpool.TypeEnum filamentType = this.KnownFilament.filament_type;
      FilamentProfile filamentProfile = FilamentProfile.CreateFilamentProfile(this.KnownFilament, (PrinterProfile) this.MyPrinterProfile);
      this.AddJob(new JobParams("backlash_calibration.gcode", "Spooler Inserted Job", "null", filamentType, 0.0f, 0.0f)
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
      if (check && this.KnownFilament == filament)
        return;
      this.KnownFilament.estimated_filament_length_printed = filament.estimated_filament_length_printed;
      this.KnownFilament.filament_color_code = filament.filament_color_code;
      this.KnownFilament.filament_location = filament.filament_location;
      this.KnownFilament.filament_size = filament.filament_size;
      this.KnownFilament.filament_temperature = filament.filament_temperature;
      this.KnownFilament.filament_type = filament.filament_type;
      this.KnownFilament.filament_uid = filament.filament_uid;
      int filamentType = (int) filament.filament_type;
      int num1 = filament.filament_temperature - 100;
      int filamentColorCode = (int) filament.filament_color_code;
      float filamentLengthPrinted = filament.estimated_filament_length_printed;
      int num2 = filamentType + (int) filament.filament_location * 64;
      int filamentSize = (int) filament.filament_size;
      int num3 = (int) this.WriteManualCommands(PrinterCompatibleString.Format("M575 P{0} S{1} E{2} T{3} I{4}", (object) num2, (object) filamentColorCode, (object) filamentLengthPrinted, (object) num1, (object) filamentSize));
      this.SetFilamentUID(filament.filament_uid);
    }

    public void PushFilamentUsedToPrinter()
    {
      FilamentSpool currentFilament = this.GetCurrentFilament();
      if (!((FilamentSpool) null != currentFilament) || currentFilament.filament_type == FilamentSpool.TypeEnum.NoFilament)
        return;
      this.SetFilamentInformation(this.GetCurrentFilament(), false);
    }

    public void SetFilamentUID(uint filamentUID)
    {
      int num = (int) this.WriteManualCommands(PrinterCompatibleString.Format("M570 P{0}", (object) BitConverter.ToInt32(BitConverter.GetBytes(filamentUID), 0)), "M576");
    }

    public FilamentSpool GetCurrentFilament()
    {
      return new FilamentSpool(this.KnownFilament);
    }

    private FilamentSpool KnownFilament
    {
      get
      {
        return this.MyPrinterInfo.filament_info;
      }
    }

    internal void SetBacklash(float X, float Y)
    {
      this.CalibrationDetails.BACKLASH_X = X;
      this.CalibrationDetails.BACKLASH_Y = Y;
      this.eeprom_mapping.SetFloat("BacklashX", X);
      this.eeprom_mapping.SetFloat("BacklashY", Y);
    }

    internal void SetBacklashSpeed(float F)
    {
      this.CalibrationDetails.BACKLASH_SPEED = F;
      this.eeprom_mapping.SetFloat("BacklashSpeed", F);
    }

    public CommandResult SetNozzleWidth(int iNozzleWidthMicrons)
    {
      if (!this.MyPrinterProfile.AccessoriesConstants.NozzleConstants.bHasInterchangeableNozzle)
        return CommandResult.Failed_FeatureNotAvailableOnPrinter;
      int num = (int) this.WriteManualCommands(string.Format("M582 S{0}", (object) iNozzleWidthMicrons));
      return CommandResult.Success;
    }

    public void SetToReady()
    {
      if (this.m_oJobController.HasJob && this.m_oJobController.Status == JobStatus.Paused)
        this.Status = PrinterStatus.Firmware_PrintingPaused;
      else
        this.Status = PrinterStatus.Firmware_Ready;
    }

    public override bool IsWorking
    {
      get
      {
        int num = 0;
        lock (this.manual_commands)
          num = this.manual_commands.Count;
        if (num > 0 || this.HasActiveJob)
          return true;
        if (!this.LastMessageClear)
          return !this.m_bDoStartupOnWait;
        return false;
      }
    }

    public override bool HasActiveJob
    {
      get
      {
        return this.m_oJobController.HasJob;
      }
    }

    public override bool IsPrinting
    {
      get
      {
        lock (this.threadsync)
          return this.m_oJobController.HasJob && this.MyPrinterInfo.Status != PrinterStatus.Firmware_PrintingPaused;
      }
    }

    public bool IsConnecting
    {
      get
      {
        return this.Status == PrinterStatus.Connecting;
      }
    }

    private bool IsRecovering
    {
      get
      {
        return PrinterStatus.Firmware_PowerRecovery == this.Status;
      }
    }

    public override int GetJobsCount()
    {
      lock (this.threadsync)
        return this.m_oJobController.HasJob ? 1 : 0;
    }

    private bool LastMessageClear
    {
      get
      {
        return this._last_message_clear.Value;
      }
      set
      {
        this._last_message_clear.Value = value;
        if (!value)
        {
          if (this.Status == PrinterStatus.Firmware_PrintingPaused)
          {
            this.Status = PrinterStatus.Firmware_PrintingPausedProcessing;
            this.oswPauseTimer.Stop();
          }
          else
          {
            if (this.IsPrinting || this.IsRecovering || this.Status != PrinterStatus.Firmware_Ready && this.Status != PrinterStatus.Firmware_Idle)
              return;
            this.Status = PrinterStatus.Firmware_Executing;
          }
        }
        else
        {
          if (this.IsConnecting)
            return;
          this._auto_resend_count.Value = 0;
          if (this.Status == PrinterStatus.Firmware_Calibrating)
            this.ClearCalibrationErrors();
          if (this.IsPausedorPausing || this.IsRecovering)
            return;
          this.Status = this.IsPrinting ? PrinterStatus.Firmware_Printing : PrinterStatus.Firmware_Ready;
        }
      }
    }

    private bool ResendLastCommand
    {
      get
      {
        return this._resend_last_command.Value;
      }
      set
      {
        this._resend_last_command.Value = value;
      }
    }

    private void ClearCalibrationErrors()
    {
      this.CalibrationDetails.Calibration_Valid = true;
      this.ExtruderDetails.Z_Valid = true;
      this.CalibrationDetails.G32_VERSION = (int) byte.MaxValue;
    }

    public void SetRepetierProtocol(int protocol)
    {
      this.HardwareDetails.repetier_protocol = protocol;
    }

    public override bool Idle
    {
      get
      {
        return this.PrinterIdle;
      }
    }

    private bool PrinterIdle
    {
      get
      {
        return this._printer_idle.Value;
      }
      set
      {
        this._printer_idle.Value = value;
      }
    }

    public PrinterInfo CurrentPrinterInfo
    {
      get
      {
        return new PrinterInfo(this.MyPrinterInfo);
      }
    }

    public bool BoundsCheckingEnabled
    {
      get
      {
        return this.__boundsCheckingEnabled.Value;
      }
      set
      {
        this.__boundsCheckingEnabled.Value = value;
      }
    }

    public EEPROMMapping EEPROM
    {
      get
      {
        return this.eeprom_mapping;
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
