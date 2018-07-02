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
      ishomed = rhs.ishomed;
      inRelativeMode = rhs.inRelativeMode;
      position = rhs.position;
      Z_Valid = rhs.Z_Valid;
      Temperature = rhs.Temperature;
      iNozzleSizeMicrons = rhs.iNozzleSizeMicrons;
      Fan = rhs.Fan;
    }

    public Extruder()
    {
      ishomed = Trilean.Unknown;
      inRelativeMode = Trilean.Unknown;
      position = new Vector3DE(0.0f, 0.0f, 0.0f, 0.0f);
      Z_Valid = true;
      Temperature = -273f;
      Fan = (byte) 0;
      iNozzleSizeMicrons = 0;
    }
  }
}
