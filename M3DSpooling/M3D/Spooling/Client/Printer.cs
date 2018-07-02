using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;
using M3D.Spooling.Printer_Profiles;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace M3D.Spooling.Client
{
  public class Printer : IPrinter
  {
    private Dictionary<string, IPrinterPlugin> m_odPluginDictionary = new Dictionary<string, IPrinterPlugin>();
    private ThreadSafeVariable<bool> can_check_idle = new ThreadSafeVariable<bool>(false);
    private object timeout_lock_sync = new object();
    private WriteOnce<PrinterProfile> m_printer_profile = new WriteOnce<PrinterProfile>((PrinterProfile) null);
    private bool inRequestedMode = true;
    public const int InactiveLockTimeOutLimitSeconds = 30;
    private bool m_bPluginsRegistered;
    private Guid mylockID;
    private ThreadSafeVariable<PrinterLockStatus> lockstatus;
    private ThreadSafeVariable<bool> lockstepmode;
    private ThreadSafeVariable<int> lockTimeOutSeconds;
    private Stopwatch keeplockalive_clock;
    private Stopwatch keeplockalive_limit_clock;
    private AsyncCallObject waiting_object;
    private object waiting_object_lock;
    private AsyncCallback AllCommandsFinished;
    private object finished_lock;
    private EventLockTimeOutCallBack __LockTimeOutCallBack;
    public PrinterInfo printer_info;
    private CircularArray<string> log;
    public bool log_updated;
    public ThreadSafeVariable<bool> Found;
    public ThreadSafeVariable<bool> _connected;
    public int time_since_found;
    public JobInfo current_job;
    public bool switching_modes;
    public OnUpdateDataDel OnUpdateData;
    public OnProcessSpoolerMessageDel OnProcessSpoolerMessage;
    private FilamentSpool current_spool;
    private bool spool_up_to_date;
    private object spool_lock;
    private byte[] incoming_data;
    private object thread_sync;
    private SpoolerClient client;
    private ConcurrentDictionary<string, string> m_ChangedKeyValuePairs;

    public Printer(string printer_serial_number, PrinterProfile profile, SpoolerClient client)
      : this(profile, client)
    {
      printer_info = new PrinterInfo
      {
        serial_number = new PrinterSerialNumber(printer_serial_number)
      };
    }

    public Printer(PrinterInfo info, PrinterProfile profile, SpoolerClient client)
      : this(profile, client)
    {
      printer_info = new PrinterInfo(info);
    }

    private Printer(PrinterProfile profile, SpoolerClient client)
    {
      this.client = client;
      mylockID = Guid.Empty;
      m_printer_profile.Value = profile;
      thread_sync = new object();
      spool_lock = new object();
      spool_up_to_date = false;
      incoming_data = (byte[]) null;
      Found = new ThreadSafeVariable<bool>
      {
        Value = false
      };
      _connected = new ThreadSafeVariable<bool>
      {
        Value = false
      };
      log = new CircularArray<string>(200);
      LogWaits = true;
      LogFeedback = true;
      waiting_object = (AsyncCallObject) null;
      waiting_object_lock = new object();
      lockstatus = new ThreadSafeVariable<PrinterLockStatus>(PrinterLockStatus.Unlocked);
      lockstepmode = new ThreadSafeVariable<bool>(true);
      lockTimeOutSeconds = new ThreadSafeVariable<int>(0);
      keeplockalive_clock = new Stopwatch();
      keeplockalive_limit_clock = new Stopwatch();
      finished_lock = new object();
      m_ChangedKeyValuePairs = new ConcurrentDictionary<string, string>();
    }

    public SpoolerResult PrintModel(AsyncCallback callback, object state, JobParams jobParams)
    {
      var spooler = (int)SendRPCToSpooler(callback, state, CallBackType.SuccessfullyReceived, false, "AddPrintJob", (object) Environment.UserName, (object) jobParams);
      if (spooler != 0)
      {
        return (SpoolerResult) spooler;
      }

      mylockID = Guid.Empty;
      lockstatus.Value = PrinterLockStatus.Unlocked;
      return (SpoolerResult) spooler;
    }

    public SpoolerResult SendManualGCode(AsyncCallback callback, object state, params string[] gcode)
    {
      return SendRPCToSpooler(callback, state, "WriteManualCommands", (object) "", (object) gcode);
    }

    public SpoolerResult PausePrint(AsyncCallback callback, object state)
    {
      return SendRPCToSpooler(callback, state, CallBackType.SuccessfullyReceived, true, nameof (PausePrint));
    }

    public SpoolerResult ContinuePrint(AsyncCallback callback, object state)
    {
      var spooler = (int)SendRPCToSpooler(callback, state, CallBackType.SuccessfullyReceived, false, nameof (ContinuePrint));
      if (spooler != 0)
      {
        return (SpoolerResult) spooler;
      }

      mylockID = Guid.Empty;
      lockstatus.Value = PrinterLockStatus.Unlocked;
      return (SpoolerResult) spooler;
    }

    public SpoolerResult AbortPrint(AsyncCallback callback, object state)
    {
      return SendRPCToSpooler(callback, state, CallBackType.CallID, true, nameof (AbortPrint));
    }

    public SpoolerResult ClearCurrentWarning(AsyncCallback callback, object state)
    {
      return SendRPCToSpooler(callback, state, "ClearWarning", new object[0]);
    }

    public SpoolerResult PrintBacklashPrint(AsyncCallback callback, object state)
    {
      var spooler = (int)SendRPCToSpooler(callback, state, CallBackType.SuccessfullyReceived, false, nameof (PrintBacklashPrint), (object) Environment.UserName);
      if (spooler != 0)
      {
        return (SpoolerResult) spooler;
      }

      mylockID = Guid.Empty;
      lockstatus.Value = PrinterLockStatus.Unlocked;
      return (SpoolerResult) spooler;
    }

    public SpoolerResult CancelQueuedPrint(AsyncCallback callback, object state)
    {
      return SendRPCToSpooler(callback, state, "KillJobs", new object[0]);
    }

    public SpoolerResult DoFirmwareUpdate(AsyncCallback callback, object state)
    {
      switching_modes = true;
      inRequestedMode = true;
      var spooler = (int)SendRPCToSpooler(callback, state, CallBackType.FirmwareMode, false, "UpdateFirmware");
      if (spooler != 0)
      {
        switching_modes = false;
        return (SpoolerResult) spooler;
      }
      mylockID = Guid.Empty;
      lockstatus.Value = PrinterLockStatus.Unlocked;
      return (SpoolerResult) spooler;
    }

    public SpoolerResult SetCalibrationOffset(AsyncCallback callback, object state, float offset)
    {
      if (!Info.calibration.UsesCalibrationOffset)
      {
        return SpoolerResult.Fail_NotAvailable;
      }

      return SendRPCToSpooler(callback, state, nameof (SetCalibrationOffset), (object) offset);
    }

    public SpoolerResult SetOffsetInfo(AsyncCallback callback, object state, BedOffsets offsets)
    {
      return SendRPCToSpooler(callback, state, "SetOffsetInformation", (object) offsets);
    }

    public SpoolerResult SetBacklash(AsyncCallback callback, object state, BacklashSettings backlash)
    {
      return SendRPCToSpooler(callback, state, "SetBacklashValues", (object) backlash);
    }

    public SpoolerResult SetNozzleWidth(AsyncCallback callback, object state, int iNozzleWidthMicrons)
    {
      return SendRPCToSpooler(callback, state, nameof (SetNozzleWidth), (object) iNozzleWidthMicrons);
    }

    public SpoolerResult AddUpdateKeyValuePair(AsyncCallback callback, object state, string key, string value)
    {
      if (m_ChangedKeyValuePairs.ContainsKey(key))
      {
        m_ChangedKeyValuePairs[key] = value;
      }
      else
      {
        m_ChangedKeyValuePairs.TryAdd(key, value);
      }

      return SendRPCToSpooler(callback, state, CallBackType.SuccessfullyReceived, true, nameof (AddUpdateKeyValuePair), (object) key, (object) value);
    }

    public string GetValidatedValueFromPrinter(string key)
    {
      if (m_ChangedKeyValuePairs.ContainsKey(key))
      {
        return m_ChangedKeyValuePairs[key];
      }

      if (Info.persistantData.SavedData.ContainsKey(key))
      {
        return Info.persistantData.SavedData[key];
      }

      return (string) null;
    }

    public SpoolerResult SendEmergencyStop(AsyncCallback callback, object state)
    {
      return SendRPCToSpooler(callback, state, CallBackType.SuccessfullyReceived, true, nameof (SendEmergencyStop));
    }

    public SpoolerResult SetFilamentInfo(AsyncCallback callback, object state, FilamentSpool info)
    {
      if (info == (FilamentSpool) null)
      {
        return SetFilamentToNone(callback, state);
      }

      current_spool = new FilamentSpool(info);
      spool_up_to_date = true;
      return SendRPCToSpooler(callback, state, "SetFilamentInformation", (object) info);
    }

    public SpoolerResult SetFilamentToNone(AsyncCallback callback, object state)
    {
      current_spool = new FilamentSpool();
      spool_up_to_date = true;
      return SetFilamentInfo(callback, state, current_spool);
    }

    public SpoolerResult SetFilamentUID(AsyncCallback callback, object state, uint filamentUID)
    {
      current_spool.filament_uid = filamentUID;
      spool_up_to_date = true;
      return SendRPCToSpooler(callback, state, nameof (SetFilamentUID), (object) filamentUID);
    }

    public SpoolerResult SetTemperatureWhilePrinting(AsyncCallback callback, object state, int temperature)
    {
      return SendRPCToSpooler(callback, state, CallBackType.SuccessfullyReceived, true, nameof (SetTemperatureWhilePrinting), (object) temperature);
    }

    public SpoolerResult SendSerialData(AsyncCallback callback, object state, byte[] data)
    {
      return SendRPCToSpooler(callback, state, CallBackType.Special, false, "WriteSerialdata", (object) Convert.ToBase64String(data));
    }

    public byte[] SendSerialDataWaitForResponse(byte[] data, int bytesToReceive)
    {
      incoming_data = (byte[]) null;
      if (SendRPCToSpooler((AsyncCallback) null, (object) null, "WriteSerialdata", (object) Convert.ToBase64String(data), (object) bytesToReceive) != SpoolerResult.OK)
      {
        return (byte[]) null;
      }

      var numArray = (byte[]) null;
      while (numArray == null)
      {
        lock (thread_sync)
        {
          numArray = incoming_data;
        }

        if (numArray == null)
        {
          Thread.Sleep(0);
        }
      }
      return numArray;
    }

    public SpoolerResult ClearPowerRecoveryFault(AsyncCallback callback, object state)
    {
      return SendRPCToSpooler(callback, state, CallBackType.SuccessfullyReceived, false, nameof (ClearPowerRecoveryFault));
    }

    public SpoolerResult RecoveryPrintFromPowerFailure(AsyncCallback callback, object state, bool bHomingRequired)
    {
      return SendRPCToSpooler(callback, state, CallBackType.SuccessfullyReceived, false, nameof (RecoveryPrintFromPowerFailure), (object) bHomingRequired);
    }

    private SpoolerResult SendRPCToSpooler(AsyncCallback callback, object state, string function_name, params object[] options)
    {
      return SendRPCToSpooler(callback, state, CallBackType.CallID, false, function_name, options);
    }

    private SpoolerResult SendRPCToSpooler(AsyncCallback callback, object state, CallBackType callbacktype, bool always_send, string function_name, params object[] options)
    {
      if (!HasLock && !always_send)
      {
        if (callback != null)
        {
          var ar = (IAsyncCallResult) new SimpleAsyncCallResult(state, CommandResult.Failed_PrinterDoesNotHaveLock);
          callback(ar);
        }
        return SpoolerResult.Fail_DoesNotHaveLock;
      }
      AsyncCallObject newWaitingObject = CreateNewWaitingObject(callback, state, LockStepMode && !always_send);
      if (newWaitingObject == null)
      {
        if (callback != null)
        {
          callback((IAsyncCallResult) new AsyncCallObject(callback, state, (IPrinter) this)
          {
            callresult = CommandResult.Failed_PreviousCommandNotCompleted
          });
        }

        return SpoolerResult.Fail_PreviousCommandNotComplete;
      }
      newWaitingObject.callbackType = callbacktype;
      return SendRPCToSpooler(function_name, newWaitingObject.callID, options);
    }

    private SpoolerResult SendRPCToSpooler(string function_name, uint callID, params object[] options)
    {
      return client.SendSpoolerMessageRPCAsync(new RPCInvoker.RPC(printer_info.serial_number, mylockID, callID, function_name, options));
    }

    public bool RegisterPlugin(string ID, IPrinterPlugin plugin)
    {
      if (!m_odPluginDictionary.ContainsKey(ID))
      {
        m_odPluginDictionary.Add(ID, plugin);
        if (Info.InFirmwareMode)
        {
          var num = (int)client.SendSpoolerMessageRPC(new RPCInvoker.RPC(Info.serial_number, Guid.Empty, 0U, "RegisterExternalPluginGCodes", new object[2]{ (object) ID, (object) plugin.GetGCodes() }));
          m_bPluginsRegistered = true;
          return true;
        }
      }
      return false;
    }

    public IPrinterPlugin GetPrinterPlugin(string ID)
    {
      if (m_odPluginDictionary.ContainsKey(ID))
      {
        return m_odPluginDictionary[ID];
      }

      return (IPrinterPlugin) null;
    }

    private void DoPluginRegistration()
    {
      foreach (KeyValuePair<string, IPrinterPlugin> odPlugin in m_odPluginDictionary)
      {
        var key = odPlugin.Key;
        IPrinterPlugin printerPlugin = odPlugin.Value;
        var num = (int)client.SendSpoolerMessageRPC(new RPCInvoker.RPC(Info.serial_number, Guid.Empty, 0U, "RegisterExternalPluginGCodes", new object[2]{ (object) key, (object) printerPlugin.GetGCodes() }));
      }
      m_bPluginsRegistered = true;
    }

    private void ProcessPluginMessageFromSpooler(SpoolerMessage message)
    {
      if (!m_odPluginDictionary.ContainsKey(message.PlugInID))
      {
        return;
      }

      m_odPluginDictionary[message.PlugInID].OnReceivedPluginMessage(message.State, message.Message);
    }

    public bool LockStepMode
    {
      get
      {
        return lockstepmode.Value;
      }
      set
      {
        lockstepmode.Value = value;
      }
    }

    private AsyncCallObject CreateNewWaitingObject(AsyncCallback callback, object state, bool lockstepmode)
    {
      var asyncCallObject = (AsyncCallObject) null;
      lock (waiting_object_lock)
      {
        if (!lockstepmode && waiting_object != null)
        {
          AsyncCallObject waitingObject = waiting_object;
          waitingObject.callresult = CommandResult.OverridedByNonLockStepCall;
          ThreadPool.QueueUserWorkItem(new WaitCallback(DoAsyncCallBack), (object) waitingObject);
          waiting_object = (AsyncCallObject) null;
        }
        if (waiting_object == null)
        {
          waiting_object = new AsyncCallObject(callback, state, (IPrinter) this);
          can_check_idle.Value = false;
          asyncCallObject = waiting_object;
        }
      }
      return asyncCallObject;
    }

    public SpoolerResult BreakLock(AsyncCallback callback, object state)
    {
      return SendRPCToSpooler(callback, state, CallBackType.Special, true, nameof (BreakLock));
    }

    public SpoolerResult AcquireLock(AsyncCallback callback, object state)
    {
      return AcquireLock(callback, state, (EventLockTimeOutCallBack) null, 0);
    }

    public SpoolerResult AcquireLock(AsyncCallback callback, object state, EventLockTimeOutCallBack LockTimeOutCallBack, int locktimeoutseconds)
    {
      if (HasLock && callback != null)
      {
        callback((IAsyncCallResult) new AsyncCallObject(callback, state, (IPrinter) this)
        {
          callresult = CommandResult.Success_LockAcquired
        });
        return SpoolerResult.OK;
      }
      AsyncCallObject newWaitingObject = CreateNewWaitingObject(callback, state, false);
      if (newWaitingObject == null)
      {
        return SpoolerResult.Fail_PreviousCommandNotComplete;
      }

      newWaitingObject.callbackType = CallBackType.Special;
      lockstatus.Value = PrinterLockStatus.OurLockPending;
      lockTimeOutSeconds.Value = locktimeoutseconds;
      lock (timeout_lock_sync)
      {
        __LockTimeOutCallBack = LockTimeOutCallBack;
      }

      return SendRPCToSpooler(nameof (AcquireLock), newWaitingObject.callID);
    }

    public SpoolerResult ReleaseLock(AsyncCallback callback, object state)
    {
      AsyncCallObject newWaitingObject = CreateNewWaitingObject(callback, state, false);
      if (newWaitingObject == null)
      {
        return SpoolerResult.Fail_PreviousCommandNotComplete;
      }

      newWaitingObject.callbackType = CallBackType.Special;
      lockstatus.Value = PrinterLockStatus.OurReleasePending;
      return SendRPCToSpooler(nameof (ReleaseLock), newWaitingObject.callID);
    }

    public void AddCommandToRunOnFinish(AsyncCallback callback)
    {
      lock (finished_lock)
      {
        AllCommandsFinished += callback;
      }
    }

    public void ClearAsyncCallbacks()
    {
      lock (waiting_object_lock)
      {
        waiting_object = (AsyncCallObject) null;
      }
    }

    private void DoAsyncCallBack(object o)
    {
      var asyncCallObject = o as AsyncCallObject;
      if (asyncCallObject.callback == null)
      {
        return;
      }

      asyncCallObject.callback((IAsyncCallResult) asyncCallObject);
    }

    private void DoTimeoutCallBack(object o)
    {
      var timeoutProperties = o as Printer.TimeoutProperties;
      if (timeoutProperties.callback == null)
      {
        return;
      }

      timeoutProperties.callback(timeoutProperties.printer);
    }

    public bool PrinterIsLocked
    {
      get
      {
        if (!Info.synchronization.Locked)
        {
          return HasLock;
        }

        return true;
      }
    }

    public bool WaitingForCommandToComplete
    {
      get
      {
        lock (waiting_object_lock)
        {
          return waiting_object != null;
        }
      }
    }

    public bool HasLock
    {
      get
      {
        return mylockID != Guid.Empty;
      }
    }

    public PrinterLockStatus LockStatus
    {
      get
      {
        if (HasLock)
        {
          return PrinterLockStatus.WeOwnLocked;
        }

        if (Info.synchronization.Locked)
        {
          return PrinterLockStatus.LockedByOther;
        }

        return lockstatus.Value;
      }
    }

    public FilamentSpool GetCurrentFilament()
    {
      var filamentSpool = (FilamentSpool) null;
      lock (spool_lock)
      {
        if (current_spool != (FilamentSpool) null)
        {
          filamentSpool = new FilamentSpool(current_spool);
        }
      }
      if (filamentSpool != (FilamentSpool) null && filamentSpool.filament_type == FilamentSpool.TypeEnum.NoFilament)
      {
        return (FilamentSpool) null;
      }

      return filamentSpool;
    }

    public bool isConnected()
    {
      return Connected;
    }

    public bool Connected
    {
      get
      {
        return _connected.Value;
      }
    }

    public bool InBootloaderMode
    {
      get
      {
        return printer_info.InBootloaderMode;
      }
    }

    public bool Switching
    {
      get
      {
        return switching_modes;
      }
    }

    public void UpdateData(PrinterInfo info)
    {
      printer_info.CopyFrom(info);
      current_job = printer_info.current_job == null ? (JobInfo) null : new JobInfo(printer_info.current_job);
      var other = new FilamentSpool(printer_info.filament_info);
      if (spool_up_to_date)
      {
        if (other == current_spool)
        {
          spool_up_to_date = false;
        }
      }
      else
      {
        current_spool = new FilamentSpool(other);
      }

      CheckForUpdatedValues();
      var asyncCallObject = (AsyncCallObject) null;
      var flag1 = false;
      var flag2 = false;
      lock (waiting_object_lock)
      {
        if (waiting_object != null)
        {
          var lastCompletedRpcid = Info.synchronization.LastCompletedRPCID;
          if (Info.Status == PrinterStatus.Firmware_Idle && can_check_idle.Value)
          {
            flag1 = true;
          }

          if (waiting_object.callbackType == CallBackType.FirmwareMode && !Info.InFirmwareMode || waiting_object.callbackType == CallBackType.BootloaderMode && !Info.InBootloaderMode)
          {
            inRequestedMode = false;
          }

          if (waiting_object.callbackType == CallBackType.CallID && (int)waiting_object.callID == (int) lastCompletedRpcid || !inRequestedMode && (waiting_object.callbackType == CallBackType.FirmwareMode && Info.InFirmwareMode || waiting_object.callbackType == CallBackType.BootloaderMode && Info.InBootloaderMode))
          {
            flag2 = true;
            flag1 = false;
          }
          if (flag1 | flag2)
          {
            asyncCallObject = waiting_object;
            waiting_object = (AsyncCallObject) null;
          }
        }
      }
      if (asyncCallObject != null)
      {
        asyncCallObject.callresult = CommandResult.Success;
        asyncCallObject.idle_callback = flag1;
        ThreadPool.QueueUserWorkItem(new WaitCallback(DoAsyncCallBack), (object) asyncCallObject);
        lock (finished_lock)
        {
          if (AllCommandsFinished != null)
          {
            AllCommandsFinished((IAsyncCallResult) new SimpleAsyncCallResult((object) this, CommandResult.Success));
          }
        }
      }
      if (HasLock && Info.IsIdle)
      {
        var num = lockTimeOutSeconds.Value;
        if (!keeplockalive_clock.IsRunning)
        {
          keeplockalive_clock.Restart();
        }

        if (!keeplockalive_limit_clock.IsRunning && num > 0)
        {
          keeplockalive_limit_clock.Restart();
        }

        if (keeplockalive_clock.Elapsed.TotalSeconds > 15.0 && (num <= 0 || keeplockalive_limit_clock.Elapsed.TotalSeconds < (double) num))
        {
          var spooler = (int)SendRPCToSpooler("KeepLockAlive", 0U);
          keeplockalive_clock.Restart();
        }
      }
      else if (keeplockalive_clock.IsRunning)
      {
        keeplockalive_clock.Stop();
        keeplockalive_limit_clock.Stop();
      }
      if (Info.InBootloaderMode)
      {
        m_bPluginsRegistered = false;
      }
      else if (!m_bPluginsRegistered)
      {
        DoPluginRegistration();
      }

      if (OnUpdateData == null)
      {
        return;
      }

      OnUpdateData(info);
    }

    private void CheckForUpdatedValues()
    {
      foreach (KeyValuePair<string, string> keyValuePair in (Dictionary<string, string>)Info.persistantData.SavedData)
      {
        if (m_ChangedKeyValuePairs.ContainsKey(keyValuePair.Key) && keyValuePair.Value == m_ChangedKeyValuePairs[keyValuePair.Key])
        {
          m_ChangedKeyValuePairs.TryRemove(keyValuePair.Key, out var str);
        }
      }
    }

    public void ProcessSpoolerMessage(SpoolerMessage message)
    {
      var asyncCallObject = (AsyncCallObject) null;
      var flag = false;
      if (message.Type == MessageType.RawData)
      {
        lock (thread_sync)
        {
          incoming_data = message.GetRawData();
        }
      }
      else if (message.Type == MessageType.PluginMessage)
      {
        ProcessPluginMessageFromSpooler(message);
      }
      else if (message.Type == MessageType.LoggingMessage)
      {
        AddMessageToLog(Base64Convert.Base64Decode(message.Message));
        lock (log)
        {
          log_updated = true;
        }
      }
      else if (message.Type == MessageType.FullLoggingData)
      {
        string[] strArray = Base64Convert.Base64Decode(message.Message).Split('\n');
        log.Clear();
        foreach (var message1 in strArray)
        {
          AddMessageToLog(message1);
        }

        lock (log)
        {
          log_updated = true;
        }
      }
      else if (message.Type == MessageType.BedLocationMustBeCalibrated || message.Type == MessageType.BedOrientationMustBeCalibrated || message.Type == MessageType.CheckGantryClips)
      {
        mylockID = Guid.Empty;
        lockstatus.Value = PrinterLockStatus.Unlocked;
        lock (waiting_object_lock)
        {
          if (waiting_object != null)
          {
            asyncCallObject = waiting_object;
            waiting_object = (AsyncCallObject) null;
            asyncCallObject.callresult = CommandResult.Failed_GantryClipsOrInvalidZ;
          }
        }
      }
      else if ((message.Type == MessageType.LockConfirmed || message.Type == MessageType.LockResult) && message.SerialNumber == Info.serial_number)
      {
        uint num = 0;
        EventLockTimeOutCallBack callback = (EventLockTimeOutCallBack) null;
        CommandResult commandResult;
        if (message.Type == MessageType.LockResult)
        {
          var s = message.Message.Substring(0, 8);
          var str = message.Message.Substring(8);
          try
          {
            num = uint.Parse(s);
            commandResult = (CommandResult) Enum.Parse(typeof (CommandResult), str);
          }
          catch (ArgumentException ex)
          {
            commandResult = CommandResult.Failed_Exception;
          }
          if (commandResult == CommandResult.Success)
          {
            can_check_idle.Value = true;
            return;
          }
          if (commandResult == CommandResult.Pending)
          {
            lockstatus.Value = PrinterLockStatus.OurLockPending;
            return;
          }
          if (commandResult == CommandResult.LockForcedOpen)
          {
            lockstatus.Value = PrinterLockStatus.Unlocked;
          }
          else if (commandResult == CommandResult.LockLost_TimedOut)
          {
            lock (timeout_lock_sync)
            {
              callback = __LockTimeOutCallBack;
              __LockTimeOutCallBack = (EventLockTimeOutCallBack) null;
            }
          }
          if (commandResult != CommandResult.CommandInterruptedByM0)
          {
            flag = true;
          }
        }
        else
        {
          mylockID = Guid.Parse(message.Message);
          commandResult = CommandResult.Success_LockAcquired;
          lockstatus.Value = PrinterLockStatus.WeOwnLocked;
        }
        lock (waiting_object_lock)
        {
          if (waiting_object != null)
          {
            if (commandResult == CommandResult.SuccessfullyReceived)
            {
              if (waiting_object.callbackType != CallBackType.SuccessfullyReceived)
              {
                goto label_59;
              }
            }
            asyncCallObject = waiting_object;
            waiting_object = (AsyncCallObject) null;
          }
        }
label_59:
        if (flag)
        {
          mylockID = Guid.Empty;
          lockstatus.Value = PrinterLockStatus.Unlocked;
        }
        if (asyncCallObject != null)
        {
          if (num != 0U && (int) num != (int) asyncCallObject.callID)
          {
            commandResult = CommandResult.Failed_AsyncCallbackError;
          }

          asyncCallObject.callresult = commandResult;
        }
        if (callback != null)
        {
          ThreadPool.QueueUserWorkItem(new WaitCallback(DoTimeoutCallBack), (object) new Printer.TimeoutProperties(callback, (IPrinter) this));
        }
      }
      if (asyncCallObject != null)
      {
        ThreadPool.QueueUserWorkItem(new WaitCallback(DoAsyncCallBack), (object) asyncCallObject);
      }

      if (OnProcessSpoolerMessage == null)
      {
        return;
      }

      OnProcessSpoolerMessage(message);
    }

    public bool LogUpdated
    {
      get
      {
        lock (log)
        {
          return log_updated;
        }
      }
    }

    public List<string> GetLog()
    {
      var stringList = (List<string>) null;
      lock (log)
      {
        stringList = new List<string>((IEnumerable<string>)log);
        log_updated = false;
      }
      return stringList;
    }

    public List<string> GetNextLogItems()
    {
      var stringList = (List<string>) null;
      lock (log)
      {
        stringList = new List<string>((IEnumerable<string>)log);
        log.Clear();
        log_updated = false;
      }
      return stringList;
    }

    public void ClearLog()
    {
      lock (log)
      {
        log.Clear();
        log_updated = true;
      }
    }

    private void AddMessageToLog(string message)
    {
      if (!LogWaits && message == ">> wait" || message.StartsWith(">> T:") && !LogFeedback)
      {
        return;
      }

      lock (log)
      {
        log.Enqueue(message);
      }
    }

    public PrinterProfile MyPrinterProfile
    {
      get
      {
        return m_printer_profile.Value;
      }
    }

    public PrinterInfo Info
    {
      get
      {
        return printer_info;
      }
    }

    public bool LogWaits { get; set; }

    public bool LogFeedback { get; set; }

    private class TimeoutProperties
    {
      public EventLockTimeOutCallBack callback;
      public IPrinter printer;

      public TimeoutProperties(EventLockTimeOutCallBack callback, IPrinter printer)
      {
        this.callback = callback;
        this.printer = printer;
      }
    }
  }
}
