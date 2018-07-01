// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.PrinterStatus
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

namespace M3D.Spooling.Common
{
  public enum PrinterStatus
  {
    Error_PrinterNotAlive,
    Uninitialized,
    Connecting,
    Connected,
    Firmware_Ready,
    Firmware_Printing,
    Firmware_PrintingPaused,
    Firmware_PrintingPausedProcessing,
    Firmware_IsWaitingToPause,
    Firmware_Calibrating,
    Firmware_Homing,
    Firmware_WarmingUp,
    Firmware_Idle,
    Firmware_Executing,
    Firmware_PowerRecovery,
    Bootloader_Ready,
    Bootloader_UpdatingFirmware,
    Bootloader_InvalidFirmware,
    Bootloader_FirmwareUpdateFailed,
    Bootloader_StartingUp,
  }
}
