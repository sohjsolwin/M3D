using M3D.Spooling.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public class PrinterInfo
  {
    private static XmlWriterSettings settings = new XmlWriterSettings();
    private static XmlSerializer __class_serializer = null;
    private XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
    [XmlElement("FilamentInfo")]
    public FilamentSpool filament_info;
    [XmlElement("Accessories")]
    public Accessories accessories;
    [XmlElement("Extruder")]
    public Extruder extruder;
    [XmlElement("Hardware")]
    public Hardware hardware;
    [XmlElement("Calibration")]
    public Calibration calibration;
    [XmlElement("CurrentJob")]
    public JobInfo current_job;
    [XmlElement("Synchronization")]
    public Synchronization synchronization;
    [XmlElement("Stats")]
    public Statistics statistics;
    [XmlElement("SupportedFeatures")]
    public SupportedFeatures supportedFeatures;
    [XmlElement("PersistantData")]
    public PersistantData persistantData;
    [XmlElement("PowerRecovery")]
    public PowerRecovery powerRecovery;
    [XmlAttribute("ProfileName")]
    public string ProfileName;
    [XmlIgnore]
    public PrinterSerialNumber serial_number;
    [XmlIgnore]
    public PrinterStatus Status;

    [XmlIgnore]
    public bool InBootloaderMode
    {
      get
      {
        return Status >= PrinterStatus.Bootloader_Ready;
      }
    }

    [XmlIgnore]
    public bool InFirmwareMode
    {
      get
      {
        if (Status >= PrinterStatus.Firmware_Ready)
        {
          return Status < PrinterStatus.Bootloader_Ready;
        }

        return false;
      }
    }

    [XmlIgnore]
    public bool FirmwareIsInvalid
    {
      get
      {
        if (Status != PrinterStatus.Bootloader_InvalidFirmware)
        {
          return Status == PrinterStatus.Bootloader_FirmwareUpdateFailed;
        }

        return true;
      }
    }

    [XmlIgnore]
    public bool HasFilament
    {
      get
      {
        return (uint)filament_info.filament_type > 0U;
      }
    }

    [XmlIgnore]
    public bool IsIdle
    {
      get
      {
        if (Status != PrinterStatus.Firmware_Idle && Status != PrinterStatus.Firmware_PrintingPaused)
        {
          return Status == PrinterStatus.Bootloader_Ready;
        }

        return true;
      }
    }

    public PrinterInfo(PrinterInfo other)
    {
      CopyFrom(other);
    }

    public PrinterInfo(string serialization)
      : this()
    {
      Deserialize(serialization);
    }

    public PrinterInfo()
    {
      current_job = null;
      serial_number = new PrinterSerialNumber("0");
      filament_info = new FilamentSpool();
      extruder = new Extruder();
      hardware = new Hardware();
      calibration = new Calibration();
      synchronization = new Synchronization();
      statistics = new Statistics();
      supportedFeatures = new SupportedFeatures();
      accessories = new Accessories();
      persistantData = new PersistantData();
      powerRecovery = new PowerRecovery();
    }

    public void Deserialize(string serialization)
    {
      using (var textReader = (TextReader) new StringReader(serialization))
      {
        try
        {
          CopyFrom((PrinterInfo) PrinterInfo.ClassSerializer.Deserialize(textReader));
        }
        catch (Exception ex)
        {
          ErrorLogger.LogErrorMsg("Loading XML Exception: " + ex.Message + (ex.InnerException != null ? "\nInner Exception: " + ex.InnerException.Message : ""));
          CopyFrom(new PrinterInfo());
        }
      }
    }

    public string Serialize()
    {
      PrinterInfo.settings.OmitXmlDeclaration = true;
      var stringWriter = new StringWriter();
      var xmlWriter = XmlWriter.Create(stringWriter, PrinterInfo.settings);
      ns.Add("", "");
      try
      {
        PrinterInfo.ClassSerializer.Serialize(xmlWriter, this, ns);
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

    public void CopyFrom(PrinterInfo other)
    {
      serial_number = new PrinterSerialNumber(other.serial_number.ToString());
      Status = other.Status;
      filament_info = new FilamentSpool(other.filament_info);
      current_job = other.current_job == null ? null : new JobInfo(other.current_job);
      extruder = new Extruder(other.extruder);
      hardware = new Hardware(other.hardware);
      calibration = new Calibration(other.calibration);
      synchronization = new Synchronization(other.synchronization);
      supportedFeatures = new SupportedFeatures(other.supportedFeatures);
      accessories = new Accessories(other.accessories);
      statistics = new Statistics(other.statistics);
      persistantData = new PersistantData(other.persistantData);
      powerRecovery = new PowerRecovery(other.powerRecovery);
      ProfileName = other.ProfileName;
    }

    [XmlAttribute("Serialnumber")]
    public string MySerialNumber
    {
      get
      {
        return serial_number.ToString();
      }
      set
      {
        serial_number = new PrinterSerialNumber(value);
      }
    }

    [XmlAttribute("PrinterStatus")]
    public string XMLPrinterStatus
    {
      get
      {
        return Status.ToString();
      }
      set
      {
        Status = (PrinterStatus) Enum.Parse(typeof (PrinterStatus), value, false);
      }
    }

    private static XmlSerializer ClassSerializer
    {
      get
      {
        if (PrinterInfo.__class_serializer == null)
        {
          PrinterInfo.__class_serializer = new XmlSerializer(typeof (PrinterInfo));
        }

        return PrinterInfo.__class_serializer;
      }
    }
  }
}
