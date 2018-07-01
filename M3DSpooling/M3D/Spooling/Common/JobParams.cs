// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.JobParams
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Printer_Profiles;
using System;
using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public class JobParams
  {
    public JobParams.Mode jobMode;
    [XmlAttribute("Gcodefile")]
    public string gcodefile;
    [XmlAttribute("Outputfile")]
    public string outputfile;
    [XmlAttribute("GUID")]
    public string jobGuid;
    [XmlAttribute("JobName")]
    public string jobname;
    [XmlAttribute("EstimatedPrintTime")]
    public float estimatedTime;
    [XmlAttribute("estimatedFilamentNeeded")]
    public float estimatedFilamentNeeded;
    [XmlAttribute("PreviewImageFileName")]
    public string preview_image_file_name;
    [XmlIgnore]
    public FilamentSpool.TypeEnum filament_type;
    [XmlElement]
    public FilamentPreprocessorData preprocessor;
    [XmlElement("options")]
    public JobOptions options;
    [XmlAttribute]
    public int filament_temperature;
    [XmlAttribute("AutoPrint")]
    public bool autoprint;

    public JobParams()
    {
      this.options = new JobOptions();
      this.preprocessor = new FilamentPreprocessorData();
      this.jobGuid = Guid.NewGuid().ToString();
      this.jobMode = JobParams.Mode.DirectPrinting;
      this.outputfile = (string) null;
    }

    public JobParams(JobParams other)
    {
      this.options = new JobOptions(other.options);
      this.preprocessor = new FilamentPreprocessorData(other.preprocessor);
      this.jobGuid = other.jobGuid;
      this.jobMode = other.jobMode;
      this.outputfile = other.outputfile;
      this.autoprint = other.autoprint;
      this.estimatedFilamentNeeded = other.estimatedFilamentNeeded;
      this.estimatedTime = other.estimatedTime;
      this.filament_temperature = other.filament_temperature;
      this.filament_type = other.filament_type;
      this.gcodefile = other.gcodefile;
      this.preview_image_file_name = other.preview_image_file_name;
    }

    public JobParams(string gcodefile, string jobname, string preview_image_file_name, FilamentSpool.TypeEnum filament_type, float estimatedTime, float estimatedFilamentNeeded)
    {
      this.jobMode = JobParams.Mode.DirectPrinting;
      this.gcodefile = gcodefile;
      this.jobname = jobname;
      this.preview_image_file_name = preview_image_file_name;
      this.filament_type = filament_type;
      this.estimatedTime = estimatedTime;
      this.estimatedFilamentNeeded = estimatedFilamentNeeded;
      this.options = new JobOptions();
      this.options.use_raft_DetailOnly = true;
      this.options.wipe_tower = false;
      this.options.ooze_shield = false;
      this.options.autostart_ignorewarnings = false;
      this.options.use_wave_bonding = false;
      this.options.dont_use_preprocessors = false;
      this.options.use_support_DetailOnly = false;
      this.options.calibrate_before_print = false;
      this.options.turn_on_fan_before_print = false;
      this.options.bounds_check_xy = true;
      this.options.quality_layer_resolution_DetailOnly = 0;
      this.options.fill_density_DetailOnly = 0;
      this.filament_temperature = 0;
      this.preprocessor = new FilamentPreprocessorData();
      this.jobGuid = Guid.NewGuid().ToString();
      this.autoprint = false;
    }

    public bool VerifyOptionsWithPrinter(PrinterProfile profile, PrinterInfo printerInfo)
    {
      bool flag1 = true;
      if (this.jobMode == JobParams.Mode.ReconnectToSDPrint)
        throw new NotImplementedException("ReconnectToSDPrint jobs can only be started directly from the FirmwareController.");
      if (this.options.calibrate_before_print && printerInfo.supportedFeatures.UsesSupportedFeatures && !printerInfo.supportedFeatures.Available("Single Point Bed Height Calibration", profile.SupportedFeaturesConstants))
      {
        flag1 = false;
        this.options.calibrate_before_print = false;
      }
      if (this.options.use_heated_bed)
      {
        int num = profile.AccessoriesConstants.HeatedBedConstants.HasBuiltinHeatedBed ? 1 : 0;
        bool flag2 = false;
        if (num != 0 && printerInfo.supportedFeatures.UsesSupportedFeatures)
          flag2 = printerInfo.supportedFeatures.Available("Heated Bed Control", profile.SupportedFeaturesConstants);
        if (num == 0 || !flag2)
        {
          flag1 = false;
          this.options.use_heated_bed = false;
        }
      }
      if (this.jobMode != JobParams.Mode.DirectPrinting && printerInfo.supportedFeatures.UsesSupportedFeatures && !printerInfo.supportedFeatures.Available("Untethered Printing", profile.SupportedFeaturesConstants))
        this.jobMode = JobParams.Mode.DirectPrinting;
      return flag1;
    }

    [XmlAttribute("FilamentType")]
    public string FilamentType
    {
      get
      {
        return this.filament_type.ToString();
      }
      set
      {
        this.filament_type = (FilamentSpool.TypeEnum) Enum.Parse(typeof (FilamentSpool.TypeEnum), value, false);
      }
    }

    public enum Mode
    {
      NotPrinting,
      DirectPrinting,
      SavingToSDCard,
      SavingToSDCardAutoStartPrint,
      FirmwarePrintingFromSDCard,
      ReconnectToSDPrint,
      SaveToBinaryGCodeFile,
    }
  }
}
