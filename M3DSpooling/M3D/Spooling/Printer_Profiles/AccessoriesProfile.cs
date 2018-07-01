// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Printer_Profiles.AccessoriesProfile
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
      this.HeatedBedConstants = new AccessoriesProfile.HeatedBedProfile(HasBuiltinHeatedBed, HeatedBedMinTemp, HeatedBedMaxTemp);
      this.SDCardConstants = new AccessoriesProfile.SDCardProfile(HasSDCard);
      this.NozzleConstants = new AccessoriesProfile.NozzleProfile(bHasInterchangeableNozzle, iDefaultNozzleSizeMicrons, iMinimumNozzleSizeMicrons, iMaximumNozzleSizeMicrons, iMinimumNozzleWarningSizeMicrons, iMaximumNozzleWarningSizeMicrons);
    }

    public AccessoriesProfile(AccessoriesProfile other)
    {
      this.HeatedBedConstants = new AccessoriesProfile.HeatedBedProfile(other.HeatedBedConstants);
      this.SDCardConstants = new AccessoriesProfile.SDCardProfile(other.SDCardConstants);
      this.NozzleConstants = new AccessoriesProfile.NozzleProfile(other.NozzleConstants);
    }

    public AccessoriesProfile()
    {
      this.HeatedBedConstants = new AccessoriesProfile.HeatedBedProfile();
      this.SDCardConstants = new AccessoriesProfile.SDCardProfile();
      this.NozzleConstants = new AccessoriesProfile.NozzleProfile();
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
        this.HasBuiltinHeatedBed = other.HasBuiltinHeatedBed;
        this.HeatedBedMinTemp = other.HeatedBedMinTemp;
        this.HeatedBedMaxTemp = other.HeatedBedMaxTemp;
      }

      public HeatedBedProfile()
      {
        this.HasBuiltinHeatedBed = false;
        this.HeatedBedMinTemp = 0;
        this.HeatedBedMaxTemp = 0;
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
        this.HasSDCard = other.HasSDCard;
      }

      public SDCardProfile()
      {
        this.HasSDCard = false;
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
        this.bHasInterchangeableNozzle = other.bHasInterchangeableNozzle;
        this.iDefaultNozzleSizeMicrons = other.iDefaultNozzleSizeMicrons;
        this.iMinimumNozzleSizeMicrons = other.iMinimumNozzleSizeMicrons;
        this.iMaximumNozzleSizeMicrons = other.iMaximumNozzleSizeMicrons;
        this.iMinimumNozzleWarningSizeMicrons = other.iMinimumNozzleWarningSizeMicrons;
        this.iMaximumNozzleWarningSizeMicrons = other.iMaximumNozzleWarningSizeMicrons;
      }

      public NozzleProfile()
      {
        this.bHasInterchangeableNozzle = false;
        this.iDefaultNozzleSizeMicrons = 0;
        this.iMinimumNozzleSizeMicrons = 0;
        this.iMaximumNozzleSizeMicrons = 0;
        this.iMinimumNozzleWarningSizeMicrons = 0;
        this.iMaximumNozzleWarningSizeMicrons = 0;
      }
    }
  }
}
