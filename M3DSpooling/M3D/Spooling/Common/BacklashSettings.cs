using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public struct BacklashSettings
  {
    [XmlAttribute]
    public float backlash_x;
    [XmlAttribute]
    public float backlash_y;
    [XmlAttribute]
    public float backlash_speed;

    public BacklashSettings(BacklashSettings other)
    {
      backlash_x = other.backlash_x;
      backlash_y = other.backlash_y;
      backlash_speed = other.backlash_speed;
    }

    public BacklashSettings(float backlash_x, float backlash_y, float backlash_speed)
    {
      this.backlash_x = backlash_x;
      this.backlash_y = backlash_y;
      this.backlash_speed = backlash_speed;
    }
  }
}
