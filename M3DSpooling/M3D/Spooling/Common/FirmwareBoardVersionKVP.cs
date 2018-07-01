// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.FirmwareBoardVersionKVP
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

namespace M3D.Spooling.Common
{
  public struct FirmwareBoardVersionKVP
  {
    public FirmwareBoardVersionKVP(char boardID, uint firmwareVersion)
    {
      this.BoardID = boardID;
      this.FirmwareVersion = firmwareVersion;
    }

    public char BoardID { get; set; }

    public uint FirmwareVersion { get; set; }

    public override string ToString()
    {
      string str = this.FirmwareVersion.ToString();
      return this.BoardID.ToString() + " - Firmware Version: " + str.Substring(0, 4) + "-" + str.Substring(4, 2) + "-" + str.Substring(6, 2) + "-" + str.Substring(8, 2);
    }
  }
}
