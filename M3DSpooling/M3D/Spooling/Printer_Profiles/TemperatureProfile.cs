using System.Xml.Serialization;

namespace M3D.Spooling.Printer_Profiles
{
  public class TemperatureProfile
  {
    [XmlAttribute]
    public int MinTemp;
    [XmlAttribute]
    public int MaxTemp;

    public int GetBoundedTemp(int temp)
    {
      if (temp < MinTemp)
      {
        return MinTemp;
      }

      if (temp > MaxTemp)
      {
        return MaxTemp;
      }

      return temp;
    }

    public TemperatureProfile(int MinTemp, int MaxTemp)
    {
      this.MinTemp = MinTemp;
      this.MaxTemp = MaxTemp;
    }

    public TemperatureProfile(TemperatureProfile other)
    {
      MinTemp = other.MinTemp;
      MaxTemp = other.MaxTemp;
    }

    public TemperatureProfile()
    {
    }
  }
}
