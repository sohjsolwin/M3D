// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Extruder
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public class Extruder
  {
    [XmlAttribute("Homed")]
    public Trilean ishomed;
    [XmlAttribute("InRelativeMode")]
    public Trilean inRelativeMode;
    [XmlElement("LastKnownExtruderLocation")]
    public Vector3DE position;
    [XmlAttribute]
    public bool Z_Valid;
    [XmlAttribute]
    public float Temperature;
    [XmlAttribute]
    public byte Fan;
    [XmlAttribute]
    public int iNozzleSizeMicrons;

    public Extruder(Extruder rhs)
    {
      this.ishomed = rhs.ishomed;
      this.inRelativeMode = rhs.inRelativeMode;
      this.position = rhs.position;
      this.Z_Valid = rhs.Z_Valid;
      this.Temperature = rhs.Temperature;
      this.iNozzleSizeMicrons = rhs.iNozzleSizeMicrons;
      this.Fan = rhs.Fan;
    }

    public Extruder()
    {
      this.ishomed = Trilean.Unknown;
      this.inRelativeMode = Trilean.Unknown;
      this.position = new Vector3DE(0.0f, 0.0f, 0.0f, 0.0f);
      this.Z_Valid = true;
      this.Temperature = -273f;
      this.Fan = (byte) 0;
      this.iNozzleSizeMicrons = 0;
    }
  }
}
