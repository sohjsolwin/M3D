// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Core.PrinterConnection
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Client;
using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;
using M3D.Spooling.Core.Controllers;
using M3D.Spooling.Core.Controllers.Plugins;
using M3D.Spooling.Printer_Profiles;
using M3D.Spooling.Sockets;
using RepetierHost.model;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace M3D.Spooling.Core
{
  internal class PrinterConnection : PublicPrinterConnection
  {
    private static ConcurrentDictionary<string, PrinterConnection.ConnectAttemptData> serial_failed_list = new ConcurrentDictionary<string, PrinterConnection.ConnectAttemptData>();
    internal ThreadSafeVariable<uint> CurrentRPC_id = new ThreadSafeVariable<uint>(0U);
    private ThreadSafeVariable<BaseController> m_oController = new ThreadSafeVariable<BaseController>((BaseController) null);
    private object shutdownlock = new object();
    private object statusthreadsync = new object();
    private Stopwatch lockResetTimer = new Stopwatch();
    private string lastError = string.Empty;
    private ThreadSafeVariable<bool> shared_shutdown;
    private bool hasbeenshutdown;
    public bool TrackStateChanges;
    private Logger internal_logger;
    private Guid lockID;
    private Guid clientID;
    private object lock_sync;
    private PrinterConnection.LockStatus lockStatus;
    private SharedShutdownThread serialThread;
    protected IBroadcastServer broadcastserver;
    private InternalPrinterProfile MyPrinterProfile;
    private PowerRecoveryPlugin m_oPowerRecoveryPlugin;
    private ISerialPortIo _safeSerialPort;
    private PrinterInfo printerInfo;

    public CommandResult SaveEEPROMDataToXMLFile(string filename)
    {
      FirmwareController firmwareController = this.GetFirmwareController();
      if (firmwareController == null)
        return CommandResult.Failed_NotInFirmware;
      return firmwareController.eeprom_mapping.SaveToXMLFile(filename) ? CommandResult.Success : CommandResult.Failed_Argument;
    }

    public CommandResult KeepLockAlive()
    {
      if (this.lockStatus != PrinterConnection.LockStatus.Locked)
        return CommandResult.Failed_PrinterDoesNotHaveLock;
      lock (this.lockResetTimer)
      {
        if (this.lockResetTimer.IsRunning)
          this.lockResetTimer.Restart();
      }
      return CommandResult.Success;
    }

    public CommandResult AddPrintJob(string user, JobParams jobParam)
    {
      int num1 = (int) this.ReleaseLock(this.lockID);
      if (this.m_oPowerRecoveryPlugin != null)
      {
        int num2 = (int) this.m_oPowerRecoveryPlugin.ClearPowerRecoveryFault(false);
      }
      FirmwareController firmwareController = this.GetFirmwareController();
      if (firmwareController == null)
        return CommandResult.Failed_NotInFirmware;
      if (firmwareController.IsPrinting || firmwareController.IsPausedorPausing)
        return CommandResult.Failed_ThePrinterIsPrintingOrPaused;
      firmwareController.AddPrintJob(user, jobParam);
      return CommandResult.SuccessfullyReceived;
    }

    public CommandResult WriteManualCommands(params string[] commands)
    {
      if (this.ControllerSelf != null)
        return this.ControllerSelf.WriteManualCommands(commands);
      return CommandResult.Failed_NoAvailableController;
    }

    public CommandResult WriteManualCommands(string dummy, params string[] commands)
    {
      return this.WriteManualCommands(commands);
    }

    public CommandResult AbortPrint()
    {
      FirmwareController firmwareController = this.GetFirmwareController();
      if (firmwareController == null)
        return CommandResult.Failed_NotInFirmware;
      firmwareController.AbortPrint();
      return CommandResult.Success;
    }

    public CommandResult ContinuePrint()
    {
      int num = (int) this.ReleaseLock(this.lockID);
      FirmwareController firmwareController = this.GetFirmwareController();
      if (firmwareController != null)
        return firmwareController.ContinuePrint();
      return CommandResult.Failed_NotInFirmware;
    }

    public CommandResult PausePrint()
    {
      FirmwareController firmwareController = this.GetFirmwareController();
      if (firmwareController != null)
        return firmwareController.PausePrint();
      return CommandResult.Failed_NotInFirmware;
    }

    public CommandResult ClearWarning()
    {
      FirmwareController firmwareController = this.GetFirmwareController();
      if (firmwareController == null)
        return CommandResult.Failed_NotInFirmware;
      firmwareController.ClearWarning();
      return CommandResult.Success;
    }

    public CommandResult SetTemperatureWhilePrinting(int temperature)
    {
      FirmwareController firmwareController = this.GetFirmwareController();
      if (firmwareController == null)
        return CommandResult.Failed_NotInFirmware;
      if (firmwareController.Status != PrinterStatus.Firmware_Printing)
        return CommandResult.Failed_PrinterNotPrinting;
      int num = (int) firmwareController.WriteManualCommands(string.Format("M104 S{0}", (object) temperature));
      return CommandResult.Success;
    }

    public CommandResult PrintBacklashPrint(string user)
    {
      int num = (int) this.ReleaseLock(this.lockID);
      FirmwareController firmwareController = this.GetFirmwareController();
      if (firmwareController == null)
        return CommandResult.Failed_NotInFirmware;
      if (firmwareController.IsPrinting || firmwareController.IsPausedorPausing)
        return CommandResult.Failed_ThePrinterIsPrintingOrPaused;
      firmwareController.PrintBacklashPrint(user);
      return CommandResult.SuccessfullyReceived;
    }

    public CommandResult KillJobs()
    {
      FirmwareController firmwareController = this.GetFirmwareController();
      if (firmwareController == null)
        return CommandResult.Failed_NotInFirmware;
      firmwareController.KillJobs();
      return CommandResult.Success;
    }

    public CommandResult UpdateFirmware()
    {
      int num = (int) this.ReleaseLock(this.lockID);
      if (this.ControllerSelf == null)
        return CommandResult.Failed_NoAvailableController;
      this.ControllerSelf.UpdateFirmware();
      return CommandResult.Success;
    }

    public void SendInterrupted(string message)
    {
      if (this.lockStatus != PrinterConnection.LockStatus.Locked)
        return;
      this.BroadcastServer.SendMessageToClient(this.clientID, message);
    }

    public CommandResult SetCalibrationOffset(float offset)
    {
      FirmwareController firmwareController = this.GetFirmwareController();
      if (firmwareController == null)
        return CommandResult.Failed_NotInFirmware;
      return firmwareController.SetCalibrationOffset(offset) ? CommandResult.Success : CommandResult.Failed_Argument;
    }

    public CommandResult SetOffsetInformation(BedOffsets Off)
    {
      FirmwareController firmwareController = this.GetFirmwareController();
      if (firmwareController == null)
        return CommandResult.Failed_NotInFirmware;
      firmwareController.SetOffsetInformation(Off);
      return CommandResult.Success;
    }

    public CommandResult SetBacklashValues(BacklashSettings backlash)
    {
      FirmwareController firmwareController = this.GetFirmwareController();
      if (firmwareController == null)
        return CommandResult.Failed_NotInFirmware;
      firmwareController.SetBacklashValues(backlash);
      return CommandResult.Success;
    }

    public CommandResult SendEmergencyStop()
    {
      FirmwareController firmwareController = this.GetFirmwareController();
      if (firmwareController == null)
        return CommandResult.Failed_NotInFirmware;
      firmwareController.SendEmergencyStop();
      return CommandResult.SuccessfullyReceived;
    }

    public CommandResult SetFilamentInformation(FilamentSpool filament)
    {
      FirmwareController firmwareController = this.GetFirmwareController();
      if (firmwareController == null)
        return CommandResult.Failed_NotInFirmware;
      firmwareController.SetFilamentInformation(filament, true);
      return CommandResult.Success;
    }

    public CommandResult SetNozzleWidth(int iNozzleWidthMicrons)
    {
      FirmwareController firmwareController = this.GetFirmwareController();
      if (firmwareController != null)
        return firmwareController.SetNozzleWidth(iNozzleWidthMicrons);
      return CommandResult.Failed_NotInFirmware;
    }

    public CommandResult AddUpdateKeyValuePair(string key, string value)
    {
      if (this.ControllerSelf == null)
        return CommandResult.Failed_NoAvailableController;
      if (this.printerInfo.persistantData.SavedData.ContainsKey(key))
        this.printerInfo.persistantData.SavedData[key] = value;
      else
        this.printerInfo.persistantData.SavedData.Add(key, value);
      this.ControllerSelf.SavePersistantData();
      return CommandResult.Success;
    }

    public CommandResult ClearPowerRecoveryFault()
    {
      if (this.m_oPowerRecoveryPlugin != null)
        return this.m_oPowerRecoveryPlugin.ClearPowerRecoveryFault(true);
      return CommandResult.Failed_NotInFirmware;
    }

    public CommandResult RecoveryPrintFromPowerFailure(bool bHomingRequired)
    {
      if (this.m_oPowerRecoveryPlugin == null)
        return CommandResult.Failed_NotInFirmware;
      int num = (int) this.m_oPowerRecoveryPlugin.RecoveryPrintFromPowerFailure(bHomingRequired);
      if (num != 0)
        return (CommandResult) num;
      this.Status = PrinterStatus.Firmware_PowerRecovery;
      this.BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.PowerOutageRecovery, this.Info.serial_number, (string) null).Serialize());
      return (CommandResult) num;
    }

    public CommandResult WriteSerialdata(string base64data)
    {
      BootloaderController bootloaderController = this.GetBootloaderController();
      if (bootloaderController == null)
        return CommandResult.Failed_NotInBootloader;
      bootloaderController.WriteSerialdata(Convert.FromBase64String(base64data));
      return CommandResult.Success;
    }

    public CommandResult WriteSerialdata(string base64data, int getbytes)
    {
      BootloaderController bootloaderController = this.GetBootloaderController();
      if (bootloaderController == null)
        return CommandResult.Failed_NotInBootloader;
      bootloaderController.WriteSerialdata(Convert.FromBase64String(base64data), getbytes);
      return CommandResult.Success;
    }

    public CommandResult GotoBootloader()
    {
      int num = (int) this.ReleaseLock(this.lockID);
      FirmwareController firmwareController = this.GetFirmwareController();
      if (firmwareController == null)
        return CommandResult.Failed_NotInFirmware;
      firmwareController.GotoBootloader();
      return CommandResult.Success;
    }

    public CommandResult GotoFirmware()
    {
      int num = (int) this.ReleaseLock(this.lockID);
      BootloaderController bootloaderController = this.GetBootloaderController();
      if (bootloaderController == null)
        return CommandResult.Failed_NotInBootloader;
      bootloaderController.GotoFirmware();
      return CommandResult.Success;
    }

    private void InitConnection()
    {
      this.shared_shutdown = new ThreadSafeVariable<bool>(false);
      this.internal_logger = new Logger(this.shared_shutdown);
      this.printerInfo = new PrinterInfo();
      this.CurrentRPC_id.Value = this.printerInfo.synchronization.LastCompletedRPCID = (uint) (SpoolerServer.RandomGenerator.Next() % (int) byte.MaxValue);
      this.printerInfo.hardware.com_port = this._safeSerialPort.PortName;
      this.Info.serial_number = PrinterSerialNumber.Undefined;
      this.Status = PrinterStatus.Uninitialized;
      this.lockID = Guid.Empty;
      this.clientID = Guid.Empty;
      this.lockStatus = PrinterConnection.LockStatus.Unlocked;
      this.lock_sync = new object();
    }

    public PrinterConnection(string com_port)
    {
      object portThreadSync = new object();
      this._safeSerialPort = (ISerialPortIo) new SafeSerialPort(com_port, portThreadSync)
      {
        Parity = Parity.None,
        StopBits = StopBits.One,
        Handshake = Handshake.None
      };
      this.InitConnection();
    }

    public void SetPrinterProfile(InternalPrinterProfile profile)
    {
      if (this.MyPrinterProfile != null)
        throw new InvalidOperationException("Profile can only be set once");
      if (profile == null)
        throw new ArgumentNullException("profile can not equal null");
      this.MyPrinterProfile = profile;
      this.printerInfo.ProfileName = this.MyPrinterProfile.ProfileName;
    }

    public void StartSerialProcessing()
    {
      if (this.serialThread != null)
        return;
      this.serialThread = new SharedShutdownThread(new SharedShutdownThreadStart(this.SerialProcessingThread), this.shared_shutdown, PrinterCompatibleString.PRINTER_CULTURE);
      this.serialThread.DelayBetweenIterations = 1;
      this.serialThread.Name = "Serial";
      this.serialThread.Priority = ThreadPriority.AboveNormal;
      this.serialThread.Start();
    }

    public void RequestFastSerialProcessing(bool bFastModeRequested)
    {
      if (bFastModeRequested)
        this.serialThread.DelayBetweenIterations = 0;
      else
        this.serialThread.DelayBetweenIterations = 1;
    }

    private bool SerialProcessingThread()
    {
      if (this.Info.Status == PrinterStatus.Uninitialized)
        return this.ProcessHandshakingPrinter();
      if (this.Info.Status == PrinterStatus.Error_PrinterNotAlive)
        return false;
      return this.ProcessConnectedPrinter();
    }

    private bool ProcessHandshakingPrinter()
    {
      if (!this.DoInitialHandShaking())
      {
        if (!PrinterConnection.serial_failed_list.ContainsKey(this.ComPort))
        {
          PrinterConnection.ConnectAttemptData connectAttemptData;
          connectAttemptData.time = DateTime.Now;
          connectAttemptData.message_sent = false;
          PrinterConnection.serial_failed_list.TryAdd(this.ComPort, connectAttemptData);
        }
        else
        {
          PrinterConnection.ConnectAttemptData comparisonValue;
          if (PrinterConnection.serial_failed_list.TryGetValue(this.ComPort, out comparisonValue) && ((DateTime.Now - comparisonValue.time).Seconds > 30 && !comparisonValue.message_sent))
          {
            PrinterConnection.ConnectAttemptData newValue;
            newValue.time = comparisonValue.time;
            newValue.message_sent = true;
            PrinterConnection.serial_failed_list.TryUpdate(this.ComPort, newValue, comparisonValue);
            SpoolerMessage spoolerMessage = new SpoolerMessage(MessageType.FirmwareErrorCyclePower, new PrinterSerialNumber("0000000000000000"), "null");
            if (this.broadcastserver != null)
              this.broadcastserver.BroadcastMessage(spoolerMessage.Serialize());
          }
        }
        this.IsAlive = false;
      }
      else if (PrinterConnection.serial_failed_list.ContainsKey(this.ComPort))
      {
        PrinterConnection.ConnectAttemptData connectAttemptData;
        PrinterConnection.serial_failed_list.TryRemove(this.ComPort, out connectAttemptData);
      }
      return this.IsAlive;
    }

    private bool ProcessConnectedPrinter()
    {
      try
      {
        if (this.Status == PrinterStatus.Error_PrinterNotAlive)
          return false;
        if (!this.IsOpen)
          this.Status = PrinterStatus.Error_PrinterNotAlive;
        BaseController controllerSelf = this.ControllerSelf;
        if (controllerSelf != null)
        {
          controllerSelf.DoConnectionLogic();
          if (!controllerSelf.IsWorking || this.Status == PrinterStatus.Firmware_PrintingPaused)
          {
            uint num = this.CurrentRPC_id.Value;
            if (num != 0U && (int) num != (int) this.Info.synchronization.LastCompletedRPCID)
              this.Info.synchronization.LastCompletedRPCID = num;
          }
          else
          {
            lock (this.lockResetTimer)
            {
              if (this.lockResetTimer.IsRunning)
                this.lockResetTimer.Restart();
            }
          }
          if (this.lockStatus == PrinterConnection.LockStatus.Pending && this.Status != PrinterStatus.Connecting && (!controllerSelf.IsWorking && !controllerSelf.IsPrinting || this.Status == PrinterStatus.Firmware_PrintingPaused))
          {
            this.lockStatus = PrinterConnection.LockStatus.Locked;
            this.Info.synchronization.Locked = true;
            this.BroadcastServer.SendMessageToClient(this.clientID, new SpoolerMessage(MessageType.LockConfirmed, this.Info.serial_number, this.lockID.ToString()).Serialize());
          }
          if (controllerSelf.Idle && this.lockStatus == PrinterConnection.LockStatus.Locked)
          {
            lock (this.lockResetTimer)
            {
              if (!this.lockResetTimer.IsRunning)
                this.lockResetTimer.Restart();
              if (this.lockResetTimer.Elapsed.TotalSeconds > 30.0)
                this.DoBreakLock();
            }
          }
          else
          {
            lock (this.lockResetTimer)
            {
              if (this.lockResetTimer.IsRunning)
                this.lockResetTimer.Stop();
            }
          }
        }
      }
      catch (Exception ex)
      {
        if (ex.Message.ToString().ToLower() != "unable to write program memory.  the serial port is not open.")
          ErrorLogger.LogException("PrinterConnection Exception", ex);
        this.Shutdown();
        return false;
      }
      return true;
    }

    public void InitializeController(IBroadcastServer broadcastserver)
    {
      this.broadcastserver = broadcastserver;
    }

    ~PrinterConnection()
    {
      this.Shutdown();
      this.shared_shutdown.Value = true;
    }

    public void Shutdown()
    {
      this.Shutdown(0);
    }

    public void Shutdown(int sleep_delay)
    {
      bool flag;
      lock (this.shutdownlock)
      {
        flag = !this.hasbeenshutdown;
        this.hasbeenshutdown = true;
      }
      if (!flag)
        return;
      this.InternalLogger.Shutdown();
      if (this.ControllerSelf != null)
        this.ControllerSelf.Shutdown();
      this.Status = PrinterStatus.Error_PrinterNotAlive;
      Thread.Sleep(sleep_delay);
      this.shared_shutdown.Value = true;
    }

    public string Serialize()
    {
      if (this.PrinterInfo.InBootloaderMode && SpoolerServer.AUTO_UPDATE_FIRMWARE || this.PrinterInfo.Status == PrinterStatus.Bootloader_StartingUp)
        return (string) null;
      return this.PrinterInfo.Serialize();
    }

    public bool DoInitialHandShaking()
    {
      string str1 = "";
      int result = 0;
      Thread.Sleep(1000);
      ASCIIEncoding asciiEncoding = new ASCIIEncoding();
      try
      {
        Stopwatch stopwatch1 = new Stopwatch();
        stopwatch1.Stop();
        stopwatch1.Reset();
        stopwatch1.Start();
        Stopwatch stopwatch2 = new Stopwatch();
        stopwatch2.Stop();
        stopwatch2.Reset();
        stopwatch2.Start();
        this.WriteToSerial(asciiEncoding.GetBytes("M115\r\n"));
        this.InternalLogger.WriteLog("<< M115 :ASCII:", Logger.TextType.Write);
        bool flag1 = true;
        bool flag2 = false;
        bool flag3 = false;
        string input = "";
        while (!this.shared_shutdown.Value)
        {
          if (flag1 && stopwatch2.ElapsedMilliseconds > 2000L)
          {
            if (!flag2)
            {
              if (!this.WriteToSerial(asciiEncoding.GetBytes("M115\r\n")))
                return false;
              this.InternalLogger.WriteLog("<< M115 :ASCII:(Resend)", Logger.TextType.Write);
              stopwatch2.Stop();
              stopwatch2.Reset();
              stopwatch2.Start();
              flag2 = true;
            }
            else
              flag3 = true;
          }
          input += this.ReadExisting();
          Match match = Regex.Match(input, "B\\d+", RegexOptions.CultureInvariant);
          if (match.Success)
          {
            string str2 = match.Value;
            if (!int.TryParse(str2.Substring(1), out result))
              result = 0;
            stopwatch1.Stop();
            str1 = "";
            this.InternalLogger.WriteLog(">> " + str2, Logger.TextType.Read);
            this.ControllerSelf = (BaseController) new BootloaderController(result, this, this.printerInfo, this.internal_logger, this.shared_shutdown, this.broadcastserver, this.MyPrinterProfile);
            return true;
          }
          int startIndex = input.IndexOf("ok");
          if (startIndex >= 0)
          {
            if (flag1)
              return false;
            stopwatch1.Stop();
            this.ControllerSelf = (BaseController) new FirmwareController(input.Substring(startIndex), this, this.printerInfo, this.internal_logger, this.shared_shutdown, this.broadcastserver, this.MyPrinterProfile);
            this.RegisterFirmwarePlugins();
            return true;
          }
          int length = input.IndexOf('\n');
          if (length >= 0)
          {
            string str2 = input.Substring(0, length);
            input = input.Substring(length + 1);
            this.InternalLogger.WriteLog(">> " + str2, Logger.TextType.Read);
            if (str2.Contains("e"))
              flag3 = true;
          }
          if (flag3)
          {
            flag3 = false;
            input = "";
            if (!this.WriteToSerial(new GCode() { M = (ushort) 115 }.getBinary(2)))
              return false;
            this.InternalLogger.WriteLog("<< M115", Logger.TextType.Write);
            flag1 = false;
          }
        }
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException("Exception in PrinterConnector.DoInitialHandshaking 3 " + ex.Message, ex);
        return false;
      }
      return false;
    }

    private FirmwareController GetFirmwareController()
    {
      BaseController controllerSelf = this.ControllerSelf;
      if (controllerSelf != null)
        return controllerSelf as FirmwareController;
      return (FirmwareController) null;
    }

    private BootloaderController GetBootloaderController()
    {
      BaseController controllerSelf = this.ControllerSelf;
      if (controllerSelf != null)
        return controllerSelf as BootloaderController;
      return (BootloaderController) null;
    }

    private BaseController ControllerSelf
    {
      get
      {
        return this.m_oController.Value;
      }
      set
      {
        this.m_oController.Value = value;
      }
    }

    public bool IsAlive
    {
      get
      {
        return (uint) this.printerInfo.Status > 0U;
      }
      set
      {
        if (value)
          return;
        this.printerInfo.Status = PrinterStatus.Error_PrinterNotAlive;
      }
    }

    public string SerialNumber
    {
      get
      {
        return this.printerInfo.serial_number.ToString();
      }
    }

    public string ComPort
    {
      get
      {
        return this.printerInfo.hardware.com_port;
      }
    }

    public PrinterInfo PrinterInfo
    {
      get
      {
        return this.printerInfo;
      }
    }

    public ISerialPortIo SerialPort
    {
      get
      {
        return this._safeSerialPort;
      }
    }

    public string LastError
    {
      get
      {
        return this.lastError;
      }
    }

    public bool IsWorking
    {
      get
      {
        if (this.ControllerSelf == null)
          return false;
        return this.ControllerSelf.IsWorking;
      }
    }

    public bool HasActiveJob
    {
      get
      {
        if (this.ControllerSelf == null)
          return false;
        return this.ControllerSelf.HasActiveJob;
      }
    }

    public bool IsPrinting
    {
      get
      {
        if (this.ControllerSelf == null)
          return false;
        return this.ControllerSelf.IsPrinting;
      }
    }

    public bool IsPaused
    {
      get
      {
        if (this.ControllerSelf == null)
          return false;
        return this.ControllerSelf.IsPaused;
      }
    }

    public int GetJobsCount()
    {
      if (this.ControllerSelf == null)
        return 0;
      return this.ControllerSelf.GetJobsCount();
    }

    public bool Ready
    {
      get
      {
        if (!this.Info.InFirmwareMode)
          return this.Info.InBootloaderMode;
        return true;
      }
    }

    public PrinterStatus Status
    {
      get
      {
        lock (this.statusthreadsync)
          return this.Info.Status;
      }
      protected set
      {
        lock (this.statusthreadsync)
          this.Info.Status = value;
      }
    }

    public PrinterSerialNumber MySerialNumber
    {
      get
      {
        return this.Info.serial_number;
      }
      set
      {
        this.Info.serial_number = value;
      }
    }

    private PrinterInfo Info
    {
      get
      {
        return this.PrinterInfo;
      }
    }

    public PrinterInfo MyInfo
    {
      get
      {
        return new PrinterInfo(this.Info);
      }
    }

    public void WriteLog(string text, Logger.TextType type)
    {
      this.internal_logger.WriteLog(text, type);
    }

    internal Logger InternalLogger
    {
      get
      {
        return this.internal_logger;
      }
    }

    public void OnClientRemoved(Guid guid)
    {
      if (!(this.clientID == guid))
        return;
      this.lockStatus = PrinterConnection.LockStatus.Unlocked;
      this.Info.synchronization.Locked = false;
      this.clientID = Guid.Empty;
      this.lockID = Guid.Empty;
    }

    public CommandResult AcquireLock(Guid clientID)
    {
      lock (this.lock_sync)
      {
        if (this.IsPrinting && !this.IsPaused)
          return CommandResult.Failed_CannotLockWhilePrinting;
        if (this.lockStatus == PrinterConnection.LockStatus.Locked && !(this.clientID == clientID))
          return CommandResult.Failed_PrinterAlreadyLocked;
        this.clientID = clientID;
        this.lockID = Guid.NewGuid();
        this.lockStatus = PrinterConnection.LockStatus.Pending;
        return CommandResult.Pending;
      }
    }

    public CommandResult BreakLock()
    {
      if (this.Status != PrinterStatus.Firmware_Idle)
        return CommandResult.Failed_PreviousCommandNotCompleted;
      this.DoBreakLock();
      return CommandResult.LockForcedOpen;
    }

    public CommandResult ReleaseLock(Guid lockID)
    {
      CommandResult commandResult = CommandResult.Failed_PrinterDoesNotHaveLock;
      lock (this.lock_sync)
      {
        if (this.lockStatus != PrinterConnection.LockStatus.Locked)
        {
          if (this.lockStatus != PrinterConnection.LockStatus.Pending)
            goto label_11;
        }
        if (this.lockID == lockID)
        {
          this.lockStatus = PrinterConnection.LockStatus.Unlocked;
          this.Info.synchronization.Locked = false;
          this.clientID = Guid.Empty;
          lockID = Guid.Empty;
          commandResult = CommandResult.Success_LockReleased;
        }
        BaseController controllerSelf = this.ControllerSelf;
        if (controllerSelf != null)
        {
          FirmwareController firmwareController = controllerSelf as FirmwareController;
          if (firmwareController != null)
            firmwareController.BoundsCheckingEnabled = true;
        }
      }
label_11:
      return commandResult;
    }

    public CommandResult VerifyLock(Guid lockID)
    {
      CommandResult commandResult = CommandResult.Failed_PrinterDoesNotHaveLock;
      lock (this.lock_sync)
      {
        if (this.lockStatus == PrinterConnection.LockStatus.Locked)
        {
          if (this.lockID == lockID)
            commandResult = CommandResult.Success;
        }
        else if (this.lockStatus == PrinterConnection.LockStatus.Pending)
        {
          if (this.lockID == lockID)
            commandResult = CommandResult.Failed_LockNotReady;
        }
        else if (lockID == Guid.Empty)
          commandResult = CommandResult.Success;
      }
      return commandResult;
    }

    private void DoBreakLock()
    {
      this.BroadcastServer.SendMessageToClient(this.clientID, new SpoolerMessage(MessageType.LockResult, this.Info.serial_number, 0.ToString("D8") + CommandResult.LockLost_TimedOut.ToString()).Serialize());
      int num = (int) this.ReleaseLock(this.lockID);
    }

    public Guid MyLock
    {
      get
      {
        return this.lockID;
      }
    }

    public bool ConnectTo()
    {
      try
      {
        return this.SerialConnect(115200);
      }
      catch (Exception ex)
      {
        this.lastError = ex.Message + "\n" + ex.StackTrace;
        return false;
      }
    }

    public bool WriteToSerial(byte[] command)
    {
      bool flag = true;
      try
      {
        this._safeSerialPort.Write(command, 0, command.Length);
      }
      catch (ThreadAbortException ex)
      {
        throw ex;
      }
      catch (Exception ex)
      {
        flag = false;
      }
      return flag;
    }

    public string ReadExisting()
    {
      if (this._safeSerialPort.BytesToRead > 0)
        return this._safeSerialPort.ReadExisting();
      return (string) null;
    }

    public bool IsOpen
    {
      get
      {
        return this._safeSerialPort.IsOpen;
      }
    }

    private bool SerialConnect(int baudrate)
    {
      try
      {
        this._safeSerialPort.Open();
      }
      catch (Exception ex)
      {
        this.lastError = ex.Message;
        return false;
      }
      try
      {
        this._safeSerialPort.WriteTimeout = 5000;
        this._safeSerialPort.ReadTimeout = 5000;
      }
      catch (Exception ex)
      {
        this._safeSerialPort.Dispose();
        this.lastError = ex.Message;
        return false;
      }
      Thread.Sleep(250);
      return true;
    }

    public CommandResult RegisterExternalPluginGCodes(string ID, string[] gCodeList)
    {
      FirmwareController firmwareController = this.GetFirmwareController();
      if (firmwareController != null)
        return firmwareController.RegisterExternalPluginGCodes(ID, gCodeList);
      return CommandResult.Failed_NotInFirmware;
    }

    public void BroadcastPluginMessage(SpoolerMessage message)
    {
      if (message.Type != MessageType.PluginMessage)
        return;
      if (this.clientID != Guid.Empty)
        this.BroadcastServer.SendMessageToClient(this.clientID, message.Serialize());
      else
        this.BroadcastServer.BroadcastMessage(message.Serialize());
    }

    private void RegisterFirmwarePlugins()
    {
      FirmwareController firmwareController = this.GetFirmwareController();
      if (firmwareController == null)
        return;
      SDCardPlugin sdCardPlugin = new SDCardPlugin(this.PrinterInfo.accessories.SDCardStatus, new SDCardPlugin.ActiveSDPrintCallback(firmwareController.ConnectToActiveSDPrint));
      int num1 = (int) firmwareController.RegisterPlugin((FirmwareControllerPlugin) sdCardPlugin);
      this.m_oPowerRecoveryPlugin = new PowerRecoveryPlugin(this.PrinterInfo.powerRecovery, (IPublicFirmwareController) firmwareController, new PowerRecoveryPlugin.RecoverySpoolerPrintCallback(firmwareController.RecoverySpoolerPrintCallback));
      int num2 = (int) firmwareController.RegisterPlugin((FirmwareControllerPlugin) this.m_oPowerRecoveryPlugin);
    }

    protected IBroadcastServer BroadcastServer
    {
      get
      {
        return this.broadcastserver;
      }
    }

    private enum Mode
    {
      Unknown,
      Firmware,
      Bootloader,
    }

    public enum PrinterType
    {
      RepRapCompatiblePrinter,
      TheMicroI,
    }

    private struct ConnectAttemptData
    {
      public DateTime time;
      public bool message_sent;
    }

    private enum LockStatus
    {
      Unlocked,
      Pending,
      Locked,
    }
  }
}
