// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Printer_Profiles.SpeedLimitProfile
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System.Xml.Serialization;

namespace M3D.Spooling.Printer_Profiles
{
  public class SpeedLimitProfile
  {
    [XmlAttribute]
    public float MAX_FEEDRATE_X;
    [XmlAttribute]
    public float MIN_FEEDRATE_X;
    [XmlAttribute]
    public float DEFAULT_FEEDRATE_X;
    [XmlAttribute]
    public float MAX_FEEDRATE_Y;
    [XmlAttribute]
    public float MIN_FEEDRATE_Y;
    [XmlAttribute]
    public float DEFAULT_FEEDRATE_Y;
    [XmlAttribute]
    public float MAX_FEEDRATE_Z;
    [XmlAttribute]
    public float MIN_FEEDRATE_Z;
    [XmlAttribute]
    public float DEFAULT_FEEDRATE_Z;
    [XmlAttribute]
    public float MAX_FEEDRATE_E_Positive;
    [XmlAttribute]
    public float MIN_FEEDRATE_E_Positive;
    [XmlAttribute]
    public float DEFAULT_FEEDRATE_E_Positive;
    [XmlAttribute]
    public float MAX_FEEDRATE_E_Negative;
    [XmlAttribute]
    public float MIN_FEEDRATE_E_Negative;
    [XmlAttribute]
    public float DEFAULT_FEEDRATE_E_Negative;
    [XmlAttribute]
    public float FastestPossible;
    [XmlAttribute]
    public float DefaultSpeed;
    [XmlAttribute]
    public float DefaultBacklashSpeed;

    public SpeedLimitProfile()
    {
    }

    public SpeedLimitProfile(SpeedLimitProfile other)
    {
      this.MAX_FEEDRATE_X = other.MAX_FEEDRATE_X;
      this.MIN_FEEDRATE_X = other.MIN_FEEDRATE_X;
      this.DEFAULT_FEEDRATE_X = other.DEFAULT_FEEDRATE_X;
      this.MAX_FEEDRATE_Y = other.MAX_FEEDRATE_Y;
      this.MIN_FEEDRATE_Y = other.MIN_FEEDRATE_Y;
      this.DEFAULT_FEEDRATE_Y = other.DEFAULT_FEEDRATE_Y;
      this.MAX_FEEDRATE_Z = other.MAX_FEEDRATE_Z;
      this.MIN_FEEDRATE_Z = other.MIN_FEEDRATE_Z;
      this.DEFAULT_FEEDRATE_Z = other.DEFAULT_FEEDRATE_Z;
      this.MAX_FEEDRATE_E_Positive = other.MAX_FEEDRATE_E_Positive;
      this.MIN_FEEDRATE_E_Positive = other.MIN_FEEDRATE_E_Positive;
      this.DEFAULT_FEEDRATE_E_Positive = other.DEFAULT_FEEDRATE_E_Positive;
      this.MAX_FEEDRATE_E_Negative = other.MAX_FEEDRATE_E_Negative;
      this.MIN_FEEDRATE_E_Negative = other.MIN_FEEDRATE_E_Negative;
      this.DEFAULT_FEEDRATE_E_Negative = other.DEFAULT_FEEDRATE_E_Negative;
      this.FastestPossible = other.FastestPossible;
      this.DefaultSpeed = other.DefaultSpeed;
      this.DefaultBacklashSpeed = other.DefaultBacklashSpeed;
    }

    public SpeedLimitProfile(float MAX_FEEDRATE_X, float MIN_FEEDRATE_X, float DEFAULT_FEEDRATE_X, float MAX_FEEDRATE_Y, float MIN_FEEDRATE_Y, float DEFAULT_FEEDRATE_Y, float MAX_FEEDRATE_Z, float MIN_FEEDRATE_Z, float DEFAULT_FEEDRATE_Z, float MAX_FEEDRATE_E_Positive, float MIN_FEEDRATE_E_Positive, float DEFAULT_FEEDRATE_E_Positive, float MAX_FEEDRATE_E_Negative, float MIN_FEEDRATE_E_Negative, float DEFAULT_FEEDRATE_E_Negative, float FastestPossible, float DefaultSpeed, float DefaultBacklashSpeed)
    {
      this.MAX_FEEDRATE_X = MAX_FEEDRATE_X;
      this.MIN_FEEDRATE_X = MIN_FEEDRATE_X;
      this.DEFAULT_FEEDRATE_X = DEFAULT_FEEDRATE_X;
      this.MAX_FEEDRATE_Y = MAX_FEEDRATE_Y;
      this.MIN_FEEDRATE_Y = MIN_FEEDRATE_Y;
      this.DEFAULT_FEEDRATE_Y = DEFAULT_FEEDRATE_Y;
      this.MAX_FEEDRATE_Z = MAX_FEEDRATE_Z;
      this.MIN_FEEDRATE_Z = MIN_FEEDRATE_Z;
      this.DEFAULT_FEEDRATE_Z = DEFAULT_FEEDRATE_Z;
      this.MAX_FEEDRATE_E_Positive = MAX_FEEDRATE_E_Positive;
      this.MIN_FEEDRATE_E_Positive = MIN_FEEDRATE_E_Positive;
      this.DEFAULT_FEEDRATE_E_Positive = DEFAULT_FEEDRATE_E_Positive;
      this.MAX_FEEDRATE_E_Negative = MAX_FEEDRATE_E_Negative;
      this.MIN_FEEDRATE_E_Negative = MIN_FEEDRATE_E_Negative;
      this.DEFAULT_FEEDRATE_E_Negative = DEFAULT_FEEDRATE_E_Negative;
      this.FastestPossible = FastestPossible;
      this.DefaultSpeed = DefaultSpeed;
      this.DefaultBacklashSpeed = DefaultBacklashSpeed;
    }
  }
}
