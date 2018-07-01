// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.PrinterInfo
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
  public class PrinterInfo
  {
    private static XmlWriterSettings settings = new XmlWriterSettings();
    private static XmlSerializer __class_serializer = (XmlSerializer) null;
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
        return this.Status >= PrinterStatus.Bootloader_Ready;
      }
    }

    [XmlIgnore]
    public bool InFirmwareMode
    {
      get
      {
        if (this.Status >= PrinterStatus.Firmware_Ready)
          return this.Status < PrinterStatus.Bootloader_Ready;
        return false;
      }
    }

    [XmlIgnore]
    public bool FirmwareIsInvalid
    {
      get
      {
        if (this.Status != PrinterStatus.Bootloader_InvalidFirmware)
          return this.Status == PrinterStatus.Bootloader_FirmwareUpdateFailed;
        return true;
      }
    }

    [XmlIgnore]
    public bool HasFilament
    {
      get
      {
        return (uint) this.filament_info.filament_type > 0U;
      }
    }

    [XmlIgnore]
    public bool IsIdle
    {
      get
      {
        if (this.Status != PrinterStatus.Firmware_Idle && this.Status != PrinterStatus.Firmware_PrintingPaused)
          return this.Status == PrinterStatus.Bootloader_Ready;
        return true;
      }
    }

    public PrinterInfo(PrinterInfo other)
    {
      this.CopyFrom(other);
    }

    public PrinterInfo(string serialization)
      : this()
    {
      this.Deserialize(serialization);
    }

    public PrinterInfo()
    {
      this.current_job = (JobInfo) null;
      this.serial_number = new PrinterSerialNumber("0");
      this.filament_info = new FilamentSpool();
      this.extruder = new Extruder();
      this.hardware = new Hardware();
      this.calibration = new Calibration();
      this.synchronization = new Synchronization();
      this.statistics = new Statistics();
      this.supportedFeatures = new SupportedFeatures();
      this.accessories = new Accessories();
      this.persistantData = new PersistantData();
      this.powerRecovery = new PowerRecovery();
    }

    public void Deserialize(string serialization)
    {
      using (TextReader textReader = (TextReader) new StringReader(serialization))
      {
        try
        {
          this.CopyFrom((PrinterInfo) PrinterInfo.ClassSerializer.Deserialize(textReader));
        }
        catch (Exception ex)
        {
          ErrorLogger.LogErrorMsg("Loading XML Exception: " + ex.Message + (ex.InnerException != null ? "\nInner Exception: " + ex.InnerException.Message : ""));
          this.CopyFrom(new PrinterInfo());
        }
      }
    }

    public string Serialize()
    {
      PrinterInfo.settings.OmitXmlDeclaration = true;
      StringWriter stringWriter = new StringWriter();
      XmlWriter xmlWriter = XmlWriter.Create((TextWriter) stringWriter, PrinterInfo.settings);
      this.ns.Add("", "");
      try
      {
        PrinterInfo.ClassSerializer.Serialize(xmlWriter, (object) this, this.ns);
      }
      catch (Exception ex)
      {
        if (Debugger.IsAttached)
          Debugger.Break();
        throw;
      }
      return stringWriter.ToString();
    }

    public void CopyFrom(PrinterInfo other)
    {
      this.serial_number = new PrinterSerialNumber(other.serial_number.ToString());
      this.Status = other.Status;
      this.filament_info = new FilamentSpool(other.filament_info);
      this.current_job = other.current_job == null ? (JobInfo) null : new JobInfo(other.current_job);
      this.extruder = new Extruder(other.extruder);
      this.hardware = new Hardware(other.hardware);
      this.calibration = new Calibration(other.calibration);
      this.synchronization = new Synchronization(other.synchronization);
      this.supportedFeatures = new SupportedFeatures(other.supportedFeatures);
      this.accessories = new Accessories(other.accessories);
      this.statistics = new Statistics(other.statistics);
      this.persistantData = new PersistantData(other.persistantData);
      this.powerRecovery = new PowerRecovery(other.powerRecovery);
      this.ProfileName = other.ProfileName;
    }

    [XmlAttribute("Serialnumber")]
    public string MySerialNumber
    {
      get
      {
        return this.serial_number.ToString();
      }
      set
      {
        this.serial_number = new PrinterSerialNumber(value);
      }
    }

    [XmlAttribute("PrinterStatus")]
    public string XMLPrinterStatus
    {
      get
      {
        return this.Status.ToString();
      }
      set
      {
        this.Status = (PrinterStatus) Enum.Parse(typeof (PrinterStatus), value, false);
      }
    }

    private static XmlSerializer ClassSerializer
    {
      get
      {
        if (PrinterInfo.__class_serializer == null)
          PrinterInfo.__class_serializer = new XmlSerializer(typeof (PrinterInfo));
        return PrinterInfo.__class_serializer;
      }
    }
  }
}
