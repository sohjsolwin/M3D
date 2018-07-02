using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public class HeatedBed
  {
    [XmlAttribute("BedTemperature")]
    public float BedTemperature;

    public HeatedBed()
    {
      BedTemperature = 0.0f;
    }

    public HeatedBed(HeatedBed other)
    {
      BedTemperature = other.BedTemperature;
    }
  }
}
