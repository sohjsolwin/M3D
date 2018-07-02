using M3D.Slicer.General;
using M3D.Spooling;
using M3D.Spooling.Common;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace M3D.GUI.Controller.Settings
{
  public class PrintDetails
  {
    public struct M3DSettings
    {
      [XmlElement("Version")]
      public string version;
      [XmlElement("SerialNumber")]
      public string serialNumber;
      [XmlElement("PrintSettings")]
      public PrintDetails.PrintSettings printSettings;
      [XmlElement("SlicerProfileName")]
      public string profileName;
      [XmlIgnore]
      public string jobGuid;
      private static XmlSerializer __class_serializer;

      public M3DSettings(JobParams jobParams, PrinterObject printer, string profileName, List<Slicer.General.KeyValuePair<string, string>> curaSettings)
      {
        printSettings.filament = printer == null ? new PrintDetails.Filament((FilamentProfile) null) : new PrintDetails.Filament(printer.MyFilamentProfile);
        serialNumber = printer == null ? PrinterSerialNumber.Undefined.ToString() : printer.Info.MySerialNumber;
        printSettings.options = jobParams.options;
        printSettings.CuraSettings = curaSettings;
        version = Version.VersionTextNoDate;
        this.profileName = profileName;
        jobGuid = "";
      }

      public static XmlSerializer ClassSerializer
      {
        get
        {
          if (PrintDetails.M3DSettings.__class_serializer == null)
          {
            PrintDetails.M3DSettings.__class_serializer = new XmlSerializer(typeof (PrintDetails.M3DSettings));
          }

          return PrintDetails.M3DSettings.__class_serializer;
        }
      }
    }

    public class PrintJobObjectViewDetails
    {
      [XmlElement("Object", Type = typeof (PrintDetails.ObjectDetails))]
      public List<PrintDetails.ObjectDetails> objectList;
      private static XmlSerializer __class_serializer;

      public PrintJobObjectViewDetails()
      {
        objectList = new List<PrintDetails.ObjectDetails>();
      }

      public PrintJobObjectViewDetails(List<PrintDetails.ObjectDetails> objectList)
      {
        this.objectList = new List<PrintDetails.ObjectDetails>((IEnumerable<PrintDetails.ObjectDetails>) objectList);
      }

      public static XmlSerializer ClassSerializer
      {
        get
        {
          if (PrintDetails.PrintJobObjectViewDetails.__class_serializer == null)
          {
            PrintDetails.PrintJobObjectViewDetails.__class_serializer = new XmlSerializer(typeof (PrintDetails.PrintJobObjectViewDetails));
          }

          return PrintDetails.PrintJobObjectViewDetails.__class_serializer;
        }
      }
    }

    public struct PrintSettings
    {
      [XmlElement("Filament")]
      public PrintDetails.Filament filament;
      [XmlElement("JobOptions")]
      public JobOptions options;
      [XmlElement("CuraSettings")]
      public List<Slicer.General.KeyValuePair<string, string>> CuraSettings;
    }

    public struct Filament
    {
      [XmlAttribute("Type")]
      public string Type;
      [XmlAttribute("Color")]
      public string Color;
      [XmlAttribute("Temperature")]
      public int Temperature;

      public Filament(FilamentProfile profile)
      {
        if (profile != null)
        {
          Color = FilamentConstants.ColorsToString(profile.Color);
          Temperature = profile.Temperature;
          Type = profile.Type.ToString();
        }
        else
        {
          Color = "";
          Temperature = 0;
          Type = "";
        }
      }
    }

    public class ObjectDetails
    {
      [XmlAttribute("filename")]
      public string filename;
      [XmlElement("transform")]
      public PrintDetails.Transform transform;
      [XmlElement("hidecontrols")]
      public bool hidecontrols;
      [XmlIgnore]
      public string printerViewXMLFile;
      [XmlIgnore]
      public string printerSettingsXMLFile;
      [XmlIgnore]
      public string zipFileName;
      [XmlIgnore]
      public uint UID;

      public ObjectDetails()
      {
        UID = uint.MaxValue;
      }

      public ObjectDetails(PrintDetails.ObjectDetails other)
        : this()
      {
        filename = other.filename;
        printerViewXMLFile = other.printerViewXMLFile;
        printerSettingsXMLFile = other.printerSettingsXMLFile;
        hidecontrols = other.hidecontrols;
        if (other.transform == null)
        {
          return;
        }

        transform = new PrintDetails.Transform(other.transform);
      }

      public ObjectDetails(string filename)
        : this(filename, (PrintDetails.Transform) null)
      {
      }

      public ObjectDetails(string filename, PrintDetails.Transform transform)
        : this()
      {
        this.filename = filename;
        this.transform = transform;
      }

      public ObjectDetails(string filename, string printerViewXMLFile, string printerSettingsXMLFile, string zipFileName)
        : this()
      {
        this.filename = filename;
        this.printerViewXMLFile = printerViewXMLFile;
        this.printerSettingsXMLFile = printerSettingsXMLFile;
        this.zipFileName = zipFileName;
      }
    }

    public class Transform
    {
      [XmlElement("translation")]
      public PrintDetails.Vector2 translation;
      [XmlElement("scale")]
      public PrintDetails.Vector3 scale;
      [XmlElement("rotation")]
      public PrintDetails.Vector3 rotation;

      public Transform()
      {
      }

      public Transform(PrintDetails.Transform other)
      {
        translation = other.translation;
        scale = other.scale;
        rotation = other.rotation;
      }

      public Transform(M3D.Model.Utils.Vector3 translation, M3D.Model.Utils.Vector3 scale, M3D.Model.Utils.Vector3 rotation)
      {
        this.translation.x = translation.x;
        this.translation.y = translation.y;
        this.scale.x = scale.x;
        this.scale.y = scale.y;
        this.scale.z = scale.z;
        this.rotation.x = rotation.x;
        this.rotation.y = rotation.y;
        this.rotation.z = rotation.z;
      }

      public Transform(PrintDetails.Vector2 translation, PrintDetails.Vector3 scale, PrintDetails.Vector3 rotation)
      {
        this.translation.x = translation.x;
        this.translation.y = translation.y;
        this.scale.x = scale.x;
        this.scale.y = scale.y;
        this.scale.z = scale.z;
        this.rotation.x = rotation.x;
        this.rotation.y = rotation.y;
        this.rotation.z = rotation.z;
      }
    }

    public struct Vector2
    {
      [XmlAttribute("X")]
      public float x;
      [XmlAttribute("Y")]
      public float y;

      public M3D.Model.Utils.Vector3 GUIVector()
      {
        return new M3D.Model.Utils.Vector3(x, y, 0.0f);
      }
    }

    public struct Vector3
    {
      [XmlAttribute("X")]
      public float x;
      [XmlAttribute("Y")]
      public float y;
      [XmlAttribute("Z")]
      public float z;

      public M3D.Model.Utils.Vector3 GUIVector()
      {
        return new M3D.Model.Utils.Vector3(x, y, z);
      }
    }
  }
}
