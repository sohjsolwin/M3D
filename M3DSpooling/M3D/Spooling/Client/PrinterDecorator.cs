// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Client.PrinterDecorator
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Common;
using M3D.Spooling.Printer_Profiles;
using System.Collections.Generic;

namespace M3D.Spooling.Client
{
  public class PrinterDecorator : IPrinter
  {
    protected IPrinter base_obj;

    public PrinterDecorator(IPrinter base_obj)
    {
      this.base_obj = base_obj;
    }

    public bool RegisterPlugin(string ID, IPrinterPlugin plugin)
    {
      return this.base_obj.RegisterPlugin(ID, plugin);
    }

    public IPrinterPlugin GetPrinterPlugin(string ID)
    {
      return this.base_obj.GetPrinterPlugin(ID);
    }

    public virtual SpoolerResult PrintModel(AsyncCallback callback, object state, JobParams jobParams)
    {
      return this.base_obj.PrintModel(callback, state, jobParams);
    }

    public bool Switching
    {
      get
      {
        return this.base_obj.Switching;
      }
    }

    public bool LockStepMode
    {
      get
      {
        return this.base_obj.LockStepMode;
      }
      set
      {
        this.base_obj.LockStepMode = value;
      }
    }

    public PrinterLockStatus LockStatus
    {
      get
      {
        return this.base_obj.LockStatus;
      }
    }

    public void AddCommandToRunOnFinish(AsyncCallback callback)
    {
      this.base_obj.AddCommandToRunOnFinish(callback);
    }

    public void ClearAsyncCallbacks()
    {
      this.base_obj.ClearAsyncCallbacks();
    }

    public SpoolerResult BreakLock(AsyncCallback callback, object state)
    {
      return this.base_obj.BreakLock(callback, state);
    }

    public SpoolerResult AcquireLock(AsyncCallback callback, object state)
    {
      return this.base_obj.AcquireLock(callback, state);
    }

    public SpoolerResult AcquireLock(AsyncCallback callback, object state, EventLockTimeOutCallBack LockTimeOutCallBack, int locktimeoutseconds)
    {
      return this.base_obj.AcquireLock(callback, state, LockTimeOutCallBack, locktimeoutseconds);
    }

    public SpoolerResult ReleaseLock(AsyncCallback callback, object state)
    {
      return this.base_obj.ReleaseLock(callback, state);
    }

    public bool WaitingForCommandToComplete
    {
      get
      {
        return this.base_obj.WaitingForCommandToComplete;
      }
    }

    public bool PrinterIsLocked
    {
      get
      {
        return this.base_obj.PrinterIsLocked;
      }
    }

    public bool HasLock
    {
      get
      {
        return this.base_obj.HasLock;
      }
    }

    public FilamentSpool GetCurrentFilament()
    {
      return this.base_obj.GetCurrentFilament();
    }

    public SpoolerResult SendManualGCode(AsyncCallback callback, object state, params string[] gcode)
    {
      return this.base_obj.SendManualGCode(callback, state, gcode);
    }

    public byte[] SendSerialDataWaitForResponse(byte[] data, int bytesToReceive)
    {
      return this.base_obj.SendSerialDataWaitForResponse(data, bytesToReceive);
    }

    public SpoolerResult SendSerialData(AsyncCallback callback, object state, byte[] data)
    {
      return this.base_obj.SendSerialData(callback, state, data);
    }

    public SpoolerResult CancelQueuedPrint(AsyncCallback callback, object state)
    {
      return this.base_obj.CancelQueuedPrint(callback, state);
    }

    public virtual SpoolerResult DoFirmwareUpdate(AsyncCallback callback, object state)
    {
      return this.base_obj.DoFirmwareUpdate(callback, state);
    }

    public SpoolerResult ClearCurrentWarning(AsyncCallback callback, object state)
    {
      return this.base_obj.ClearCurrentWarning(callback, state);
    }

    public virtual SpoolerResult PausePrint(AsyncCallback callback, object state)
    {
      return this.base_obj.PausePrint(callback, state);
    }

    public virtual SpoolerResult ContinuePrint(AsyncCallback callback, object state)
    {
      return this.base_obj.ContinuePrint(callback, state);
    }

