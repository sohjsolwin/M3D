using M3D.Spooling.Common;
using M3D.Spooling.Printer_Profiles;
using System.Collections.Generic;

namespace M3D.Spooling.Client
{
  public interface IPrinter
  {
    bool RegisterPlugin(string ID, IPrinterPlugin plugin);

    IPrinterPlugin GetPrinterPlugin(string ID);

    SpoolerResult PrintModel(AsyncCallback callback, object state, JobParams jobParams);

    SpoolerResult SendManualGCode(AsyncCallback callback, object state, params string[] gcode);

    byte[] SendSerialDataWaitForResponse(byte[] data, int bytesToReceive);

    bool Switching { get; }

    bool LockStepMode { get; set; }

    PrinterLockStatus LockStatus { get; }

    SpoolerResult BreakLock(AsyncCallback callback, object state);

    SpoolerResult AcquireLock(AsyncCallback callback, object state);

    SpoolerResult AcquireLock(AsyncCallback callback, object state, EventLockTimeOutCallBack LockTimeOutCallBack, int locktimeoutseconds);

    SpoolerResult ReleaseLock(AsyncCallback callback, object state);

    SpoolerResult SendSerialData(AsyncCallback callback, object state, byte[] data);

    SpoolerResult CancelQueuedPrint(AsyncCallback callback, object state);

    SpoolerResult PausePrint(AsyncCallback callback, object state);

    SpoolerResult ContinuePrint(AsyncCallback callback, object state);

    SpoolerResult DoFirmwareUpdate(AsyncCallback callback, object state);

    SpoolerResult ClearCurrentWarning(AsyncCallback callback, object state);

    SpoolerResult AbortPrint(AsyncCallback callback, object state);

    SpoolerResult SetFilamentInfo(AsyncCallback callback, object state, FilamentSpool info);

    SpoolerResult SetFilamentToNone(AsyncCallback callback, object state);

    SpoolerResult SetFilamentUID(AsyncCallback callback, object state, uint filamentUID);

    SpoolerResult PrintBacklashPrint(AsyncCallback callback, object state);

    SpoolerResult SendEmergencyStop(AsyncCallback callback, object state);

    SpoolerResult SetCalibrationOffset(AsyncCallback callback, object state, float offset);

    SpoolerResult SetOffsetInfo(AsyncCallback callback, object state, BedOffsets offsets);

    SpoolerResult SetBacklash(AsyncCallback callback, object state, BacklashSettings backlash);

    SpoolerResult SetTemperatureWhilePrinting(AsyncCallback callback, object state, int temperature);

    SpoolerResult SetNozzleWidth(AsyncCallback callback, object state, int iNozzleWidthMicrons);

    SpoolerResult AddUpdateKeyValuePair(AsyncCallback callback, object state, string key, string value);

    SpoolerResult ClearPowerRecoveryFault(AsyncCallback callback, object state);

    SpoolerResult RecoveryPrintFromPowerFailure(AsyncCallback callback, object state, bool bHomingRequired);

    string GetValidatedValueFromPrinter(string key);

    FilamentSpool GetCurrentFilament();

    void UpdateData(PrinterInfo info);

    void ProcessSpoolerMessage(SpoolerMessage message);

    bool IsConnected();

    void AddCommandToRunOnFinish(AsyncCallback callback);

    void ClearAsyncCallbacks();

    bool WaitingForCommandToComplete { get; }

    bool PrinterIsLocked { get; }

    bool HasLock { get; }

    PrinterInfo Info { get; }

    bool LogUpdated { get; }

    bool LogWaits { get; set; }

    bool LogFeedback { get; set; }

    List<string> GetLog();

    void ClearLog();

    PrinterProfile MyPrinterProfile { get; }
  }
}
