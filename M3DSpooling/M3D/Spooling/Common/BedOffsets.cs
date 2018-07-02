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
