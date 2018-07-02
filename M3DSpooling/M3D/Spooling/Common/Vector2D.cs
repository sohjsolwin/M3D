using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public struct Vector2D
  {
    [XmlAttribute]
    public float x;
    [XmlAttribute]
    public float y;

    public Vector2D(float x, float y)
    {
      this.x = x;
      this.y = y;
    }

    public Vector2D(Vector2D other)
    {
      x = other.x;
      y = other.y;
    }
  }
}
