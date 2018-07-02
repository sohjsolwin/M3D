using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public struct Vector3DE
  {
    [XmlElement]
    public Vector3D pos;
    [XmlAttribute]
    public float e;

    public Vector3DE(Vector3DE other)
    {
      pos.x = other.pos.x;
      pos.y = other.pos.y;
      pos.z = other.pos.z;
      e = other.e;
    }

    public Vector3DE(float x, float y, float z, float e)
    {
      pos.x = x;
      pos.y = y;
      pos.z = z;
      this.e = e;
    }

    public void Reset()
    {
      pos.x = 0.0f;
      pos.y = 0.0f;
      pos.z = 0.0f;
      e = 0.0f;
    }
  }
}
