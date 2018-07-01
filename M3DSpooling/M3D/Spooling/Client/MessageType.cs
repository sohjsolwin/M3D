// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Client.MessageType
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

namespace M3D.Spooling.Client
{
  public enum MessageType
  {
    ErrorUndefinedMessage,
    PrinterConnected,
    PrinterShutdown,
    JobComplete,
    JobCanceled,
    JobNotStarted,
    JobStarted,
    PrinterError,
    PrinterMessage,
    FirmwareUpdateComplete,
    FirmwareUpdateFailed,
    ResetPrinterConnection,
    UserDefined,
    RawData,
    PrinterNotConnected,
    BedLocationMustBeCalibrated,
    BedOrientationMustBeCalibrated,
    MicroMotionControllerFailed,
    InvalidZ,
    ModelOutOfPrintableBounds,
    PrinterTimeout,
    WarningABSPrintLarge,
    IncompatibleSpooler,
    UnexpectedDisconnect,
    CantStartJobPrinterBusy,
    BacklashOutOfRange,
    CheckGantryClips,
    FirmwareMustBeUpdated,
    FirmwareErrorCyclePower,
    SinglePointCalibrationNotSupported,
    MultiPointCalibrationNotSupported,
    LoggingMessage,
    FullLoggingData,
    RPCError,
    LockResult,
    LockConfirmed,
    PluginMessage,
    SDPrintIncompatibleFilament,
    PowerOutageWhilePrinting,
    PowerOutageRecovery,
  }
}
