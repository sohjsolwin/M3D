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
      options = new JobOptions();
      preprocessor = new FilamentPreprocessorData();
      jobGuid = Guid.NewGuid().ToString();
      jobMode = JobParams.Mode.DirectPrinting;
      outputfile = (string) null;
    }

    public JobParams(JobParams other)
    {
      options = new JobOptions(other.options);
      preprocessor = new FilamentPreprocessorData(other.preprocessor);
      jobGuid = other.jobGuid;
      jobMode = other.jobMode;
      outputfile = other.outputfile;
      autoprint = other.autoprint;
      estimatedFilamentNeeded = other.estimatedFilamentNeeded;
      estimatedTime = other.estimatedTime;
      filament_temperature = other.filament_temperature;
      filament_type = other.filament_type;
      gcodefile = other.gcodefile;
      preview_image_file_name = other.preview_image_file_name;
    }

    public JobParams(string gcodefile, string jobname, string preview_image_file_name, FilamentSpool.TypeEnum filament_type, float estimatedTime, float estimatedFilamentNeeded)
    {
      jobMode = JobParams.Mode.DirectPrinting;
      this.gcodefile = gcodefile;
      this.jobname = jobname;
      this.preview_image_file_name = preview_image_file_name;
      this.filament_type = filament_type;
      this.estimatedTime = estimatedTime;
      this.estimatedFilamentNeeded = estimatedFilamentNeeded;
      options = new JobOptions
      {
        use_raft_DetailOnly = true,
        wipe_tower = false,
        ooze_shield = false,
        autostart_ignorewarnings = false,
        use_wave_bonding = false,
        dont_use_preprocessors = false,
        use_support_DetailOnly = false,
        calibrate_before_print = false,
        turn_on_fan_before_print = false,
        bounds_check_xy = true,
        quality_layer_resolution_DetailOnly = 0,
        fill_density_DetailOnly = 0
      };
      filament_temperature = 0;
      preprocessor = new FilamentPreprocessorData();
      jobGuid = Guid.NewGuid().ToString();
      autoprint = false;
    }

    public bool VerifyOptionsWithPrinter(PrinterProfile profile, PrinterInfo printerInfo)
    {
      var flag1 = true;
      if (jobMode == JobParams.Mode.ReconnectToSDPrint)
      {
        throw new NotImplementedException("ReconnectToSDPrint jobs can only be started directly from the FirmwareController.");
      }

      if (options.calibrate_before_print && printerInfo.supportedFeatures.UsesSupportedFeatures && !printerInfo.supportedFeatures.Available("Single Point Bed Height Calibration", profile.SupportedFeaturesConstants))
      {
        flag1 = false;
        options.calibrate_before_print = false;
      }
      if (options.use_heated_bed)
      {
        var num = profile.AccessoriesConstants.HeatedBedConstants.HasBuiltinHeatedBed ? 1 : 0;
        var flag2 = false;
        if (num != 0 && printerInfo.supportedFeatures.UsesSupportedFeatures)
        {
          flag2 = printerInfo.supportedFeatures.Available("Heated Bed Control", profile.SupportedFeaturesConstants);
        }

        if (num == 0 || !flag2)
        {
          flag1 = false;
          options.use_heated_bed = false;
        }
      }
      if (jobMode != JobParams.Mode.DirectPrinting && printerInfo.supportedFeatures.UsesSupportedFeatures && !printerInfo.supportedFeatures.Available("Untethered Printing", profile.SupportedFeaturesConstants))
      {
        jobMode = JobParams.Mode.DirectPrinting;
      }

      return flag1;
    }

    [XmlAttribute("FilamentType")]
    public string FilamentType
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
