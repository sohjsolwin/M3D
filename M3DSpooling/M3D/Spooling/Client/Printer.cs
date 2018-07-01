// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Client.Printer
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
      this.printer_info = new PrinterInfo();
      this.printer_info.serial_number = new PrinterSerialNumber(printer_serial_number);
    }

    public Printer(PrinterInfo info, PrinterProfile profile, SpoolerClient client)
      : this(profile, client)
    {
      this.printer_info = new PrinterInfo(info);
    }

    private Printer(PrinterProfile profile, SpoolerClient client)
    {
      this.client = client;
      this.mylockID = Guid.Empty;
      this.m_printer_profile.Value = profile;
      this.thread_sync = new object();
      this.spool_lock = new object();
      this.spool_up_to_date = false;
      this.incoming_data = (byte[]) null;
      this.Found = new ThreadSafeVariable<bool>();
      this.Found.Value = false;
      this._connected = new ThreadSafeVariable<bool>();
      this._connected.Value = false;
      this.log = new CircularArray<string>(200);
      this.LogWaits = true;
      this.LogFeedback = true;
      this.waiting_object = (AsyncCallObject) null;
      this.waiting_object_lock = new object();
      this.lockstatus = new ThreadSafeVariable<PrinterLockStatus>(PrinterLockStatus.Unlocked);
      this.lockstepmode = new ThreadSafeVariable<bool>(true);
      this.lockTimeOutSeconds = new ThreadSafeVariable<int>(0);
      this.keeplockalive_clock = new Stopwatch();
      this.keeplockalive_limit_clock = new Stopwatch();
      this.finished_lock = new object();
      this.m_ChangedKeyValuePairs = new ConcurrentDictionary<string, string>();
    }

    public SpoolerResult PrintModel(AsyncCallback callback, object state, JobParams jobParams)
    {
      int spooler = (int) this.SendRPCToSpooler(callback, state, CallBackType.SuccessfullyReceived, false, "AddPrintJob", (object) Environment.UserName, (object) jobParams);
      if (spooler != 0)
        return (SpoolerResult) spooler;
      this.mylockID = Guid.Empty;
      this.lockstatus.Value = PrinterLockStatus.Unlocked;
      return (SpoolerResult) spooler;
    }

    public SpoolerResult SendManualGCode(AsyncCallback callback, object state, params string[] gcode)
    {
      return this.SendRPCToSpooler(callback, state, "WriteManualCommands", (object) "", (object) gcode);
    }

    public SpoolerResult PausePrint(AsyncCallback callback, object state)
    {
      return this.SendRPCToSpooler(callback, state, CallBackType.SuccessfullyReceived, true, nameof (PausePrint));
    }

    public SpoolerResult ContinuePrint(AsyncCallback callback, object state)
    {
      int spooler = (int) this.SendRPCToSpooler(callback, state, CallBackType.SuccessfullyReceived, false, nameof (ContinuePrint));
      if (spooler != 0)
        return (SpoolerResult) spooler;
      this.mylockID = Guid.Empty;
      this.lockstatus.Value = PrinterLockStatus.Unlocked;
      return (SpoolerResult) spooler;
    }

    public SpoolerResult AbortPrint(AsyncCallback callback, object state)
    {
      return this.SendRPCToSpooler(callback, state, CallBackType.CallID, true, nameof (AbortPrint));
    }

    public SpoolerResult ClearCurrentWarning(AsyncCallback callback, object state)
    {
      return this.SendRPCToSpooler(callback, state, "ClearWarning", new object[0]);
    }

    public SpoolerResult PrintBacklashPrint(AsyncCallback callback, object state)
    {
      int spooler = (int) this.SendRPCToSpooler(callback, state, CallBackType.SuccessfullyReceived, false, nameof (PrintBacklashPrint), (object) Environment.UserName);
      if (spooler != 0)
        return (SpoolerResult) spooler;
      this.mylockID = Guid.Empty;
      this.lockstatus.Value = PrinterLockStatus.Unlocked;
      return (SpoolerResult) spooler;
    }

    public SpoolerResult CancelQueuedPrint(AsyncCallback callback, object state)
    {
      return this.SendRPCToSpooler(callback, state, "KillJobs", new object[0]);
    }

    public SpoolerResult DoFirmwareUpdate(AsyncCallback callback, object state)
    {
      this.switching_modes = true;
      this.inRequestedMode = true;
      int spooler = (int) this.SendRPCToSpooler(callback, state, CallBackType.FirmwareMode, false, "UpdateFirmware");
      if (spooler != 0)
      {
        this.switching_modes = false;
        return (SpoolerResult) spooler;
      }
      this.mylockID = Guid.Empty;
      this.lockstatus.Value = PrinterLockStatus.Unlocked;
      return (SpoolerResult) spooler;
    }

    public SpoolerResult SetCalibrationOffset(AsyncCallback callback, object state, float offset)
    {
      if (!this.Info.calibration.UsesCalibrationOffset)
        return SpoolerResult.Fail_NotAvailable;
      return this.SendRPCToSpooler(callback, state, nameof (SetCalibrationOffset), (object) offset);
    }

    public SpoolerResult SetOffsetInfo(AsyncCallback callback, object state, BedOffsets offsets)
    {
      return this.SendRPCToSpooler(callback, state, "SetOffsetInformation", (object) offsets);
    }

    public SpoolerResult SetBacklash(AsyncCallback callback, object state, BacklashSettings backlash)
    {
      return this.SendRPCToSpooler(callback, state, "SetBacklashValues", (object) backlash);
    }

    public SpoolerResult SetNozzleWidth(AsyncCallback callback, object state, int iNozzleWidthMicrons)
    {
      return this.SendRPCToSpooler(callback, state, nameof (SetNozzleWidth), (object) iNozzleWidthMicrons);
    }

    public SpoolerResult AddUpdateKeyValuePair(AsyncCallback callback, object state, string key, string value)
    {
      if (this.m_ChangedKeyValuePairs.ContainsKey(key))
        this.m_ChangedKeyValuePairs[key] = value;
      else
        this.m_ChangedKeyValuePairs.TryAdd(key, value);
      return this.SendRPCToSpooler(callback, state, CallBackType.SuccessfullyReceived, true, nameof (AddUpdateKeyValuePair), (object) key, (object) value);
    }

    public string GetValidatedValueFromPrinter(string key)
    {
      if (this.m_ChangedKeyValuePairs.ContainsKey(key))
        return this.m_ChangedKeyValuePairs[key];
      if (this.Info.persistantData.SavedData.ContainsKey(key))
        return this.Info.persistantData.SavedData[key];
      return (string) null;
    }

    public SpoolerResult SendEmergencyStop(AsyncCallback callback, object state)
    {
      return this.SendRPCToSpooler(callback, state, CallBackType.SuccessfullyReceived, true, nameof (SendEmergencyStop));
    }

    public SpoolerResult SetFilamentInfo(AsyncCallback callback, object state, FilamentSpool info)
    {
      if (info == (FilamentSpool) null)
        return this.SetFilamentToNone(callback, state);
      this.current_spool = new FilamentSpool(info);
      this.spool_up_to_date = true;
      return this.SendRPCToSpooler(callback, state, "SetFilamentInformation", (object) info);
    }

    public SpoolerResult SetFilamentToNone(AsyncCallback callback, object state)
    {
      this.current_spool = new FilamentSpool();
      this.spool_up_to_date = true;
      return this.SetFilamentInfo(callback, state, this.current_spool);
    }

    public SpoolerResult SetFilamentUID(AsyncCallback callback, object state, uint filamentUID)
    {
      this.current_spool.filament_uid = filamentUID;
      this.spool_up_to_date = true;
      return this.SendRPCToSpooler(callback, state, nameof (SetFilamentUID), (object) filamentUID);
    }

    public SpoolerResult SetTemperatureWhilePrinting(AsyncCallback callback, object state, int temperature)
    {
      return this.SendRPCToSpooler(callback, state, CallBackType.SuccessfullyReceived, true, nameof (SetTemperatureWhilePrinting), (object) temperature);
    }

    public SpoolerResult SendSerialData(AsyncCallback callback, object state, byte[] data)
    {
      return this.SendRPCToSpooler(callback, state, CallBackType.Special, false, "WriteSerialdata", (object) Convert.ToBase64String(data));
    }

    public byte[] SendSerialDataWaitForResponse(byte[] data, int bytesToReceive)
    {
      this.incoming_data = (byte[]) null;
      if (this.SendRPCToSpooler((AsyncCallback) null, (object) null, "WriteSerialdata", (object) Convert.ToBase64String(data), (object) bytesToReceive) != SpoolerResult.OK)
        return (byte[]) null;
      byte[] numArray = (byte[]) null;
      while (numArray == null)
      {
        lock (this.thread_sync)
          numArray = this.incoming_data;
        if (numArray == null)
          Thread.Sleep(0);
      }
      return numArray;
    }

    public SpoolerResult ClearPowerRecoveryFault(AsyncCallback callback, object state)
    {
      return this.SendRPCToSpooler(callback, state, CallBackType.SuccessfullyReceived, false, nameof (ClearPowerRecoveryFault));
    }

    public SpoolerResult RecoveryPrintFromPowerFailure(AsyncCallback callback, object state, bool bHomingRequired)
    {
      return this.SendRPCToSpooler(callback, state, CallBackType.SuccessfullyReceived, false, nameof (RecoveryPrintFromPowerFailure), (object) bHomingRequired);
    }

    private SpoolerResult SendRPCToSpooler(AsyncCallback callback, object state, string function_name, params object[] options)
    {
      return this.SendRPCToSpooler(callback, state, CallBackType.CallID, false, function_name, options);
    }

    private SpoolerResult SendRPCToSpooler(AsyncCallback callback, object state, CallBackType callbacktype, bool always_send, string function_name, params object[] options)
    {
      if (!this.HasLock && !always_send)
      {
        if (callback != null)
        {
          IAsyncCallResult ar = (IAsyncCallResult) new SimpleAsyncCallResult(state, CommandResult.Failed_PrinterDoesNotHaveLock);
          callback(ar);
        }
        return SpoolerResult.Fail_DoesNotHaveLock;
      }
      AsyncCallObject newWaitingObject = this.CreateNewWaitingObject(callback, state, this.LockStepMode && !always_send);
      if (newWaitingObject == null)
      {
        if (callback != null)
          callback((IAsyncCallResult) new AsyncCallObject(callback, state, (IPrinter) this)
          {
            callresult = CommandResult.Failed_PreviousCommandNotCompleted
          });
        return SpoolerResult.Fail_PreviousCommandNotComplete;
      }
      newWaitingObject.callbackType = callbacktype;
      return this.SendRPCToSpooler(function_name, newWaitingObject.callID, options);
    }

    private SpoolerResult SendRPCToSpooler(string function_name, uint callID, params object[] options)
    {
      return this.client.SendSpoolerMessageRPCAsync(new RPCInvoker.RPC(this.printer_info.serial_number, this.mylockID, callID, function_name, options));
    }

    public bool RegisterPlugin(string ID, IPrinterPlugin plugin)
    {
      if (!this.m_odPluginDictionary.ContainsKey(ID))
      {
        this.m_odPluginDictionary.Add(ID, plugin);
        if (this.Info.InFirmwareMode)
        {
          int num = (int) this.client.SendSpoolerMessageRPC(new RPCInvoker.RPC(this.Info.serial_number, Guid.Empty, 0U, "RegisterExternalPluginGCodes", new object[2]{ (object) ID, (object) plugin.GetGCodes() }));
          this.m_bPluginsRegistered = true;
          return true;
        }
      }
      return false;
    }

    public IPrinterPlugin GetPrinterPlugin(string ID)
    {
      if (this.m_odPluginDictionary.ContainsKey(ID))
        return this.m_odPluginDictionary[ID];
      return (IPrinterPlugin) null;
    }

    private void DoPluginRegistration()
    {
      foreach (KeyValuePair<string, IPrinterPlugin> odPlugin in this.m_odPluginDictionary)
      {
        string key = odPlugin.Key;
        IPrinterPlugin printerPlugin = odPlugin.Value;
        int num = (int) this.client.SendSpoolerMessageRPC(new RPCInvoker.RPC(this.Info.serial_number, Guid.Empty, 0U, "RegisterExternalPluginGCodes", new object[2]{ (object) key, (object) printerPlugin.GetGCodes() }));
      }
      this.m_bPluginsRegistered = true;
    }

    private void ProcessPluginMessageFromSpooler(SpoolerMessage message)
    {
      if (!this.m_odPluginDictionary.ContainsKey(message.PlugInID))
        return;
      this.m_odPluginDictionary[message.PlugInID].OnReceivedPluginMessage(message.State, message.Message);
    }

    public bool LockStepMode
    {
      get
      {
        return this.lockstepmode.Value;
      }
      set
      {
        this.lockstepmode.Value = value;
      }
    }

    private AsyncCallObject CreateNewWaitingObject(AsyncCallback callback, object state, bool lockstepmode)
    {
      AsyncCallObject asyncCallObject = (AsyncCallObject) null;
      lock (this.waiting_object_lock)
      {
        if (!lockstepmode && this.waiting_object != null)
        {
          AsyncCallObject waitingObject = this.waiting_object;
          waitingObject.callresult = CommandResult.OverridedByNonLockStepCall;
          ThreadPool.QueueUserWorkItem(new WaitCallback(this.DoAsyncCallBack), (object) waitingObject);
          this.waiting_object = (AsyncCallObject) null;
        }
        if (this.waiting_object == null)
        {
          this.waiting_object = new AsyncCallObject(callback, state, (IPrinter) this);
          this.can_check_idle.Value = false;
          asyncCallObject = this.waiting_object;
        }
      }
      return asyncCallObject;
    }

    public SpoolerResult BreakLock(AsyncCallback callback, object state)
    {
      return this.SendRPCToSpooler(callback, state, CallBackType.Special, true, nameof (BreakLock));
    }

    public SpoolerResult AcquireLock(AsyncCallback callback, object state)
    {
      return this.AcquireLock(callback, state, (EventLockTimeOutCallBack) null, 0);
    }

    public SpoolerResult AcquireLock(AsyncCallback callback, object state, EventLockTimeOutCallBack LockTimeOutCallBack, int locktimeoutseconds)
    {
      if (this.HasLock && callback != null)
      {
        callback((IAsyncCallResult) new AsyncCallObject(callback, state, (IPrinter) this)
        {
          callresult = CommandResult.Success_LockAcquired
        });
        return SpoolerResult.OK;
      }
      AsyncCallObject newWaitingObject = this.CreateNewWaitingObject(callback, state, false);
      if (newWaitingObject == null)
        return SpoolerResult.Fail_PreviousCommandNotComplete;
      newWaitingObject.callbackType = CallBackType.Special;
      this.lockstatus.Value = PrinterLockStatus.OurLockPending;
      this.lockTimeOutSeconds.Value = locktimeoutseconds;
      lock (this.timeout_lock_sync)
        this.__LockTimeOutCallBack = LockTimeOutCallBack;
      return this.SendRPCToSpooler(nameof (AcquireLock), newWaitingObject.callID);
    }

    public SpoolerResult ReleaseLock(AsyncCallback callback, object state)
    {
      AsyncCallObject newWaitingObject = this.CreateNewWaitingObject(callback, state, false);
      if (newWaitingObject == null)
        return SpoolerResult.Fail_PreviousCommandNotComplete;
      newWaitingObject.callbackType = CallBackType.Special;
      this.lockstatus.Value = PrinterLockStatus.OurReleasePending;
      return this.SendRPCToSpooler(nameof (ReleaseLock), newWaitingObject.callID);
    }

    public void AddCommandToRunOnFinish(AsyncCallback callback)
    {
      lock (this.finished_lock)
        this.AllCommandsFinished += callback;
    }

    public void ClearAsyncCallbacks()
    {
      lock (this.waiting_object_lock)
        this.waiting_object = (AsyncCallObject) null;
    }

    private void DoAsyncCallBack(object o)
    {
      AsyncCallObject asyncCallObject = o as AsyncCallObject;
      if (asyncCallObject.callback == null)
        return;
      asyncCallObject.callback((IAsyncCallResult) asyncCallObject);
    }

    private void DoTimeoutCallBack(object o)
    {
      Printer.TimeoutProperties timeoutProperties = o as Printer.TimeoutProperties;
      if (timeoutProperties.callback == null)
        return;
      timeoutProperties.callback(timeoutProperties.printer);
    }

    public bool PrinterIsLocked
    {
      get
      {
        if (!this.Info.synchronization.Locked)
          return this.HasLock;
        return true;
      }
    }

    public bool WaitingForCommandToComplete
    {
      get
      {
        lock (this.waiting_object_lock)
          return this.waiting_object != null;
      }
    }

    public bool HasLock
    {
      get
      {
        return this.mylockID != Guid.Empty;
      }
    }

    public PrinterLockStatus LockStatus
    {
      get
      {
        if (this.HasLock)
          return PrinterLockStatus.WeOwnLocked;
        if (this.Info.synchronization.Locked)
          return PrinterLockStatus.LockedByOther;
        return this.lockstatus.Value;
      }
    }

    public FilamentSpool GetCurrentFilament()
    {
      FilamentSpool filamentSpool = (FilamentSpool) null;
      lock (this.spool_lock)
      {
        if (this.current_spool != (FilamentSpool) null)
          filamentSpool = new FilamentSpool(this.current_spool);
      }
      if (filamentSpool != (FilamentSpool) null && filamentSpool.filament_type == FilamentSpool.TypeEnum.NoFilament)
        return (FilamentSpool) null;
      return filamentSpool;
    }

    public bool isConnected()
    {
      return this.Connected;
    }

    public bool Connected
    {
      get
      {
        return this._connected.Value;
      }
    }

    public bool InBootloaderMode
    {
      get
      {
        return this.printer_info.InBootloaderMode;
      }
    }

    public bool Switching
    {
      get
      {
        return this.switching_modes;
      }
    }

    public void UpdateData(PrinterInfo info)
    {
      this.printer_info.CopyFrom(info);
      this.current_job = this.printer_info.current_job == null ? (JobInfo) null : new JobInfo(this.printer_info.current_job);
      FilamentSpool other = new FilamentSpool(this.printer_info.filament_info);
      if (this.spool_up_to_date)
      {
        if (other == this.current_spool)
          this.spool_up_to_date = false;
      }
      else
        this.current_spool = new FilamentSpool(other);
      this.CheckForUpdatedValues();
      AsyncCallObject asyncCallObject = (AsyncCallObject) null;
      bool flag1 = false;
      bool flag2 = false;
      lock (this.waiting_object_lock)
      {
        if (this.waiting_object != null)
        {
          uint lastCompletedRpcid = this.Info.synchronization.LastCompletedRPCID;
          if (this.Info.Status == PrinterStatus.Firmware_Idle && this.can_check_idle.Value)
            flag1 = true;
          if (this.waiting_object.callbackType == CallBackType.FirmwareMode && !this.Info.InFirmwareMode || this.waiting_object.callbackType == CallBackType.BootloaderMode && !this.Info.InBootloaderMode)
            this.inRequestedMode = false;
          if (this.waiting_object.callbackType == CallBackType.CallID && (int) this.waiting_object.callID == (int) lastCompletedRpcid || !this.inRequestedMode && (this.waiting_object.callbackType == CallBackType.FirmwareMode && this.Info.InFirmwareMode || this.waiting_object.callbackType == CallBackType.BootloaderMode && this.Info.InBootloaderMode))
          {
            flag2 = true;
            flag1 = false;
          }
          if (flag1 | flag2)
          {
            asyncCallObject = this.waiting_object;
            this.waiting_object = (AsyncCallObject) null;
          }
        }
      }
      if (asyncCallObject != null)
      {
        asyncCallObject.callresult = CommandResult.Success;
        asyncCallObject.idle_callback = flag1;
        ThreadPool.QueueUserWorkItem(new WaitCallback(this.DoAsyncCallBack), (object) asyncCallObject);
        lock (this.finished_lock)
        {
          if (this.AllCommandsFinished != null)
            this.AllCommandsFinished((IAsyncCallResult) new SimpleAsyncCallResult((object) this, CommandResult.Success));
        }
      }
      if (this.HasLock && this.Info.IsIdle)
      {
        int num = this.lockTimeOutSeconds.Value;
        if (!this.keeplockalive_clock.IsRunning)
          this.keeplockalive_clock.Restart();
        if (!this.keeplockalive_limit_clock.IsRunning && num > 0)
          this.keeplockalive_limit_clock.Restart();
        if (this.keeplockalive_clock.Elapsed.TotalSeconds > 15.0 && (num <= 0 || this.keeplockalive_limit_clock.Elapsed.TotalSeconds < (double) num))
        {
          int spooler = (int) this.SendRPCToSpooler("KeepLockAlive", 0U);
          this.keeplockalive_clock.Restart();
        }
      }
      else if (this.keeplockalive_clock.IsRunning)
      {
        this.keeplockalive_clock.Stop();
        this.keeplockalive_limit_clock.Stop();
      }
      if (this.Info.InBootloaderMode)
        this.m_bPluginsRegistered = false;
      else if (!this.m_bPluginsRegistered)
        this.DoPluginRegistration();
      if (this.OnUpdateData == null)
        return;
      this.OnUpdateData(info);
    }

    private void CheckForUpdatedValues()
    {
      foreach (KeyValuePair<string, string> keyValuePair in (Dictionary<string, string>) this.Info.persistantData.SavedData)
      {
        if (this.m_ChangedKeyValuePairs.ContainsKey(keyValuePair.Key) && keyValuePair.Value == this.m_ChangedKeyValuePairs[keyValuePair.Key])
        {
          string str;
          this.m_ChangedKeyValuePairs.TryRemove(keyValuePair.Key, out str);
        }
      }
    }

    public void ProcessSpoolerMessage(SpoolerMessage message)
    {
      AsyncCallObject asyncCallObject = (AsyncCallObject) null;
      bool flag = false;
      if (message.Type == MessageType.RawData)
      {
        lock (this.thread_sync)
          this.incoming_data = message.GetRawData();
      }
      else if (message.Type == MessageType.PluginMessage)
        this.ProcessPluginMessageFromSpooler(message);
      else if (message.Type == MessageType.LoggingMessage)
      {
        this.AddMessageToLog(Base64Convert.Base64Decode(message.Message));
        lock (this.log)
          this.log_updated = true;
      }
      else if (message.Type == MessageType.FullLoggingData)
      {
        string[] strArray = Base64Convert.Base64Decode(message.Message).Split('\n');
        this.log.Clear();
        foreach (string message1 in strArray)
          this.AddMessageToLog(message1);
        lock (this.log)
          this.log_updated = true;
      }
      else if (message.Type == MessageType.BedLocationMustBeCalibrated || message.Type == MessageType.BedOrientationMustBeCalibrated || message.Type == MessageType.CheckGantryClips)
      {
        this.mylockID = Guid.Empty;
        this.lockstatus.Value = PrinterLockStatus.Unlocked;
        lock (this.waiting_object_lock)
        {
          if (this.waiting_object != null)
          {
            asyncCallObject = this.waiting_object;
            this.waiting_object = (AsyncCallObject) null;
            asyncCallObject.callresult = CommandResult.Failed_GantryClipsOrInvalidZ;
          }
        }
      }
      else if ((message.Type == MessageType.LockConfirmed || message.Type == MessageType.LockResult) && message.SerialNumber == this.Info.serial_number)
      {
        uint num = 0;
        EventLockTimeOutCallBack callback = (EventLockTimeOutCallBack) null;
        CommandResult commandResult;
        if (message.Type == MessageType.LockResult)
        {
          string s = message.Message.Substring(0, 8);
          string str = message.Message.Substring(8);
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
            this.can_check_idle.Value = true;
            return;
          }
          if (commandResult == CommandResult.Pending)
          {
            this.lockstatus.Value = PrinterLockStatus.OurLockPending;
            return;
          }
          if (commandResult == CommandResult.LockForcedOpen)
            this.lockstatus.Value = PrinterLockStatus.Unlocked;
          else if (commandResult == CommandResult.LockLost_TimedOut)
          {
            lock (this.timeout_lock_sync)
            {
              callback = this.__LockTimeOutCallBack;
              this.__LockTimeOutCallBack = (EventLockTimeOutCallBack) null;
            }
          }
          if (commandResult != CommandResult.CommandInterruptedByM0)
            flag = true;
        }
        else
        {
          this.mylockID = Guid.Parse(message.Message);
          commandResult = CommandResult.Success_LockAcquired;
          this.lockstatus.Value = PrinterLockStatus.WeOwnLocked;
        }
        lock (this.waiting_object_lock)
        {
          if (this.waiting_object != null)
          {
            if (commandResult == CommandResult.SuccessfullyReceived)
            {
              if (this.waiting_object.callbackType != CallBackType.SuccessfullyReceived)
                goto label_59;
            }
            asyncCallObject = this.waiting_object;
            this.waiting_object = (AsyncCallObject) null;
          }
        }
label_59:
        if (flag)
        {
          this.mylockID = Guid.Empty;
          this.lockstatus.Value = PrinterLockStatus.Unlocked;
        }
        if (asyncCallObject != null)
        {
          if (num != 0U && (int) num != (int) asyncCallObject.callID)
            commandResult = CommandResult.Failed_AsyncCallbackError;
          asyncCallObject.callresult = commandResult;
        }
        if (callback != null)
          ThreadPool.QueueUserWorkItem(new WaitCallback(this.DoTimeoutCallBack), (object) new Printer.TimeoutProperties(callback, (IPrinter) this));
      }
      if (asyncCallObject != null)
        ThreadPool.QueueUserWorkItem(new WaitCallback(this.DoAsyncCallBack), (object) asyncCallObject);
      if (this.OnProcessSpoolerMessage == null)
        return;
      this.OnProcessSpoolerMessage(message);
    }

    public bool LogUpdated
    {
      get
      {
        lock (this.log)
          return this.log_updated;
      }
    }

    public List<string> GetLog()
    {
      List<string> stringList = (List<string>) null;
      lock (this.log)
      {
        stringList = new List<string>((IEnumerable<string>) this.log);
        this.log_updated = false;
      }
      return stringList;
    }

    public List<string> GetNextLogItems()
    {
      List<string> stringList = (List<string>) null;
      lock (this.log)
      {
        stringList = new List<string>((IEnumerable<string>) this.log);
        this.log.Clear();
        this.log_updated = false;
      }
      return stringList;
    }

    public void ClearLog()
    {
      lock (this.log)
      {
        this.log.Clear();
        this.log_updated = true;
      }
    }

    private void AddMessageToLog(string message)
    {
      if (!this.LogWaits && message == ">> wait" || message.StartsWith(">> T:") && !this.LogFeedback)
        return;
      lock (this.log)
        this.log.Enqueue(message);
    }

    public PrinterProfile MyPrinterProfile
    {
      get
      {
        return this.m_printer_profile.Value;
      }
    }

    public PrinterInfo Info
    {
      get
      {
        return this.printer_info;
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
