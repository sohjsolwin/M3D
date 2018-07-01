// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Printer_Profiles.PrinterProfile
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Common.Utils;
using System.Xml.Serialization;

namespace M3D.Spooling.Printer_Profiles
{
  public class PrinterProfile
  {
    [XmlIgnore]
    private WriteOnce<string> profilename = new WriteOnce<string>();
    [XmlElement("AccessoriesConstants")]
    public AccessoriesProfile AccessoriesConstants;
    [XmlElement("PrinterSizeConstants")]
    public PrinterSizeProfile PrinterSizeConstants;
    [XmlElement("TemperatureConstants")]
    public TemperatureProfile TemperatureConstants;
    [XmlElement("SpeedLimitConstants")]
    public SpeedLimitProfile SpeedLimitConstants;
    [XmlElement("SupportedFeaturesConstants")]
    public SupportedFeaturesProfile SupportedFeaturesConstants;
    public const string StylizedModelNameMicro = "Micro";
    public const string StylizedModelNamePro = "Pro";
    public const string StylizedModelNameMicroPlus = "Micro+";

    public PrinterProfile()
    {
      this.AccessoriesConstants = new AccessoriesProfile();
      this.TemperatureConstants = new TemperatureProfile();
      this.PrinterSizeConstants = new PrinterSizeProfile();
      this.SpeedLimitConstants = new SpeedLimitProfile();
      this.SupportedFeaturesConstants = new SupportedFeaturesProfile();
    }

    public PrinterProfile(PrinterProfile other)
    {
      this.AccessoriesConstants = new AccessoriesProfile(other.AccessoriesConstants);
      this.TemperatureConstants = new TemperatureProfile(other.TemperatureConstants);
      this.PrinterSizeConstants = new PrinterSizeProfile(other.PrinterSizeConstants);
      this.SpeedLimitConstants = new SpeedLimitProfile(other.SpeedLimitConstants);
      this.SupportedFeaturesConstants = new SupportedFeaturesProfile(other.SupportedFeaturesConstants);
      this.ProfileName = other.ProfileName;
    }

    [XmlElement("ProfileName")]
    public string ProfileName
    {
      get
      {
        return this.profilename.Value;
      }
      set
      {
        this.profilename.Value = value;
      }
    }
  }
}
