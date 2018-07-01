// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.ResetCauseEnum
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
