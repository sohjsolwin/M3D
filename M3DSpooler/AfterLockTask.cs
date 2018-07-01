// Decompiled with JetBrains decompiler
// Type: M3D.Spooler.AfterLockTask
// Assembly: M3DSpooler, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 88304559-4DEB-43F9-B51D-EEDFDC1EF62E
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\Resources\Spooler\M3DSpooler.exe

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
