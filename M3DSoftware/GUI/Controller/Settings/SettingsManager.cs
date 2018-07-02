using M3D.Graphics.Ext3D.ModelRendering;
using M3D.GUI.Interfaces;
using M3D.Slicer.General;
using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;
using M3D.Spooling.Printer_Profiles;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace M3D.GUI.Controller.Settings
{
  public class SettingsManager
  {
    private FilamentDictionary filamentDictionary = new FilamentDictionary();
    private IFileAssociations fileAssociations;
    private SettingsManager.M3DSettings m3dsettings;

    public SettingsManager(IFileAssociations fileAssociations)
    {
      this.fileAssociations = fileAssociations;
      LoadSettings();
    }

    public SettingsManager.M3DSettings Settings
    {
      get
      {
        return m3dsettings;
      }
    }

    public SettingsManager.AppearanceSettings CurrentAppearanceSettings
    {
      get
      {
        return Settings.appearanceSettings;
      }
    }

    public SettingsManager.MiscSettings CurrentMiscSettings
    {
      get
      {
        return Settings.miscSettings;
      }
    }

    public SettingsManager.FilamentSettings CurrentFilamentSettings
    {
      get
      {
        return Settings.filamentSettings;
      }
    }

    public IFileAssociations FileAssociations
    {
      get
      {
        return fileAssociations;
      }
    }

    public bool ShowAllWarnings
    {
      get
      {
        return CurrentAppearanceSettings.ShowAllWarnings;
      }
      set
      {
        CurrentAppearanceSettings.ShowAllWarnings = value;
      }
    }

    public FilamentDictionary FilamentDictionary
    {
      get
      {
        return filamentDictionary;
      }
    }

    public void LoadSettings()
    {
      try
      {
        var textReader = (TextReader) new StreamReader(M3D.GUI.Paths.SettingsPath);
        m3dsettings = (SettingsManager.M3DSettings) SettingsManager.M3DSettings.ClassSerializer.Deserialize(textReader);
        textReader.Close();
        Settings.customFilamentProfiles.PushDataToDictionary(FilamentDictionary);
      }
      catch (Exception ex)
      {
        m3dsettings = new SettingsManager.M3DSettings();
        SaveSettings();
      }
    }

    public void SaveSettings()
    {
      Settings.customFilamentProfiles.PullDataFromDictionary(FilamentDictionary);
      try
      {
        var textWriter = (TextWriter) new StreamWriter(M3D.GUI.Paths.SettingsPath);
        var namespaces = new XmlSerializerNamespaces();
        namespaces.Add(string.Empty, string.Empty);
        SettingsManager.M3DSettings.ClassSerializer.Serialize(textWriter, Settings, namespaces);
        textWriter.Close();
      }
      catch (Exception ex)
      {
      }
    }

    public int GetFilamentTemperature(FilamentSpool.TypeEnum type, FilamentConstants.ColorsEnum colors)
    {
      foreach (var customValue in filamentDictionary.CustomValues)
      {
        if (customValue.Key.type == type && customValue.Key.color == colors)
        {
          return customValue.Value.temperature;
        }
      }
      return FilamentConstants.Temperature.Default(type);
    }

    public FilamentSpool FindMatchingUsedSpool(FilamentSpool spool)
    {
      return CurrentFilamentSettings.usedFilamentSpools.filament.Find(x => x.Matches(spool));
    }

    public void AssociateFilamentToPrinter(PrinterSerialNumber printer, FilamentSpool spool)
    {
      DisassociateFilamentFromPrinter(printer);
      if (!(spool != null))
      {
        return;
      }

      DisassociateFilamentFromPrinter(spool.filament_uid);
      if (spool.filament_uid == 0U)
      {
        return;
      }

      CurrentFilamentSettings.printerFilamentAssociations.Add(printer, spool.filament_uid);
      for (var index = 0; index < CurrentFilamentSettings.usedFilamentSpools.filament.Count; ++index)
      {
        FilamentSpool other = CurrentFilamentSettings.usedFilamentSpools.filament[index];
        if (spool.Matches(other) && (int) other.filament_uid != (int) spool.filament_uid && FindAssociatedPrinter(other.filament_uid) == PrinterSerialNumber.Undefined)
        {
          CurrentFilamentSettings.usedFilamentSpools.filament.RemoveAt(index);
          --index;
        }
      }
      UpdateUsedFilamentSpool(spool);
      SaveSettings();
    }

    public void UpdateUsedFilamentSpool(FilamentSpool spool)
    {
      if (!(spool != null))
      {
        return;
      }

      FilamentSpool usedSpool = FindUsedSpool(spool.filament_uid);
      if (usedSpool == null)
      {
        CurrentFilamentSettings.usedFilamentSpools.filament.Add(spool);
      }
      else
      {
        if (spool.estimated_filament_length_printed <= (double)usedSpool.estimated_filament_length_printed)
        {
          return;
        }

        usedSpool.estimated_filament_length_printed = spool.estimated_filament_length_printed;
      }
    }

    public void DisassociateFilamentFromPrinter(PrinterSerialNumber printer)
    {
      if (!CurrentFilamentSettings.printerFilamentAssociations.ContainsKey(printer))
      {
        return;
      }

      CurrentFilamentSettings.printerFilamentAssociations.Remove(printer);
    }

    private void DisassociateFilamentFromPrinter(uint filamentUID)
    {
      PrinterSerialNumber associatedPrinter = FindAssociatedPrinter(filamentUID);
      if (!(associatedPrinter != PrinterSerialNumber.Undefined))
      {
        return;
      }

      DisassociateFilamentFromPrinter(associatedPrinter);
    }

    private PrinterSerialNumber FindAssociatedPrinter(uint filamentUID)
    {
      PrinterSerialNumber printerSerialNumber = PrinterSerialNumber.Undefined;
      foreach (var filamentAssociation in CurrentFilamentSettings.printerFilamentAssociations)
      {
        if ((int) filamentAssociation.Value == (int) filamentUID)
        {
          printerSerialNumber = filamentAssociation.Key;
        }
      }
      return printerSerialNumber;
    }

    public FilamentSpool FindUsedSpool(uint filament_uid)
    {
      return CurrentFilamentSettings.usedFilamentSpools.filament.Find(x => (int)x.filament_uid == (int)filament_uid);
    }

    public void OnShutdown()
    {
      SaveSettings();
    }

    public static bool SavePrintingObjectsDetails(JobParams jobParams, List<PrintDetails.ObjectDetails> objectList)
    {
      return SettingsManager.SavePrintingObjectsDetails(Path.Combine(M3D.Spooling.Core.Paths.QueuePath, jobParams.jobGuid) + "_printerview.xml", objectList);
    }

    public static bool SavePrintingObjectsDetails(string printerViewFile, List<PrintDetails.ObjectDetails> objectList)
    {
      var objectViewDetails = new PrintDetails.PrintJobObjectViewDetails(objectList);
      try
      {
        var textWriter = (TextWriter) new StreamWriter(printerViewFile);
        var namespaces = new XmlSerializerNamespaces();
        namespaces.Add(string.Empty, string.Empty);
        PrintDetails.PrintJobObjectViewDetails.ClassSerializer.Serialize(textWriter, objectViewDetails, namespaces);
        textWriter.Close();
      }
      catch (Exception ex)
      {
        return false;
      }
      return true;
    }

    public static bool LoadPrinterView(string jobGuid, out PrintDetails.PrintJobObjectViewDetails printerview_settings)
    {
      return SettingsManager.LoadPrinterViewFile(Path.Combine(M3D.Spooling.Core.Paths.QueuePath, jobGuid) + "_printerview.xml", out printerview_settings);
    }

    public static bool LoadPrinterViewFile(string filename, out PrintDetails.PrintJobObjectViewDetails printerview_settings)
    {
      try
      {
        var textReader = (TextReader) new StreamReader(filename);
        XmlSerializer classSerializer = PrintDetails.PrintJobObjectViewDetails.ClassSerializer;
        printerview_settings = (PrintDetails.PrintJobObjectViewDetails) classSerializer.Deserialize(textReader);
        textReader.Close();
        return true;
      }
      catch (Exception ex)
      {
        printerview_settings = null;
        return false;
      }
    }

    public static bool SavePrintJobInfo(JobParams jobParams, PrinterObject printer, string slicerProfileName, List<Slicer.General.KeyValuePair<string, string>> userKeyValuePairList)
    {
      return SettingsManager.SavePrintJobInfo(Path.Combine(M3D.Spooling.Core.Paths.QueuePath, jobParams.jobGuid) + "_printersettings.xml", jobParams, printer, slicerProfileName, userKeyValuePairList);
    }

    public static bool SavePrintJobInfo(string printerSettingsFile, JobParams jobParams, PrinterObject printer, string slicerProfileName, List<Slicer.General.KeyValuePair<string, string>> userKeyValuePairList)
    {
      var m3Dsettings = new PrintDetails.M3DSettings(jobParams, printer, slicerProfileName, userKeyValuePairList);
      try
      {
        var textWriter = (TextWriter) new StreamWriter(printerSettingsFile);
        var namespaces = new XmlSerializerNamespaces();
        namespaces.Add(string.Empty, string.Empty);
        PrintDetails.M3DSettings.ClassSerializer.Serialize(textWriter, m3Dsettings, namespaces);
        textWriter.Close();
      }
      catch (Exception ex)
      {
        return false;
      }
      return true;
    }

    public static PrintDetails.M3DSettings? LoadPrintJobInfo(string jobGuid)
    {
      PrintDetails.M3DSettings? nullable = SettingsManager.LoadPrintJobInfoFile(Path.Combine(M3D.Spooling.Core.Paths.QueuePath, jobGuid) + "_printersettings.xml");
      if (!nullable.HasValue)
      {
        return nullable;
      }

      PrintDetails.M3DSettings m3Dsettings = nullable.Value;
      m3Dsettings.jobGuid = jobGuid;
      return new PrintDetails.M3DSettings?(m3Dsettings);
    }

    public static PrintDetails.M3DSettings? LoadPrintJobInfoFile(string printerSettingsFile)
    {
      try
      {
        var textReader = (TextReader) new StreamReader(printerSettingsFile);
        var m3Dsettings = (PrintDetails.M3DSettings) PrintDetails.M3DSettings.ClassSerializer.Deserialize(textReader);
        textReader.Close();
        if (string.IsNullOrEmpty(m3Dsettings.profileName))
        {
          m3Dsettings.profileName = "Micro";
        }

        return new PrintDetails.M3DSettings?(m3Dsettings);
      }
      catch (Exception ex)
      {
        return new PrintDetails.M3DSettings?();
      }
    }

    public enum GridUnit
    {
      Inches,
      MM,
    }

    public class M3DSettings
    {
      [XmlElement("PrintSettings")]
      public SerializableDictionary<string, SettingsManager.PrintSettings> printSettings;
      [XmlElement("CustomFilamentProfiles")]
      public SettingsManager.CustomFilamentProfiles customFilamentProfiles;
      [XmlElement("Appearance")]
      public SettingsManager.AppearanceSettings appearanceSettings;
      [XmlElement("Misc")]
      public SettingsManager.MiscSettings miscSettings;
      [XmlElement("Filament")]
      public SettingsManager.FilamentSettings filamentSettings;
      private static XmlSerializer __class_serializer;

      public M3DSettings()
      {
        printSettings = new SerializableDictionary<string, SettingsManager.PrintSettings>();
        customFilamentProfiles = new SettingsManager.CustomFilamentProfiles();
        appearanceSettings = new SettingsManager.AppearanceSettings();
        miscSettings = new SettingsManager.MiscSettings();
        filamentSettings = new SettingsManager.FilamentSettings();
      }

      public SettingsManager.PrintSettings GetPrintSettingsSafe(string printerProfileName)
      {
        if (!printSettings.ContainsKey(printerProfileName))
        {
          printSettings.Add(printerProfileName, new SettingsManager.PrintSettings());
        }

        return printSettings[printerProfileName];
      }

      public static XmlSerializer ClassSerializer
      {
        get
        {
          if (SettingsManager.M3DSettings.__class_serializer == null)
          {
            SettingsManager.M3DSettings.__class_serializer = new XmlSerializer(typeof (SettingsManager.M3DSettings));
          }

          return SettingsManager.M3DSettings.__class_serializer;
        }
      }
    }

    public class AppearanceSettings
    {
      [XmlElement("PrinterColor")]
      public string PrinterColor;
      [XmlElement("ModelColor")]
      public string ModelColor;
      [XmlElement("IconColor")]
      public string IconColor;
      [XmlIgnore]
      public bool StartFullScreen;
      [XmlIgnore]
      public bool ShowAllWarnings;
      [XmlElement("ShowPrinterMismatchWarning")]
      public bool ShowPrinterMismatchWarning;
      [XmlElement("ShowImproveHelpDialog")]
      public bool ShowImproveHelpDialog;
      [XmlElement("ShowRemoveModelWarning")]
      public bool ShowRemoveModelWarning;
      [XmlElement("UseMultipleModels")]
      public bool UseMultipleModels;
      [XmlElement("AutoDetectModelUnits")]
      public bool AutoDetectModelUnits;
      [XmlElement("AllowSDOnlyPrinting")]
      public bool AllowSDOnlyPrinting;
      [XmlElement("Units")]
      public SettingsManager.GridUnit Units;
      [XmlElement("RenderMode")]
      public OpenGLRendererObject.OpenGLRenderMode RenderMode;
      [XmlElement("UpdaterMode")]
      public Updater.UpdateSettings UpdaterMode;
      private PrinterSizeProfile.CaseType _CaseType;
      [XmlIgnore]
      public Color4 auto_filament_color;

      public AppearanceSettings()
      {
        PrinterColor = "Automatic";
        ModelColor = "Automatic";
        IconColor = "Standard";
        StartFullScreen = false;
        ShowImproveHelpDialog = true;
        ShowPrinterMismatchWarning = true;
        ShowAllWarnings = true;
        ShowRemoveModelWarning = true;
        UseMultipleModels = true;
        UpdaterMode = Updater.UpdateSettings.DownloadNotInstall;
        auto_filament_color = new Color4(98, 181, 233, byte.MaxValue);
        Units = SettingsManager.GridUnit.MM;
        RenderMode = OpenGLRendererObject.OpenGLRenderMode.VBOs;
        AutoDetectModelUnits = true;
        AllowSDOnlyPrinting = false;
        CaseType = PrinterSizeProfile.CaseType.ProCase;
      }

      public PrinterSizeProfile.CaseType CaseType
      {
        get
        {
          return _CaseType;
        }
        set
        {
          _CaseType = value;
        }
      }

      [XmlElement("StartFullScreen")]
      public string StartFullScreenXML
      {
        set
        {
          StartFullScreen = value.ToLowerInvariant() == "true";
        }
        get
        {
          return !StartFullScreen ? "false" : "True";
        }
      }

      [XmlElement("ShowAllWarnings")]
      public string ShowAllWarningsXML
      {
        set
        {
          ShowAllWarnings = value.ToLowerInvariant() == "true";
        }
        get
        {
          return !ShowAllWarnings ? "false" : "True";
        }
      }
    }

    public class FileAssociationSettings
    {
      [XmlIgnore]
      public bool ShowFileAssociationsDialog;

      public FileAssociationSettings()
      {
        ShowFileAssociationsDialog = true;
      }

      [XmlElement("ShowFileAssociationsDialog")]
      public string ShowFileAssociationsDialogXML
      {
        set
        {
          ShowFileAssociationsDialog = value.ToLowerInvariant() == "true";
        }
        get
        {
          return !ShowFileAssociationsDialog ? "false" : "True";
        }
      }
    }

    public class MiscSettings
    {
      [XmlElement("FileAssociations")]
      public SettingsManager.FileAssociationSettings FileAssociations;
      [XmlElement("LastProFeaturesFlag")]
      public SerializableDictionary<string, uint> LastProFeaturesFlag;

      public MiscSettings()
      {
        FileAssociations = new SettingsManager.FileAssociationSettings();
        LastProFeaturesFlag = new SerializableDictionary<string, uint>();
      }
    }

    public class Filament
    {
      [XmlElement("Type")]
      public string Type;
      [XmlElement("Color")]
      public string Color;
      [XmlElement("Temperature")]
      public int Temperature;
    }

    public class CustomFilamentProfiles
    {
      [XmlElement("Filament", Type = typeof (SettingsManager.Filament))]
      public List<SettingsManager.Filament> profiles = new List<SettingsManager.Filament>();

      public void PullDataFromDictionary(FilamentDictionary dictionary)
      {
        profiles.Clear();
        foreach (var customValue in dictionary.CustomValues)
        {
          profiles.Add(new SettingsManager.Filament()
          {
            Type = customValue.Key.type.ToString(),
            Color = customValue.Key.color.ToString(),
            Temperature = customValue.Value.temperature
          });
        }
      }

      public void PushDataToDictionary(FilamentDictionary dictionary)
      {
        foreach (SettingsManager.Filament profile in profiles)
        {
          FilamentProfile.TypeColorKey typeColorKey;
          typeColorKey.type = FilamentConstants.StringToType(profile.Type);
          typeColorKey.color = FilamentConstants.StringToFilamentColors(profile.Color);
          FilamentProfile.CustomOptions customOptions;
          customOptions.temperature = profile.Temperature;
          dictionary.AddCustomTemperature(typeColorKey.type, typeColorKey.color, customOptions.temperature);
        }
      }
    }

    public class PrintSettings
    {
      [XmlIgnore]
      public bool WaveBonding;
      [XmlIgnore]
      public bool VerifyBed;
      [XmlElement("useheatedbed")]
      public bool UseHeatedBed;
      [XmlElement("autoUntetheredSupport")]
      public bool AutoUntetheredSupport;

      public PrintSettings()
      {
        Reset();
      }

      public void Reset()
      {
        WaveBonding = false;
        VerifyBed = true;
        UseHeatedBed = true;
        AutoUntetheredSupport = false;
      }

      [XmlElement("wavebonding")]
      public string WaveBondingXML
      {
        set
        {
          WaveBonding = value.ToLowerInvariant() == "true";
        }
        get
        {
          return !WaveBonding ? "false" : "True";
        }
      }

      [XmlElement("VerifyBed")]
      public string VerifyBedXML
      {
        set
        {
          VerifyBed = value.ToLowerInvariant() == "true";
        }
        get
        {
          return !VerifyBed ? "false" : "True";
        }
      }
    }

    public class FilamentSettings
    {
      public bool CleanNozzleAfterInsert;
      public bool TrackFilament;
      [XmlElement("FilamentSpoolsDetails")]
      public SettingsManager.UsedSpools usedFilamentSpools;
      [XmlElement("PrinterFilamentAssociations")]
      public SerializableDictionary<PrinterSerialNumber, uint> printerFilamentAssociations;

      public FilamentSettings()
      {
        CleanNozzleAfterInsert = true;
        TrackFilament = true;
        usedFilamentSpools = new SettingsManager.UsedSpools();
        printerFilamentAssociations = new SerializableDictionary<PrinterSerialNumber, uint>();
      }
    }

    public class UsedSpools
    {
      [XmlElement("FilamentSpool", Type = typeof (FilamentSpool))]
      public List<FilamentSpool> filament;

      public UsedSpools()
      {
        filament = new List<FilamentSpool>();
      }
    }
  }
}
