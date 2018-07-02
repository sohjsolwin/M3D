using System;
using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public struct Vector4D
  {
    [XmlAttribute]
    public float X;
    [XmlAttribute]
    public float Y;
    [XmlAttribute]
    public float Z;
    [XmlAttribute]
    public float E;

    public Vector4D(Vector4D other)
    {
      X = other.X;
      Y = other.Y;
      Z = other.Z;
      E = other.E;
    }

    public Vector4D(float x, float y, float z, float e)
    {
      X = x;
      Y = y;
      Z = z;
      E = e;
    }

    public void Reset()
    {
      X = 0.0f;
      Y = 0.0f;
      Z = 0.0f;
      E = 0.0f;
    }

    public float this[int key]
    {
      get
      {
        switch (key)
        {
          case 0:
            return X;
          case 1:
            return Y;
          case 2:
            return Z;
          case 3:
            return E;
          default:
            throw new IndexOutOfRangeException();
        }
      }
      set
      {
        switch (key)
        {
          case 0:
            X = value;
            break;
          case 1:
            Y = value;
            break;
          case 2:
            Z = value;
            break;
          case 3:
            E = value;
            break;
          default:
            throw new IndexOutOfRangeException();
        }
      }
    }
  }
}
