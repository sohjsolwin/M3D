// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Calibration
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public class Calibration
  {
    [XmlAttribute]
    public bool Calibration_Valid;
    [XmlAttribute]
    public int G32_VERSION;
    [XmlAttribute]
    public float CORNER_HEIGHT_BACK_RIGHT;
    [XmlAttribute]
    public float CORNER_HEIGHT_BACK_LEFT;
    [XmlAttribute]
    public float CORNER_HEIGHT_FRONT_LEFT;
    [XmlAttribute]
    public float CORNER_HEIGHT_FRONT_RIGHT;
    [XmlAttribute]
    public float CORNER_HEIGHT_BACK_RIGHT_OFFSET;
    [XmlAttribute]
    public float CORNER_HEIGHT_BACK_LEFT_OFFSET;
    [XmlAttribute]
    public float CORNER_HEIGHT_FRONT_LEFT_OFFSET;
    [XmlAttribute]
    public float CORNER_HEIGHT_FRONT_RIGHT_OFFSET;
    [XmlAttribute]
    public float ENTIRE_Z_HEIGHT_OFFSET;
    [XmlAttribute]
    public float BACKLASH_X;
    [XmlAttribute]
    public float BACKLASH_Y;
    [XmlAttribute]
    public float BACKLASH_SPEED;
    [XmlAttribute]
    public float CALIBRATION_OFFSET;
    [XmlAttribute]
    public bool UsesCalibrationOffset;

    public Calibration(Calibration rhs)
    {
      this.Calibration_Valid = rhs.Calibration_Valid;
      this.G32_VERSION = rhs.G32_VERSION;
      this.CORNER_HEIGHT_BACK_LEFT = rhs.CORNER_HEIGHT_BACK_LEFT;
      this.CORNER_HEIGHT_BACK_LEFT_OFFSET = rhs.CORNER_HEIGHT_BACK_LEFT_OFFSET;
      this.CORNER_HEIGHT_BACK_RIGHT = rhs.CORNER_HEIGHT_BACK_RIGHT;
      this.CORNER_HEIGHT_BACK_RIGHT_OFFSET = rhs.CORNER_HEIGHT_BACK_RIGHT_OFFSET;
      this.CORNER_HEIGHT_FRONT_LEFT = rhs.CORNER_HEIGHT_FRONT_LEFT;
      this.CORNER_HEIGHT_FRONT_LEFT_OFFSET = rhs.CORNER_HEIGHT_FRONT_LEFT_OFFSET;
      this.CORNER_HEIGHT_FRONT_RIGHT = rhs.CORNER_HEIGHT_FRONT_RIGHT;
      this.CORNER_HEIGHT_FRONT_RIGHT_OFFSET = rhs.CORNER_HEIGHT_FRONT_RIGHT_OFFSET;
      this.ENTIRE_Z_HEIGHT_OFFSET = rhs.ENTIRE_Z_HEIGHT_OFFSET;
      this.BACKLASH_X = rhs.BACKLASH_X;
      this.BACKLASH_Y = rhs.BACKLASH_Y;
      this.BACKLASH_SPEED = rhs.BACKLASH_SPEED;
      this.CALIBRATION_OFFSET = rhs.CALIBRATION_OFFSET;
      this.UsesCalibrationOffset = rhs.UsesCalibrationOffset;
    }

    public Calibration()
    {
      this.Calibration_Valid = true;
      this.G32_VERSION = (int) byte.MaxValue;
      this.CORNER_HEIGHT_BACK_RIGHT = 0.0f;
      this.CORNER_HEIGHT_BACK_LEFT = 0.0f;
      this.CORNER_HEIGHT_FRONT_LEFT = 0.0f;
      this.CORNER_HEIGHT_FRONT_RIGHT = 0.0f;
      this.CORNER_HEIGHT_BACK_RIGHT_OFFSET = 0.0f;
      this.CORNER_HEIGHT_BACK_LEFT_OFFSET = 0.0f;
      this.CORNER_HEIGHT_FRONT_LEFT_OFFSET = 0.0f;
      this.CORNER_HEIGHT_FRONT_RIGHT_OFFSET = 0.0f;
      this.ENTIRE_Z_HEIGHT_OFFSET = 0.0f;
      this.BACKLASH_X = 0.0f;
      this.BACKLASH_Y = 0.0f;
      this.BACKLASH_SPEED = 2900f;
      this.CALIBRATION_OFFSET = 0.0f;
      this.UsesCalibrationOffset = false;
    }
  }
}
