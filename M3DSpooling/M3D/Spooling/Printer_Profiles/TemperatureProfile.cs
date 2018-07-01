// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Printer_Profiles.TemperatureProfile
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
      if (temp < this.MinTemp)
        return this.MinTemp;
      if (temp > this.MaxTemp)
        return this.MaxTemp;
      return temp;
    }

    public TemperatureProfile(int MinTemp, int MaxTemp)
    {
      this.MinTemp = MinTemp;
      this.MaxTemp = MaxTemp;
    }

    public TemperatureProfile(TemperatureProfile other)
    {
      this.MinTemp = other.MinTemp;
      this.MaxTemp = other.MaxTemp;
    }

    public TemperatureProfile()
    {
    }
  }
}
