// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.CommandResult
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

namespace M3D.Spooling.Common
{
  public enum CommandResult
  {
    Success,
    SuccessfullyReceived,
    OverridedByNonLockStepCall,
    Failed_PrinterDoesNotHaveLock,
    Failed_LockNotReady,
    Failed_CannotLockWhilePrinting,
    Failed_PrinterAlreadyLocked,
    Failed_PreviousCommandNotCompleted,
    Failed_TimedOut,
    Failed_Exception,
    Failed_AsyncCallbackError,
    Failed_NotInBootloader,
    Failed_NotInFirmware,
    Failed_NoAvailableController,
    Failed_GantryClipsOrInvalidZ,
    Failed_PrinterNotPaused,
    Failed_PrinterNotPrinting,
    Failed_CannotPauseSavingToSD,
    Failed_ThePrinterIsPrintingOrPaused,
    Failed_NotImplemented,
    Failed_Argument,
    Failed_FeatureNotAvailableOnPrinter,
    Failed_PrinterInUnconfiguredState,
    Failed_G28_G30_G32_NotAllowedWhilePaused,
    Failed_PluginGCodeConflict,
    Failed_PrinterDoesNotHaveFilament,
    Failed_RequiredDataNotAvailable,
    LockLost_TimedOut,
    Pending,
    CommandInterruptedByM0,
    Success_LockReleased,
    Success_LockAcquired,
    LockForcedOpen,
  }
}
