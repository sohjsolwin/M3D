// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.BacklashSettings
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public struct BacklashSettings
  {
    [XmlAttribute]
    public float backlash_x;
    [XmlAttribute]
    public float backlash_y;
    [XmlAttribute]
    public float backlash_speed;

    public BacklashSettings(BacklashSettings other)
    {
      this.backlash_x = other.backlash_x;
      this.backlash_y = other.backlash_y;
      this.backlash_speed = other.backlash_speed;
    }

    public BacklashSettings(float backlash_x, float backlash_y, float backlash_speed)
    {
      this.backlash_x = backlash_x;
      this.backlash_y = backlash_y;
      this.backlash_speed = backlash_speed;
    }
  }
}
