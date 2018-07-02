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
    private ThreadSafeVariable<BaseController> m_oController = new ThreadSafeVariable<BaseController>(null);
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
      FirmwareController firmwareController = GetFirmwareController();
      if (firmwareController == null)
      {
        return CommandResult.Failed_NotInFirmware;
      }

      return firmwareController.eeprom_mapping.SaveToXMLFile(filename) ? CommandResult.Success : CommandResult.Failed_Argument;
    }

    public CommandResult KeepLockAlive()
    {
      if (lockStatus != PrinterConnection.LockStatus.Locked)
      {
        return CommandResult.Failed_PrinterDoesNotHaveLock;
      }

      lock (lockResetTimer)
      {
        if (lockResetTimer.IsRunning)
        {
          lockResetTimer.Restart();
        }
      }
      return CommandResult.Success;
    }

    public CommandResult AddPrintJob(string user, JobParams jobParam)
    {
      var num1 = (int)ReleaseLock(lockID);
      if (m_oPowerRecoveryPlugin != null)
      {
        var num2 = (int)m_oPowerRecoveryPlugin.ClearPowerRecoveryFault(false);
      }
      FirmwareController firmwareController = GetFirmwareController();
      if (firmwareController == null)
      {
        return CommandResult.Failed_NotInFirmware;
      }

      if (firmwareController.IsPrinting || firmwareController.IsPausedorPausing)
      {
        return CommandResult.Failed_ThePrinterIsPrintingOrPaused;
      }

      firmwareController.AddPrintJob(user, jobParam);
      return CommandResult.SuccessfullyReceived;
    }

    public CommandResult WriteManualCommands(params string[] commands)
    {
      if (ControllerSelf != null)
      {
        return ControllerSelf.WriteManualCommands(commands);
      }

      return CommandResult.Failed_NoAvailableController;
    }

    public CommandResult WriteManualCommands(string dummy, params string[] commands)
    {
      return WriteManualCommands(commands);
    }

    public CommandResult AbortPrint()
    {
      FirmwareController firmwareController = GetFirmwareController();
      if (firmwareController == null)
      {
        return CommandResult.Failed_NotInFirmware;
      }

      firmwareController.AbortPrint();
      return CommandResult.Success;
    }

    public CommandResult ContinuePrint()
    {
      var num = (int)ReleaseLock(lockID);
      FirmwareController firmwareController = GetFirmwareController();
      if (firmwareController != null)
      {
        return firmwareController.ContinuePrint();
      }

      return CommandResult.Failed_NotInFirmware;
    }

    public CommandResult PausePrint()
    {
      FirmwareController firmwareController = GetFirmwareController();
      if (firmwareController != null)
      {
        return firmwareController.PausePrint();
      }

      return CommandResult.Failed_NotInFirmware;
    }

    public CommandResult ClearWarning()
    {
      FirmwareController firmwareController = GetFirmwareController();
      if (firmwareController == null)
      {
        return CommandResult.Failed_NotInFirmware;
      }

      firmwareController.ClearWarning();
      return CommandResult.Success;
    }

    public CommandResult SetTemperatureWhilePrinting(int temperature)
    {
      FirmwareController firmwareController = GetFirmwareController();
      if (firmwareController == null)
      {
        return CommandResult.Failed_NotInFirmware;
      }

      if (firmwareController.Status != PrinterStatus.Firmware_Printing)
      {
        return CommandResult.Failed_PrinterNotPrinting;
      }

      var num = (int) firmwareController.WriteManualCommands(string.Format("M104 S{0}", temperature));
      return CommandResult.Success;
    }

    public CommandResult PrintBacklashPrint(string user)
    {
      var num = (int)ReleaseLock(lockID);
      FirmwareController firmwareController = GetFirmwareController();
      if (firmwareController == null)
      {
        return CommandResult.Failed_NotInFirmware;
      }

      if (firmwareController.IsPrinting || firmwareController.IsPausedorPausing)
      {
        return CommandResult.Failed_ThePrinterIsPrintingOrPaused;
      }

      firmwareController.PrintBacklashPrint(user);
      return CommandResult.SuccessfullyReceived;
    }

    public CommandResult KillJobs()
    {
      FirmwareController firmwareController = GetFirmwareController();
      if (firmwareController == null)
      {
        return CommandResult.Failed_NotInFirmware;
      }

      firmwareController.KillJobs();
      return CommandResult.Success;
    }

    public CommandResult UpdateFirmware()
    {
      var num = (int)ReleaseLock(lockID);
      if (ControllerSelf == null)
      {
        return CommandResult.Failed_NoAvailableController;
      }

      ControllerSelf.UpdateFirmware();
      return CommandResult.Success;
    }

    public void SendInterrupted(string message)
    {
      if (lockStatus != PrinterConnection.LockStatus.Locked)
      {
        return;
      }

      BroadcastServer.SendMessageToClient(clientID, message);
    }

    public CommandResult SetCalibrationOffset(float offset)
    {
      FirmwareController firmwareController = GetFirmwareController();
      if (firmwareController == null)
      {
        return CommandResult.Failed_NotInFirmware;
      }

      return firmwareController.SetCalibrationOffset(offset) ? CommandResult.Success : CommandResult.Failed_Argument;
    }

    public CommandResult SetOffsetInformation(BedOffsets Off)
    {
      FirmwareController firmwareController = GetFirmwareController();
      if (firmwareController == null)
      {
        return CommandResult.Failed_NotInFirmware;
      }

      firmwareController.SetOffsetInformation(Off);
      return CommandResult.Success;
    }

    public CommandResult SetBacklashValues(BacklashSettings backlash)
    {
      FirmwareController firmwareController = GetFirmwareController();
      if (firmwareController == null)
      {
        return CommandResult.Failed_NotInFirmware;
      }

      firmwareController.SetBacklashValues(backlash);
      return CommandResult.Success;
    }

    public CommandResult SendEmergencyStop()
    {
      FirmwareController firmwareController = GetFirmwareController();
      if (firmwareController == null)
      {
        return CommandResult.Failed_NotInFirmware;
      }

      firmwareController.SendEmergencyStop();
      return CommandResult.SuccessfullyReceived;
    }

    public CommandResult SetFilamentInformation(FilamentSpool filament)
    {
      FirmwareController firmwareController = GetFirmwareController();
      if (firmwareController == null)
      {
        return CommandResult.Failed_NotInFirmware;
      }

      firmwareController.SetFilamentInformation(filament, true);
      return CommandResult.Success;
    }

    public CommandResult SetNozzleWidth(int iNozzleWidthMicrons)
    {
      FirmwareController firmwareController = GetFirmwareController();
      if (firmwareController != null)
      {
        return firmwareController.SetNozzleWidth(iNozzleWidthMicrons);
      }

      return CommandResult.Failed_NotInFirmware;
    }

    public CommandResult AddUpdateKeyValuePair(string key, string value)
    {
      if (ControllerSelf == null)
      {
        return CommandResult.Failed_NoAvailableController;
      }

      if (printerInfo.persistantData.SavedData.ContainsKey(key))
      {
        printerInfo.persistantData.SavedData[key] = value;
      }
      else
      {
        printerInfo.persistantData.SavedData.Add(key, value);
      }

      ControllerSelf.SavePersistantData();
      return CommandResult.Success;
    }

    public CommandResult ClearPowerRecoveryFault()
    {
      if (m_oPowerRecoveryPlugin != null)
      {
        return m_oPowerRecoveryPlugin.ClearPowerRecoveryFault(true);
      }

      return CommandResult.Failed_NotInFirmware;
    }

    public CommandResult RecoveryPrintFromPowerFailure(bool bHomingRequired)
    {
      if (m_oPowerRecoveryPlugin == null)
      {
        return CommandResult.Failed_NotInFirmware;
      }

      var num = (int)m_oPowerRecoveryPlugin.RecoveryPrintFromPowerFailure(bHomingRequired);
      if (num != 0)
      {
        return (CommandResult) num;
      }

      Status = PrinterStatus.Firmware_PowerRecovery;
      BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.PowerOutageRecovery, Info.serial_number, null).Serialize());
      return (CommandResult) num;
    }

    public CommandResult WriteSerialdata(string base64data)
    {
      BootloaderController bootloaderController = GetBootloaderController();
      if (bootloaderController == null)
      {
        return CommandResult.Failed_NotInBootloader;
      }

      bootloaderController.WriteSerialdata(Convert.FromBase64String(base64data));
      return CommandResult.Success;
    }

    public CommandResult WriteSerialdata(string base64data, int getbytes)
    {
      BootloaderController bootloaderController = GetBootloaderController();
      if (bootloaderController == null)
      {
        return CommandResult.Failed_NotInBootloader;
      }

      bootloaderController.WriteSerialdata(Convert.FromBase64String(base64data), getbytes);
      return CommandResult.Success;
    }

    public CommandResult GotoBootloader()
    {
      var num = (int)ReleaseLock(lockID);
      FirmwareController firmwareController = GetFirmwareController();
      if (firmwareController == null)
      {
        return CommandResult.Failed_NotInFirmware;
      }

      firmwareController.GotoBootloader();
      return CommandResult.Success;
    }

    public CommandResult GotoFirmware()
    {
      var num = (int)ReleaseLock(lockID);
      BootloaderController bootloaderController = GetBootloaderController();
      if (bootloaderController == null)
      {
        return CommandResult.Failed_NotInBootloader;
      }

      bootloaderController.GotoFirmware();
      return CommandResult.Success;
    }

    private void InitConnection()
    {
      shared_shutdown = new ThreadSafeVariable<bool>(false);
      internal_logger = new Logger(shared_shutdown);
      printerInfo = new PrinterInfo();
      CurrentRPC_id.Value = printerInfo.synchronization.LastCompletedRPCID = (uint) (SpoolerServer.RandomGenerator.Next() % byte.MaxValue);
      printerInfo.hardware.com_port = _safeSerialPort.PortName;
      Info.serial_number = PrinterSerialNumber.Undefined;
      Status = PrinterStatus.Uninitialized;
      lockID = Guid.Empty;
      clientID = Guid.Empty;
      lockStatus = PrinterConnection.LockStatus.Unlocked;
      lock_sync = new object();
    }

    public PrinterConnection(string com_port)
    {
      var portThreadSync = new object();
      _safeSerialPort = new SafeSerialPort(com_port, portThreadSync)
      {
        Parity = Parity.None,
        StopBits = StopBits.One,
        Handshake = Handshake.None
      };
      InitConnection();
    }

    public void SetPrinterProfile(InternalPrinterProfile profile)
    {
      if (MyPrinterProfile != null)
      {
        throw new InvalidOperationException("Profile can only be set once");
      }

      MyPrinterProfile = profile ?? throw new ArgumentNullException("profile can not equal null");
      printerInfo.ProfileName = MyPrinterProfile.ProfileName;
    }

    public void StartSerialProcessing()
    {
      if (serialThread != null)
      {
        return;
      }

      serialThread = new SharedShutdownThread(new SharedShutdownThreadStart(SerialProcessingThread), shared_shutdown, PrinterCompatibleString.PRINTER_CULTURE)
      {
        DelayBetweenIterations = 1,
        Name = "Serial",
        Priority = ThreadPriority.AboveNormal
      };
      serialThread.Start();
    }

    public void RequestFastSerialProcessing(bool bFastModeRequested)
    {
      if (bFastModeRequested)
      {
        serialThread.DelayBetweenIterations = 0;
      }
      else
      {
        serialThread.DelayBetweenIterations = 1;
      }
    }

    private bool SerialProcessingThread()
    {
      if (Info.Status == PrinterStatus.Uninitialized)
      {
        return ProcessHandshakingPrinter();
      }

      if (Info.Status == PrinterStatus.Error_PrinterNotAlive)
      {
        return false;
      }

      return ProcessConnectedPrinter();
    }

    private bool ProcessHandshakingPrinter()
    {
      if (!DoInitialHandShaking())
      {
        if (!serial_failed_list.ContainsKey(ComPort))
        {
          PrinterConnection.ConnectAttemptData connectAttemptData;
          connectAttemptData.time = DateTime.Now;
          connectAttemptData.message_sent = false;
          serial_failed_list.TryAdd(ComPort, connectAttemptData);
        }
        else
        {
          if (serial_failed_list.TryGetValue(ComPort, out ConnectAttemptData comparisonValue) && ((DateTime.Now - comparisonValue.time).Seconds > 30 && !comparisonValue.message_sent))
          {
            PrinterConnection.ConnectAttemptData newValue;
            newValue.time = comparisonValue.time;
            newValue.message_sent = true;
            serial_failed_list.TryUpdate(ComPort, newValue, comparisonValue);
            var spoolerMessage = new SpoolerMessage(MessageType.FirmwareErrorCyclePower, new PrinterSerialNumber("0000000000000000"), "null");
            if (broadcastserver != null)
            {
              broadcastserver.BroadcastMessage(spoolerMessage.Serialize());
            }
          }
        }
        IsAlive = false;
      }
      else if (serial_failed_list.ContainsKey(ComPort))
      {
        serial_failed_list.TryRemove(ComPort, out ConnectAttemptData connectAttemptData);
      }
      return IsAlive;
    }

    private bool ProcessConnectedPrinter()
    {
      try
      {
        if (Status == PrinterStatus.Error_PrinterNotAlive)
        {
          return false;
        }

        if (!IsOpen)
        {
          Status = PrinterStatus.Error_PrinterNotAlive;
        }

        BaseController controllerSelf = ControllerSelf;
        if (controllerSelf != null)
        {
          controllerSelf.DoConnectionLogic();
          if (!controllerSelf.IsWorking || Status == PrinterStatus.Firmware_PrintingPaused)
          {
            var num = CurrentRPC_id.Value;
            if (num != 0U && (int) num != (int)Info.synchronization.LastCompletedRPCID)
            {
              Info.synchronization.LastCompletedRPCID = num;
            }
          }
          else
          {
            lock (lockResetTimer)
            {
              if (lockResetTimer.IsRunning)
              {
                lockResetTimer.Restart();
              }
            }
          }
          if (lockStatus == PrinterConnection.LockStatus.Pending && Status != PrinterStatus.Connecting && (!controllerSelf.IsWorking && !controllerSelf.IsPrinting || Status == PrinterStatus.Firmware_PrintingPaused))
          {
            lockStatus = PrinterConnection.LockStatus.Locked;
            Info.synchronization.Locked = true;
            BroadcastServer.SendMessageToClient(clientID, new SpoolerMessage(MessageType.LockConfirmed, Info.serial_number, lockID.ToString()).Serialize());
          }
          if (controllerSelf.Idle && lockStatus == PrinterConnection.LockStatus.Locked)
          {
            lock (lockResetTimer)
            {
              if (!lockResetTimer.IsRunning)
              {
                lockResetTimer.Restart();
              }

              if (lockResetTimer.Elapsed.TotalSeconds > 30.0)
              {
                DoBreakLock();
              }
            }
          }
          else
          {
            lock (lockResetTimer)
            {
              if (lockResetTimer.IsRunning)
              {
                lockResetTimer.Stop();
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        if (ex.Message.ToString().ToLower() != "unable to write program memory.  the serial port is not open.")
        {
          ErrorLogger.LogException("PrinterConnection Exception", ex);
        }

        Shutdown();
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
      Shutdown();
      shared_shutdown.Value = true;
    }

    public void Shutdown()
    {
      Shutdown(0);
    }

    public void Shutdown(int sleep_delay)
    {
      bool flag;
      lock (shutdownlock)
      {
        flag = !hasbeenshutdown;
        hasbeenshutdown = true;
      }
      if (!flag)
      {
        return;
      }

      InternalLogger.Shutdown();
      if (ControllerSelf != null)
      {
        ControllerSelf.Shutdown();
      }

      Status = PrinterStatus.Error_PrinterNotAlive;
      Thread.Sleep(sleep_delay);
      shared_shutdown.Value = true;
    }

    public string Serialize()
    {
      if (PrinterInfo.InBootloaderMode && SpoolerServer.AUTO_UPDATE_FIRMWARE || PrinterInfo.Status == PrinterStatus.Bootloader_StartingUp)
      {
        return null;
      }

      return PrinterInfo.Serialize();
    }

    public bool DoInitialHandShaking()
    {
      var result = 0;
      Thread.Sleep(1000);
      var asciiEncoding = new ASCIIEncoding();
      try
      {
        var stopwatch1 = new Stopwatch();
        stopwatch1.Stop();
        stopwatch1.Reset();
        stopwatch1.Start();
        var stopwatch2 = new Stopwatch();
        stopwatch2.Stop();
        stopwatch2.Reset();
        stopwatch2.Start();
        WriteToSerial(asciiEncoding.GetBytes("M115\r\n"));
        InternalLogger.WriteLog("<< M115 :ASCII:", Logger.TextType.Write);
        var flag1 = true;
        var flag2 = false;
        var flag3 = false;
        var input = "";
        while (!shared_shutdown.Value)
        {
          if (flag1 && stopwatch2.ElapsedMilliseconds > 2000L)
          {
            if (!flag2)
            {
              if (!WriteToSerial(asciiEncoding.GetBytes("M115\r\n")))
              {
                return false;
              }

              InternalLogger.WriteLog("<< M115 :ASCII:(Resend)", Logger.TextType.Write);
              stopwatch2.Stop();
              stopwatch2.Reset();
              stopwatch2.Start();
              flag2 = true;
            }
            else
            {
              flag3 = true;
            }
          }
          input += ReadExisting();
          Match match = Regex.Match(input, "B\\d+", RegexOptions.CultureInvariant);
          if (match.Success)
          {
            var str2 = match.Value;
            if (!int.TryParse(str2.Substring(1), out result))
            {
              result = 0;
            }

            stopwatch1.Stop();
            InternalLogger.WriteLog(">> " + str2, Logger.TextType.Read);
            ControllerSelf = new BootloaderController(result, this, printerInfo, internal_logger, shared_shutdown, broadcastserver, MyPrinterProfile);
            return true;
          }
          var startIndex = input.IndexOf("ok");
          if (startIndex >= 0)
          {
            if (flag1)
            {
              return false;
            }

            stopwatch1.Stop();
            ControllerSelf = new FirmwareController(input.Substring(startIndex), this, printerInfo, internal_logger, shared_shutdown, broadcastserver, MyPrinterProfile);
            RegisterFirmwarePlugins();
            return true;
          }
          var length = input.IndexOf('\n');
          if (length >= 0)
          {
            var str2 = input.Substring(0, length);
            input = input.Substring(length + 1);
            InternalLogger.WriteLog(">> " + str2, Logger.TextType.Read);
            if (str2.Contains("e"))
            {
              flag3 = true;
            }
          }
          if (flag3)
          {
            flag3 = false;
            input = "";
            if (!WriteToSerial(new GCode() { M = 115 }.getBinary(2)))
            {
              return false;
            }

            InternalLogger.WriteLog("<< M115", Logger.TextType.Write);
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
      BaseController controllerSelf = ControllerSelf;
      if (controllerSelf != null)
      {
        return controllerSelf as FirmwareController;
      }

      return null;
    }

    private BootloaderController GetBootloaderController()
    {
      BaseController controllerSelf = ControllerSelf;
      if (controllerSelf != null)
      {
        return controllerSelf as BootloaderController;
      }

      return null;
    }

    private BaseController ControllerSelf
    {
      get
      {
        return m_oController.Value;
      }
      set
      {
        m_oController.Value = value;
      }
    }

    public bool IsAlive
    {
      get
      {
        return (uint)printerInfo.Status > 0U;
      }
      set
      {
        if (value)
        {
          return;
        }

        printerInfo.Status = PrinterStatus.Error_PrinterNotAlive;
      }
    }

    public string SerialNumber
    {
      get
      {
        return printerInfo.serial_number.ToString();
      }
    }

    public string ComPort
    {
      get
      {
        return printerInfo.hardware.com_port;
      }
    }

    public PrinterInfo PrinterInfo
    {
      get
      {
        return printerInfo;
      }
    }

    public ISerialPortIo SerialPort
    {
      get
      {
        return _safeSerialPort;
      }
    }

    public string LastError
    {
      get
      {
        return lastError;
      }
    }

    public bool IsWorking
    {
      get
      {
        if (ControllerSelf == null)
        {
          return false;
        }

        return ControllerSelf.IsWorking;
      }
    }

    public bool HasActiveJob
    {
      get
      {
        if (ControllerSelf == null)
        {
          return false;
        }

        return ControllerSelf.HasActiveJob;
      }
    }

    public bool IsPrinting
    {
      get
      {
        if (ControllerSelf == null)
        {
          return false;
        }

        return ControllerSelf.IsPrinting;
      }
    }

    public bool IsPaused
    {
      get
      {
        if (ControllerSelf == null)
        {
          return false;
        }

        return ControllerSelf.IsPaused;
      }
    }

    public int GetJobsCount()
    {
      if (ControllerSelf == null)
      {
        return 0;
      }

      return ControllerSelf.GetJobsCount();
    }

    public bool Ready
    {
      get
      {
        if (!Info.InFirmwareMode)
        {
          return Info.InBootloaderMode;
        }

        return true;
      }
    }

    public PrinterStatus Status
    {
      get
      {
        lock (statusthreadsync)
        {
          return Info.Status;
        }
      }
      protected set
      {
        lock (statusthreadsync)
        {
          Info.Status = value;
        }
      }
    }

    public PrinterSerialNumber MySerialNumber
    {
      get
      {
        return Info.serial_number;
      }
      set
      {
        Info.serial_number = value;
      }
    }

    private PrinterInfo Info
    {
      get
      {
        return PrinterInfo;
      }
    }

    public PrinterInfo MyInfo
    {
      get
      {
        return new PrinterInfo(Info);
      }
    }

    public void WriteLog(string text, Logger.TextType type)
    {
      internal_logger.WriteLog(text, type);
    }

    internal Logger InternalLogger
    {
      get
      {
        return internal_logger;
      }
    }

    public void OnClientRemoved(Guid guid)
    {
      if (!(clientID == guid))
      {
        return;
      }

      lockStatus = PrinterConnection.LockStatus.Unlocked;
      Info.synchronization.Locked = false;
      clientID = Guid.Empty;
      lockID = Guid.Empty;
    }

    public CommandResult AcquireLock(Guid clientID)
    {
      lock (lock_sync)
      {
        if (IsPrinting && !IsPaused)
        {
          return CommandResult.Failed_CannotLockWhilePrinting;
        }

        if (lockStatus == PrinterConnection.LockStatus.Locked && !(this.clientID == clientID))
        {
          return CommandResult.Failed_PrinterAlreadyLocked;
        }

        this.clientID = clientID;
        lockID = Guid.NewGuid();
        lockStatus = PrinterConnection.LockStatus.Pending;
        return CommandResult.Pending;
      }
    }

    public CommandResult BreakLock()
    {
      if (Status != PrinterStatus.Firmware_Idle)
      {
        return CommandResult.Failed_PreviousCommandNotCompleted;
      }

      DoBreakLock();
      return CommandResult.LockForcedOpen;
    }

    public CommandResult ReleaseLock(Guid lockID)
    {
      CommandResult commandResult = CommandResult.Failed_PrinterDoesNotHaveLock;
      lock (lock_sync)
      {
        if (lockStatus != PrinterConnection.LockStatus.Locked)
        {
          if (lockStatus != PrinterConnection.LockStatus.Pending)
          {
            goto label_11;
          }
        }
        if (this.lockID == lockID)
        {
          lockStatus = PrinterConnection.LockStatus.Unlocked;
          Info.synchronization.Locked = false;
          clientID = Guid.Empty;
          lockID = Guid.Empty;
          commandResult = CommandResult.Success_LockReleased;
        }
        BaseController controllerSelf = ControllerSelf;
        if (controllerSelf != null)
        {
          if (controllerSelf is FirmwareController firmwareController)
          {
            firmwareController.BoundsCheckingEnabled = true;
          }
        }
      }
label_11:
      return commandResult;
    }

    public CommandResult VerifyLock(Guid lockID)
    {
      CommandResult commandResult = CommandResult.Failed_PrinterDoesNotHaveLock;
      lock (lock_sync)
      {
        if (lockStatus == PrinterConnection.LockStatus.Locked)
        {
          if (this.lockID == lockID)
          {
            commandResult = CommandResult.Success;
          }
        }
        else if (lockStatus == PrinterConnection.LockStatus.Pending)
        {
          if (this.lockID == lockID)
          {
            commandResult = CommandResult.Failed_LockNotReady;
          }
        }
        else if (lockID == Guid.Empty)
        {
          commandResult = CommandResult.Success;
        }
      }
      return commandResult;
    }

    private void DoBreakLock()
    {
      BroadcastServer.SendMessageToClient(clientID, new SpoolerMessage(MessageType.LockResult, Info.serial_number, 0.ToString("D8") + CommandResult.LockLost_TimedOut.ToString()).Serialize());
      var num = (int)ReleaseLock(lockID);
    }

    public Guid MyLock
    {
      get
      {
        return lockID;
      }
    }

    public bool ConnectTo()
    {
      try
      {
        return SerialConnect(115200);
      }
      catch (Exception ex)
      {
        lastError = ex.Message + "\n" + ex.StackTrace;
        return false;
      }
    }

    public bool WriteToSerial(byte[] command)
    {
      var flag = true;
      try
      {
        _safeSerialPort.Write(command, 0, command.Length);
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
      if (_safeSerialPort.BytesToRead > 0)
      {
        return _safeSerialPort.ReadExisting();
      }

      return null;
    }

    public bool IsOpen
    {
      get
      {
        return _safeSerialPort.IsOpen;
      }
    }

    private bool SerialConnect(int baudrate)
    {
      try
      {
        _safeSerialPort.Open();
      }
      catch (Exception ex)
      {
        lastError = ex.Message;
        return false;
      }
      try
      {
        _safeSerialPort.WriteTimeout = 5000;
        _safeSerialPort.ReadTimeout = 5000;
      }
      catch (Exception ex)
      {
        _safeSerialPort.Dispose();
        lastError = ex.Message;
        return false;
      }
      Thread.Sleep(250);
      return true;
    }

    public CommandResult RegisterExternalPluginGCodes(string ID, string[] gCodeList)
    {
      FirmwareController firmwareController = GetFirmwareController();
      if (firmwareController != null)
      {
        return firmwareController.RegisterExternalPluginGCodes(ID, gCodeList);
      }

      return CommandResult.Failed_NotInFirmware;
    }

    public void BroadcastPluginMessage(SpoolerMessage message)
    {
      if (message.Type != MessageType.PluginMessage)
      {
        return;
      }

      if (clientID != Guid.Empty)
      {
        BroadcastServer.SendMessageToClient(clientID, message.Serialize());
      }
      else
      {
        BroadcastServer.BroadcastMessage(message.Serialize());
      }
    }

    private void RegisterFirmwarePlugins()
    {
      FirmwareController firmwareController = GetFirmwareController();
      if (firmwareController == null)
      {
        return;
      }

      var sdCardPlugin = new SDCardPlugin(PrinterInfo.accessories.SDCardStatus, new SDCardPlugin.ActiveSDPrintCallback(firmwareController.ConnectToActiveSDPrint));
      var num1 = (int) firmwareController.RegisterPlugin(sdCardPlugin);
      m_oPowerRecoveryPlugin = new PowerRecoveryPlugin(PrinterInfo.powerRecovery, firmwareController, new PowerRecoveryPlugin.RecoverySpoolerPrintCallback(firmwareController.RecoverySpoolerPrintCallback));
      var num2 = (int) firmwareController.RegisterPlugin(m_oPowerRecoveryPlugin);
    }

    protected IBroadcastServer BroadcastServer
    {
      get
      {
        return broadcastserver;
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
