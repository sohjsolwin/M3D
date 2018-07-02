namespace M3D.Spooling.Common
{
  public enum ResetCauseEnum
  {
    None = 0,
    PoweronReset = 1,
    ExternalReset = 2,
    BrownoutReset = 4,
    WatchdogReset = 8,
    ProgrammingDebugInterfaceReset = 16, // 0x00000010
    SoftwareReset = 32, // 0x00000020
    SpikeDetectionReset = 64, // 0x00000040
  }
}
