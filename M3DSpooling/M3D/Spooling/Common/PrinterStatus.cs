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