    public SpoolerResult AbortPrint(AsyncCallback callback, object state)
    {
      return this.base_obj.AbortPrint(callback, state);
    }

    public virtual SpoolerResult SetFilamentInfo(AsyncCallback callback, object state, FilamentSpool info)
    {
      return this.base_obj.SetFilamentInfo(callback, state, info);
    }

    public virtual SpoolerResult SetFilamentToNone(AsyncCallback callback, object state)
    {
      return this.base_obj.SetFilamentToNone(callback, state);
    }

    public SpoolerResult SetFilamentUID(AsyncCallback callback, object state, uint filamentUID)
    {
      return this.base_obj.SetFilamentUID(callback, state, filamentUID);
    }

    public void UpdateData(PrinterInfo info)
    {
      this.base_obj.UpdateData(info);
    }

    public virtual void ProcessSpoolerMessage(SpoolerMessage message)
    {
      this.base_obj.ProcessSpoolerMessage(message);
    }

    public SpoolerResult PrintBacklashPrint(AsyncCallback callback, object state)
    {
      return this.base_obj.PrintBacklashPrint(callback, state);
    }

    public SpoolerResult SetCalibrationOffset(AsyncCallback callback, object state, float offset)
    {
      return this.base_obj.SetCalibrationOffset(callback, state, offset);
    }

    public SpoolerResult SetOffsetInfo(AsyncCallback callback, object state, BedOffsets offsets)
    {
      return this.base_obj.SetOffsetInfo(callback, state, offsets);
    }

    public SpoolerResult SendEmergencyStop(AsyncCallback callback, object state)
    {
      return this.base_obj.SendEmergencyStop(callback, state);
    }

    public SpoolerResult SetBacklash(AsyncCallback callback, object state, BacklashSettings backlash)
    {
      return this.base_obj.SetBacklash(callback, state, backlash);
    }

    public SpoolerResult SetTemperatureWhilePrinting(AsyncCallback callback, object state, int temperature)
    {
      return this.base_obj.SetTemperatureWhilePrinting(callback, state, temperature);
    }

    public SpoolerResult SetNozzleWidth(AsyncCallback callback, object state, int iNozzleWidthMicrons)
    {
      return this.base_obj.SetNozzleWidth(callback, state, iNozzleWidthMicrons);
    }

    public SpoolerResult AddUpdateKeyValuePair(AsyncCallback callback, object state, string key, string value)
    {
      return this.base_obj.AddUpdateKeyValuePair(callback, state, key, value);
    }

    public SpoolerResult ClearPowerRecoveryFault(AsyncCallback callback, object state)
    {
      return this.base_obj.ClearPowerRecoveryFault(callback, state);
    }

    public SpoolerResult RecoveryPrintFromPowerFailure(AsyncCallback callback, object state, bool bHomingRequired)
    {
      return this.base_obj.RecoveryPrintFromPowerFailure(callback, state, bHomingRequired);
    }

    public string GetValidatedValueFromPrinter(string key)
    {
      return this.base_obj.GetValidatedValueFromPrinter(key);
    }

    public bool isConnected()
    {
      return this.base_obj.isConnected();
    }

    public bool Connected
    {
      get
      {
        return this.isConnected();
      }
    }

    public PrinterInfo Info
    {
      get
      {
        return this.base_obj.Info;
      }
    }

    public bool LogUpdated
    {
      get
      {
        return this.base_obj.LogUpdated;
      }
    }

    public bool LogWaits
    {
      get
      {
        return this.base_obj.LogWaits;
      }
      set
      {
        this.base_obj.LogWaits = value;
      }
    }

    public bool LogFeedback
    {
      get
      {
        return this.base_obj.LogFeedback;
      }
      set
      {
        this.base_obj.LogFeedback = value;
      }
    }

    public List<string> GetLog()
    {
      return this.base_obj.GetLog();
    }

    public void ClearLog()
    {
      this.base_obj.ClearLog();
    }

    public PrinterProfile MyPrinterProfile
    {
      get
      {
        return this.base_obj.MyPrinterProfile;
      }
    }
  }
}
