// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.FilamentSpool
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public class FilamentSpool
  {
    private static XmlWriterSettings settings = new XmlWriterSettings();
    private static XmlSerializer __class_serializer = (XmlSerializer) null;
    private XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
    public const uint UndefinedFilamentUID = 0;
    private const float MICRO_SPOOL_LENGTH = 76200f;
    private const float PRO_SPOOL_LENGTH = 152400f;
    [XmlAttribute("ColorCode")]
    public uint filament_color_code;
    [XmlIgnore]
    public FilamentSpool.TypeEnum filament_type;
    [XmlIgnore]
    public FilamentSpool.Location filament_location;
    [XmlAttribute("Temperature")]
    public int filament_temperature;
    [XmlAttribute("EstimatedLength")]
    public float estimated_filament_length_printed;
    [XmlAttribute("Size")]
    public FilamentSpool.SizeEnum filament_size;
    [XmlAttribute("UID")]
    public uint filament_uid;

    public FilamentSpool(FilamentSpool other)
    {
      CopyFrom(other);
    }

    public FilamentSpool()
    {
      filament_color_code = FilamentSpool.DefaultColorCode;
      filament_type = FilamentSpool.TypeEnum.NoFilament;
      filament_location = FilamentSpool.Location.External;
      filament_temperature = 200;
      estimated_filament_length_printed = 0.0f;
      filament_size = FilamentSpool.SizeEnum.Micro;
      filament_uid = 0U;
    }

    public FilamentSpool(FilamentSpool.TypeEnum type, int temperature)
    {
      filament_color_code = FilamentSpool.DefaultColorCode;
      filament_type = type;
      filament_location = FilamentSpool.Location.External;
      filament_temperature = temperature;
      estimated_filament_length_printed = 0.0f;
      filament_size = FilamentSpool.SizeEnum.Micro;
      filament_uid = 0U;
    }

    [XmlAttribute("Type")]
    public string XMLType
    {
      get
      {
        return filament_type.ToString();
      }
      set
      {
        filament_type = (FilamentSpool.TypeEnum) Enum.Parse(typeof (FilamentSpool.TypeEnum), value, false);
      }
    }

    [XmlAttribute("Location")]
    public string XMLLocation
    {
      get
      {
        return filament_location.ToString();
      }
      set
      {
        filament_location = (FilamentSpool.Location) Enum.Parse(typeof (FilamentSpool.Location), value, false);
      }
    }

    public void Deserialize(string serialization)
    {
      using (var textReader = (TextReader) new StringReader(serialization))
      {
        try
        {
          CopyFrom((FilamentSpool) FilamentSpool.ClassSerializer.Deserialize(textReader));
        }
        catch (Exception ex)
        {
          ErrorLogger.LogErrorMsg("Loading XML Exception: " + ex.Message + (ex.InnerException != null ? "\nInner Exception: " + ex.InnerException.Message : ""));
          CopyFrom(new FilamentSpool());
        }
      }
    }

    public string Serialize()
    {
      FilamentSpool.settings.OmitXmlDeclaration = true;
      var stringWriter = new StringWriter();
      var xmlWriter = XmlWriter.Create((TextWriter) stringWriter, FilamentSpool.settings);
      ns.Add("", "");
      try
      {
        FilamentSpool.ClassSerializer.Serialize(xmlWriter, (object) this, ns);
      }
      catch (Exception ex)
      {
        if (Debugger.IsAttached)
        {
          Debugger.Break();
        }

        throw;
      }
      return stringWriter.ToString();
    }

    public void CopyFrom(FilamentSpool other)
    {
      filament_color_code = other.filament_color_code;
      filament_type = other.filament_type;
      filament_temperature = other.filament_temperature;
      filament_location = other.filament_location;
      estimated_filament_length_printed = other.estimated_filament_length_printed;
      filament_size = other.filament_size;
      filament_uid = other.filament_uid;
    }

    public float GetMaxFilamentBySpoolSize()
    {
      switch (filament_size)
      {
        case FilamentSpool.SizeEnum.Micro:
          return 76200f;
        case FilamentSpool.SizeEnum.Pro:
          return 152400f;
        default:
          return 76200f;
      }
    }

    public static uint DefaultColorCode
    {
      get
      {
        return 0;
      }
    }

    private static XmlSerializer ClassSerializer
    {
      get
      {
        if (FilamentSpool.__class_serializer == null)
        {
          FilamentSpool.__class_serializer = new XmlSerializer(typeof (FilamentSpool));
        }

        return FilamentSpool.__class_serializer;
      }
    }

    public override bool Equals(object obj)
    {
      if (obj == null)
      {
        return false;
      }

      var b = obj as FilamentSpool;
      if ((object) b == null)
      {
        return false;
      }

      return Equals(b);
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public bool Equals(FilamentSpool b)
    {
      if ((object) b == null || filament_type != b.filament_type || ((int)filament_color_code != (int) b.filament_color_code || filament_temperature != b.filament_temperature) || (filament_location != b.filament_location || filament_size != b.filament_size || (double)estimated_filament_length_printed != (double) b.estimated_filament_length_printed))
      {
        return false;
      }

      return (int)filament_uid == (int) b.filament_uid;
    }

    public static bool operator ==(FilamentSpool a, FilamentSpool b)
    {
      if ((object) a == (object) b)
      {
        return true;
      }

      if ((object) a == null || (object) b == null)
      {
        return false;
      }

      return a.Equals(b);
    }

    public static bool operator !=(FilamentSpool a, FilamentSpool b)
    {
      return !(a == b);
    }

    public bool Matches(FilamentSpool other)
    {
      if (filament_type == other.filament_type && (int)filament_color_code == (int) other.filament_color_code)
      {
        return filament_size == other.filament_size;
      }

      return false;
    }

    public static uint GenerateUID()
    {
      DateTime now = DateTime.Now;
      var num = (uint) (now.Year % 100 - 16) % 64U;
      return (uint) (now.Second + (now.Minute << 6) + (now.Hour << 12) + (now.Day << 17) + (now.Month << 22) + ((int) num << 26));
    }

    public enum TypeEnum
    {
      NoFilament,
      ABS,
      PLA,
      HIPS,
      OtherOrUnknown,
      FLX,
      TGH,
      CAM,
      ABS_R,
    }

    public enum Location
    {
      External,
      Internal,
    }

    public enum SizeEnum
    {
      Micro,
      Pro,
    }
  }
}
