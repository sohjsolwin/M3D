using System.Xml.Serialization;

namespace M3D.Spooling.Printer_Profiles
{
  public class AccessoriesProfile
  {
    [XmlElement("HeatedBedConstants")]
    public AccessoriesProfile.HeatedBedProfile HeatedBedConstants;
    [XmlElement("SDCardConstants")]
    public AccessoriesProfile.SDCardProfile SDCardConstants;
    [XmlElement("NozzleConstants")]
    public AccessoriesProfile.NozzleProfile NozzleConstants;

    public AccessoriesProfile(bool HasBuiltinHeatedBed, int HeatedBedMinTemp, int HeatedBedMaxTemp, bool HasSDCard, bool bHasInterchangeableNozzle, int iDefaultNozzleSizeMicrons, int iMinimumNozzleSizeMicrons, int iMaximumNozzleSizeMicrons, int iMinimumNozzleWarningSizeMicrons, int iMaximumNozzleWarningSizeMicrons)
    {
      HeatedBedConstants = new AccessoriesProfile.HeatedBedProfile(HasBuiltinHeatedBed, HeatedBedMinTemp, HeatedBedMaxTemp);
      SDCardConstants = new AccessoriesProfile.SDCardProfile(HasSDCard);
      NozzleConstants = new AccessoriesProfile.NozzleProfile(bHasInterchangeableNozzle, iDefaultNozzleSizeMicrons, iMinimumNozzleSizeMicrons, iMaximumNozzleSizeMicrons, iMinimumNozzleWarningSizeMicrons, iMaximumNozzleWarningSizeMicrons);
    }

    public AccessoriesProfile(AccessoriesProfile other)
    {
      HeatedBedConstants = new AccessoriesProfile.HeatedBedProfile(other.HeatedBedConstants);
      SDCardConstants = new AccessoriesProfile.SDCardProfile(other.SDCardConstants);
      NozzleConstants = new AccessoriesProfile.NozzleProfile(other.NozzleConstants);
    }

    public AccessoriesProfile()
    {
      HeatedBedConstants = new AccessoriesProfile.HeatedBedProfile();
      SDCardConstants = new AccessoriesProfile.SDCardProfile();
      NozzleConstants = new AccessoriesProfile.NozzleProfile();
    }

    public class HeatedBedProfile
    {
      [XmlAttribute]
      public bool HasBuiltinHeatedBed;
      [XmlAttribute]
      public int HeatedBedMinTemp;
      [XmlAttribute]
      public int HeatedBedMaxTemp;

      public HeatedBedProfile(bool HasBuiltinHeatedBed, int HeatedBedMinTemp, int HeatedBedMaxTemp)
      {
        this.HasBuiltinHeatedBed = HasBuiltinHeatedBed;
        this.HeatedBedMinTemp = HeatedBedMinTemp;
        this.HeatedBedMaxTemp = HeatedBedMaxTemp;
      }

      public HeatedBedProfile(AccessoriesProfile.HeatedBedProfile other)
      {
        HasBuiltinHeatedBed = other.HasBuiltinHeatedBed;
        HeatedBedMinTemp = other.HeatedBedMinTemp;
        HeatedBedMaxTemp = other.HeatedBedMaxTemp;
      }

      public HeatedBedProfile()
      {
        HasBuiltinHeatedBed = false;
        HeatedBedMinTemp = 0;
        HeatedBedMaxTemp = 0;
      }
    }

    public class SDCardProfile
    {
      [XmlAttribute]
      public bool HasSDCard;

      public SDCardProfile(bool HasSDCard)
      {
        this.HasSDCard = HasSDCard;
      }

      public SDCardProfile(AccessoriesProfile.SDCardProfile other)
      {
        HasSDCard = other.HasSDCard;
      }

      public SDCardProfile()
      {
        HasSDCard = false;
      }
    }

    public class NozzleProfile
    {
      [XmlAttribute]
      public bool bHasInterchangeableNozzle;
      [XmlAttribute]
      public int iDefaultNozzleSizeMicrons;
      [XmlAttribute]
      public int iMinimumNozzleSizeMicrons;
      [XmlAttribute]
      public int iMaximumNozzleSizeMicrons;
      [XmlAttribute]
      public int iMinimumNozzleWarningSizeMicrons;
      [XmlAttribute]
      public int iMaximumNozzleWarningSizeMicrons;

      public NozzleProfile(bool bHasInterchangeableNozzle, int iDefaultNozzleSizeMicrons, int iMinimumNozzleSizeMicrons, int iMaximumNozzleSizeMicrons, int iMinimumNozzleWarningSizeMicrons, int iMaximumNozzleWarningSizeMicrons)
      {
        this.bHasInterchangeableNozzle = bHasInterchangeableNozzle;
        this.iDefaultNozzleSizeMicrons = iDefaultNozzleSizeMicrons;
        this.iMinimumNozzleSizeMicrons = iMinimumNozzleSizeMicrons;
        this.iMaximumNozzleSizeMicrons = iMaximumNozzleSizeMicrons;
        this.iMinimumNozzleWarningSizeMicrons = iMinimumNozzleWarningSizeMicrons;
        this.iMaximumNozzleWarningSizeMicrons = iMaximumNozzleWarningSizeMicrons;
      }

      public NozzleProfile(AccessoriesProfile.NozzleProfile other)
      {
        bHasInterchangeableNozzle = other.bHasInterchangeableNozzle;
        iDefaultNozzleSizeMicrons = other.iDefaultNozzleSizeMicrons;
        iMinimumNozzleSizeMicrons = other.iMinimumNozzleSizeMicrons;
        iMaximumNozzleSizeMicrons = other.iMaximumNozzleSizeMicrons;
        iMinimumNozzleWarningSizeMicrons = other.iMinimumNozzleWarningSizeMicrons;
        iMaximumNozzleWarningSizeMicrons = other.iMaximumNozzleWarningSizeMicrons;
      }

      public NozzleProfile()
      {
        bHasInterchangeableNozzle = false;
        iDefaultNozzleSizeMicrons = 0;
        iMinimumNozzleSizeMicrons = 0;
        iMaximumNozzleSizeMicrons = 0;
        iMinimumNozzleWarningSizeMicrons = 0;
        iMaximumNozzleWarningSizeMicrons = 0;
      }
    }
  }
}
