using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public struct Vector3D
  {
    [XmlAttribute]
    public float x;
    [XmlAttribute]
    public float y;
    [XmlAttribute]
    public float z;

    public Vector3D(float x, float y, float z)
    {
      this.x = x;
      this.y = y;
      this.z = z;
    }

    public Vector3D(Vector3D other)
    {
      x = other.x;
      y = other.y;
      z = other.z;
    }
  }
}
