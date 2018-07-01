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
      this.X = other.X;
      this.Y = other.Y;
      this.Z = other.Z;
      this.E = other.E;
      this.F = other.F;
    }

    public Vector5D(float x, float y, float z, float e, float f)
    {
      this.X = x;
      this.Y = y;
      this.Z = z;
      this.E = e;
      this.F = f;
    }

    public void Reset()
    {
      this.X = -1000f;
      this.Y = -1000f;
      this.Z = -1000f;
      this.E = -1000f;
      this.F = -1000f;
    }
  }
}
