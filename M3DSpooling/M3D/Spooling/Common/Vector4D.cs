// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Vector4D
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
      this.X = other.X;
      this.Y = other.Y;
      this.Z = other.Z;
      this.E = other.E;
    }

    public Vector4D(float x, float y, float z, float e)
    {
      this.X = x;
      this.Y = y;
      this.Z = z;
      this.E = e;
    }

    public void Reset()
    {
      this.X = 0.0f;
      this.Y = 0.0f;
      this.Z = 0.0f;
      this.E = 0.0f;
    }

    public float this[int key]
    {
      get
      {
        switch (key)
        {
          case 0:
            return this.X;
          case 1:
            return this.Y;
          case 2:
            return this.Z;
          case 3:
            return this.E;
          default:
            throw new IndexOutOfRangeException();
        }
      }
      set
      {
        switch (key)
        {
          case 0:
            this.X = value;
            break;
          case 1:
            this.Y = value;
            break;
          case 2:
            this.Z = value;
            break;
          case 3:
            this.E = value;
            break;
          default:
            throw new IndexOutOfRangeException();
        }
      }
    }
  }
}
