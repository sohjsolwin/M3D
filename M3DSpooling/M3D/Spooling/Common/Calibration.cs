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
      Calibration_Valid = rhs.Calibration_Valid;
      G32_VERSION = rhs.G32_VERSION;
      CORNER_HEIGHT_BACK_LEFT = rhs.CORNER_HEIGHT_BACK_LEFT;
      CORNER_HEIGHT_BACK_LEFT_OFFSET = rhs.CORNER_HEIGHT_BACK_LEFT_OFFSET;
      CORNER_HEIGHT_BACK_RIGHT = rhs.CORNER_HEIGHT_BACK_RIGHT;
      CORNER_HEIGHT_BACK_RIGHT_OFFSET = rhs.CORNER_HEIGHT_BACK_RIGHT_OFFSET;
      CORNER_HEIGHT_FRONT_LEFT = rhs.CORNER_HEIGHT_FRONT_LEFT;
      CORNER_HEIGHT_FRONT_LEFT_OFFSET = rhs.CORNER_HEIGHT_FRONT_LEFT_OFFSET;
      CORNER_HEIGHT_FRONT_RIGHT = rhs.CORNER_HEIGHT_FRONT_RIGHT;
      CORNER_HEIGHT_FRONT_RIGHT_OFFSET = rhs.CORNER_HEIGHT_FRONT_RIGHT_OFFSET;
      ENTIRE_Z_HEIGHT_OFFSET = rhs.ENTIRE_Z_HEIGHT_OFFSET;
      BACKLASH_X = rhs.BACKLASH_X;
      BACKLASH_Y = rhs.BACKLASH_Y;
      BACKLASH_SPEED = rhs.BACKLASH_SPEED;
      CALIBRATION_OFFSET = rhs.CALIBRATION_OFFSET;
      UsesCalibrationOffset = rhs.UsesCalibrationOffset;
    }

    public Calibration()
    {
      Calibration_Valid = true;
      G32_VERSION = byte.MaxValue;
      CORNER_HEIGHT_BACK_RIGHT = 0.0f;
      CORNER_HEIGHT_BACK_LEFT = 0.0f;
      CORNER_HEIGHT_FRONT_LEFT = 0.0f;
      CORNER_HEIGHT_FRONT_RIGHT = 0.0f;
      CORNER_HEIGHT_BACK_RIGHT_OFFSET = 0.0f;
      CORNER_HEIGHT_BACK_LEFT_OFFSET = 0.0f;
      CORNER_HEIGHT_FRONT_LEFT_OFFSET = 0.0f;
      CORNER_HEIGHT_FRONT_RIGHT_OFFSET = 0.0f;
      ENTIRE_Z_HEIGHT_OFFSET = 0.0f;
      BACKLASH_X = 0.0f;
      BACKLASH_Y = 0.0f;
      BACKLASH_SPEED = 2900f;
      CALIBRATION_OFFSET = 0.0f;
      UsesCalibrationOffset = false;
    }
  }
}
