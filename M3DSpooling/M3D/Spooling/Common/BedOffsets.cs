// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.BedOffsets
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public struct BedOffsets
  {
    [XmlAttribute]
    public float BL;
    [XmlAttribute]
    public float BR;
    [XmlAttribute]
    public float FR;
    [XmlAttribute]
    public float FL;
    [XmlAttribute]
    public float ZO;

    public BedOffsets(BedOffsets other)
    {
      BL = other.BL;
      BR = other.BR;
      FR = other.FR;
      FL = other.FL;
      ZO = other.ZO;
    }

    public BedOffsets(float BL, float BR, float FR, float FL, float ZO)
    {
      this.BL = BL;
      this.BR = BR;
      this.FR = FR;
      this.FL = FL;
      this.ZO = ZO;
    }
  }
}
