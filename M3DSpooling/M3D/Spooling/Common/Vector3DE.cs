// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Vector3DE
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
      this.pos.x = other.pos.x;
      this.pos.y = other.pos.y;
      this.pos.z = other.pos.z;
      this.e = other.e;
    }

    public Vector3DE(float x, float y, float z, float e)
    {
      this.pos.x = x;
      this.pos.y = y;
      this.pos.z = z;
      this.e = e;
    }

    public void Reset()
    {
      this.pos.x = 0.0f;
      this.pos.y = 0.0f;
      this.pos.z = 0.0f;
      this.e = 0.0f;
    }
  }
}
