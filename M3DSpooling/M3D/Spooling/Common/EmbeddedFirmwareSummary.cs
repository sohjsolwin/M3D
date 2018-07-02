// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.EmbeddedFirmwareSummary
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System.Collections.Generic;

namespace M3D.Spooling.Common
{
  public class EmbeddedFirmwareSummary
  {
    public string Name;
    public List<FirmwareBoardVersionKVP> FirmwareVersions;

    public EmbeddedFirmwareSummary()
    {
      FirmwareVersions = new List<FirmwareBoardVersionKVP>();
    }

    public EmbeddedFirmwareSummary(string name)
      : this()
    {
      Name = name;
    }

    public override string ToString()
    {
      return "M3D " + Name;
    }
  }
}
