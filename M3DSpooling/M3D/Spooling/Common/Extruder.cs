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
