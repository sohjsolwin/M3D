namespace M3D.Spooler
{
  internal enum AfterLockTask
  {
    DoFirmwareUpdate,
    CalibrateBedLocationG30,
    CalibrateGantryG32,
    ReleaseLock,
    AbortPrint,
    ClearWarning,
    CheckGantryClips,
    ManualBedClear,
    RecoverFromPowerOutage,
    RecoverFromPowerOutageG28,
    ClearPowerRecoveryFault,
    None,
  }
}
