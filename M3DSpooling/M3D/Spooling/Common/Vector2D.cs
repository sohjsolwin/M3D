// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Vector2D
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
