using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public class Accessories
  {
    [XmlElement("HeatedBed")]
    public HeatedBed BedStatus;
    [XmlElement("SDCard")]
    public SDCard SDCardStatus;

    public Accessories(Accessories rhs)
    {
      BedStatus = new HeatedBed(rhs.BedStatus);
      SDCardStatus = new SDCard(rhs.SDCardStatus);
    }

    public Accessories()
    {
      BedStatus = new HeatedBed();
      SDCardStatus = new SDCard();
    }
  }
}
