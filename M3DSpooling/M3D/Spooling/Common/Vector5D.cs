// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Vector5D
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
