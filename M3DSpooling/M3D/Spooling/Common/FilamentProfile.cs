using M3D.Spooling.Printer_Profiles;
using System;

namespace M3D.Spooling.Common
{
  public abstract class FilamentProfile
  {
    public FilamentPreprocessorData preprocessor = new FilamentPreprocessorData();
    public SlicerProfile slicerProfile = new SlicerProfile();
    protected FilamentSpool filament;

    public FilamentProfile(FilamentSpool spool)
    {
      filament = spool;
      preprocessor.initialPrint.BedTemperature = FilamentConstants.Temperature.BedDefault(spool.filament_type);
    }

    public abstract bool TestSizeWarning(float minX, float maxX, float minY, float maxY, float minZ, float maxZ);

    public static FilamentProfile CreateFilamentProfile(FilamentSpool spool, PrinterProfile printer_profile)
    {
      switch (spool.filament_type)
      {
        case FilamentSpool.TypeEnum.ABS:
        case FilamentSpool.TypeEnum.HIPS:
          return (FilamentProfile) new ABS_FilamentProfile(spool, printer_profile);
        case FilamentSpool.TypeEnum.PLA:
        case FilamentSpool.TypeEnum.CAM:
          return (FilamentProfile) new PLA_FilamentProfile(spool, printer_profile);
        case FilamentSpool.TypeEnum.FLX:
        case FilamentSpool.TypeEnum.TGH:
          return (FilamentProfile) new TGH_FilamentProfile(spool, printer_profile);
        case FilamentSpool.TypeEnum.ABS_R:
          return (FilamentProfile) new ABS_R_FilamentProfile(spool, printer_profile);
        default:
          throw new ArgumentException("FilamentProfile.CreateFilamentProfile does not know that type :(");
      }
    }

    public string Name
    {
      get
      {
        return Type.ToString() + " " + Color.ToString();
      }
    }

    public FilamentSpool.TypeEnum Type
    {
      get
      {
        return filament.filament_type;
      }
    }

    public FilamentConstants.ColorsEnum Color
    {
      get
      {
        return (FilamentConstants.ColorsEnum) Enum.ToObject(typeof (FilamentConstants.ColorsEnum), filament.filament_color_code);
      }
    }

    public int Temperature
    {
      get
      {
        return filament.filament_temperature;
      }
    }

    public FilamentSpool Spool
    {
      get
      {
        return filament;
      }
    }

    public abstract string ShortName { get; }

    public static string GenerateSpoolName(FilamentSpool spool, bool location)
    {
      var color = (FilamentConstants.ColorsEnum) Enum.ToObject(typeof (FilamentConstants.ColorsEnum), spool.filament_color_code);
      if (location)
      {
        return "My " + FilamentConstants.ColorsToString(color) + " Filament (" + spool.filament_type.ToString() + ") " + spool.filament_location.ToString();
      }

      return "My " + FilamentConstants.ColorsToString(color) + " Filament (" + spool.filament_type.ToString() + ") ";
    }

    public override string ToString()
    {
      return Name;
    }

    public struct TypeColorKey
    {
      public FilamentSpool.TypeEnum type;
      public FilamentConstants.ColorsEnum color;

      public TypeColorKey(FilamentProfile.TypeColorKey other)
      {
        type = other.type;
        color = other.color;
      }

      public TypeColorKey(FilamentSpool.TypeEnum type, FilamentConstants.ColorsEnum color)
      {
        this.type = type;
        this.color = color;
      }

      public override int GetHashCode()
      {
        return 13 * (13 * 27 + type.GetHashCode()) + color.GetHashCode();
      }

      public override bool Equals(object b)
      {
        if (b is FilamentProfile.TypeColorKey)
        {
          return Equals((FilamentProfile.TypeColorKey) b);
        }

        return false;
      }

      public bool Equals(FilamentProfile.TypeColorKey b)
      {
        if ((ValueType) b == null || type != b.type)
        {
          return false;
        }

        return color == b.color;
      }

      public static bool operator ==(FilamentProfile.TypeColorKey a, FilamentProfile.TypeColorKey b)
      {
        if ((ValueType) a == (ValueType) b)
        {
          return true;
        }

        if ((ValueType) a == null || (ValueType) b == null || a.type != b.type)
        {
          return false;
        }

        return a.color == b.color;
      }

      public static bool operator !=(FilamentProfile.TypeColorKey a, FilamentProfile.TypeColorKey b)
      {
        return !(a == b);
      }
    }

    public struct CustomOptions
    {
      public int temperature;
    }
  }
}
