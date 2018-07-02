// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Statistics
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public class Statistics
  {
    [XmlAttribute]
    public bool isMetering;
    [XmlAttribute]
    public double AvgCMDsBeforeRS;
    [XmlAttribute]
    public double AvgRSDelay;
    [XmlAttribute]
    public double RSDelayStandardDeviation;

    public Statistics(Statistics rhs)
    {
      isMetering = rhs.isMetering;
      AvgCMDsBeforeRS = rhs.AvgCMDsBeforeRS;
      AvgRSDelay = rhs.AvgRSDelay;
      RSDelayStandardDeviation = rhs.RSDelayStandardDeviation;
    }

    public Statistics()
    {
      isMetering = false;
      AvgCMDsBeforeRS = 0.0;
      AvgRSDelay = 0.0;
      RSDelayStandardDeviation = 0.0;
    }
  }
}
