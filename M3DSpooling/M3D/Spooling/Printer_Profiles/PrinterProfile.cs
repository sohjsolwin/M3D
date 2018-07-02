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
      AccessoriesConstants = new AccessoriesProfile();
      TemperatureConstants = new TemperatureProfile();
      PrinterSizeConstants = new PrinterSizeProfile();
      SpeedLimitConstants = new SpeedLimitProfile();
      SupportedFeaturesConstants = new SupportedFeaturesProfile();
    }

    public PrinterProfile(PrinterProfile other)
    {
      AccessoriesConstants = new AccessoriesProfile(other.AccessoriesConstants);
      TemperatureConstants = new TemperatureProfile(other.TemperatureConstants);
      PrinterSizeConstants = new PrinterSizeProfile(other.PrinterSizeConstants);
      SpeedLimitConstants = new SpeedLimitProfile(other.SpeedLimitConstants);
      SupportedFeaturesConstants = new SupportedFeaturesProfile(other.SupportedFeaturesConstants);
      ProfileName = other.ProfileName;
    }

    [XmlElement("ProfileName")]
    public string ProfileName
    {
      get
      {
        return profilename.Value;
      }
      set
      {
        profilename.Value = value;
      }
    }
  }
}
