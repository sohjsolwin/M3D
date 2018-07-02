using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public struct Vector5D
  {
    [XmlAttribute]
    public float X;
    [XmlAttribute]
    public float Y;
    [XmlAttribute]
    public float Z;
    [XmlAttribute]
    public float E;
    [XmlAttribute]
    public float F;

    public Vector5D(Vector5D other)
    {
      X = other.X;
      Y = other.Y;
      Z = other.Z;
      E = other.E;
      F = other.F;
    }

    public Vector5D(float x, float y, float z, float e, float f)
    {
      X = x;
      Y = y;
      Z = z;
      E = e;
      F = f;
    }

    public void Reset()
    {
      X = -1000f;
      Y = -1000f;
      Z = -1000f;
      E = -1000f;
      F = -1000f;
    }
  }
}
