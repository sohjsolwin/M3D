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
