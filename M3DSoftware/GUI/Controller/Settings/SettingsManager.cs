// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Controller.Settings.SettingsManager
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      this.LoadSettings();
    }

    public SettingsManager.M3DSettings Settings
    {
      get
      {
        return this.m3dsettings;
      }
    }

    public SettingsManager.AppearanceSettings CurrentAppearanceSettings
    {
      get
      {
        return this.Settings.appearanceSettings;
      }
    }

    public SettingsManager.MiscSettings CurrentMiscSettings
    {
      get
      {
        return this.Settings.miscSettings;
      }
    }

    public SettingsManager.FilamentSettings CurrentFilamentSettings
    {
      get
      {
        return this.Settings.filamentSettings;
      }
    }

    public IFileAssociations FileAssociations
    {
      get
      {
        return this.fileAssociations;
      }
    }

    public bool ShowAllWarnings
    {
      get
      {
        return this.CurrentAppearanceSettings.ShowAllWarnings;
      }
      set
      {
        this.CurrentAppearanceSettings.ShowAllWarnings = value;
      }
    }

    public FilamentDictionary FilamentDictionary
    {
      get
      {
        return this.filamentDictionary;
      }
    }

    public void LoadSettings()
    {
      try
      {
        TextReader textReader = (TextReader) new StreamReader(M3D.GUI.Paths.SettingsPath);
        this.m3dsettings = (SettingsManager.M3DSettings) SettingsManager.M3DSettings.ClassSerializer.Deserialize(textReader);
        textReader.Close();
        this.Settings.customFilamentProfiles.PushDataToDictionary(this.FilamentDictionary);
      }
      catch (Exception ex)
      {
        this.m3dsettings = new SettingsManager.M3DSettings();
        this.SaveSettings();
      }
    }

    public void SaveSettings()
    {
      this.Settings.customFilamentProfiles.PullDataFromDictionary(this.FilamentDictionary);
      try
      {
        TextWriter textWriter = (TextWriter) new StreamWriter(M3D.GUI.Paths.SettingsPath);
        XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
        namespaces.Add(string.Empty, string.Empty);
        SettingsManager.M3DSettings.ClassSerializer.Serialize(textWriter, (object) this.Settings, namespaces);
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
          return customValue.Value.temperature;
      }
      return FilamentConstants.Temperature.Default(type);
    }

    public FilamentSpool FindMatchingUsedSpool(FilamentSpool spool)
    {
      return this.CurrentFilamentSettings.usedFilamentSpools.filament.Find((Predicate<FilamentSpool>) (x => x.Matches(spool)));
    }

    public void AssociateFilamentToPrinter(PrinterSerialNumber printer, FilamentSpool spool)
    {
      this.DisassociateFilamentFromPrinter(printer);
      if (!(spool != (FilamentSpool) null))
        return;
      this.DisassociateFilamentFromPrinter(spool.filament_uid);
      if (spool.filament_uid == 0U)
        return;
      this.CurrentFilamentSettings.printerFilamentAssociations.Add(printer, spool.filament_uid);
      for (int index = 0; index < this.CurrentFilamentSettings.usedFilamentSpools.filament.Count; ++index)
      {
        FilamentSpool other = this.CurrentFilamentSettings.usedFilamentSpools.filament[index];
        if (spool.Matches(other) && (int) other.filament_uid != (int) spool.filament_uid && this.FindAssociatedPrinter(other.filament_uid) == PrinterSerialNumber.Undefined)
        {
          this.CurrentFilamentSettings.usedFilamentSpools.filament.RemoveAt(index);
          --index;
        }
      }
      this.UpdateUsedFilamentSpool(spool);
      this.SaveSettings();
    }

    public void UpdateUsedFilamentSpool(FilamentSpool spool)
    {
      if (!(spool != (FilamentSpool) null))
        return;
      FilamentSpool usedSpool = this.FindUsedSpool(spool.filament_uid);
      if (usedSpool == (FilamentSpool) null)
      {
        this.CurrentFilamentSettings.usedFilamentSpools.filament.Add(spool);
      }
      else
      {
        if ((double) spool.estimated_filament_length_printed <= (double) usedSpool.estimated_filament_length_printed)
          return;
        usedSpool.estimated_filament_length_printed = spool.estimated_filament_length_printed;
      }
    }

    public void DisassociateFilamentFromPrinter(PrinterSerialNumber printer)
    {
      if (!this.CurrentFilamentSettings.printerFilamentAssociations.ContainsKey(printer))
        return;
      this.CurrentFilamentSettings.printerFilamentAssociations.Remove(printer);
    }

    private void DisassociateFilamentFromPrinter(uint filamentUID)
    {
      PrinterSerialNumber associatedPrinter = this.FindAssociatedPrinter(filamentUID);
      if (!(associatedPrinter != PrinterSerialNumber.Undefined))
        return;
      this.DisassociateFilamentFromPrinter(associatedPrinter);
    }

    private PrinterSerialNumber FindAssociatedPrinter(uint filamentUID)
    {
      PrinterSerialNumber printerSerialNumber = PrinterSerialNumber.Undefined;
      foreach (var filamentAssociation in CurrentFilamentSettings.printerFilamentAssociations)
      {
        if ((int) filamentAssociation.Value == (int) filamentUID)
          printerSerialNumber = filamentAssociation.Key;
      }
      return printerSerialNumber;
    }

    public FilamentSpool FindUsedSpool(uint filament_uid)
    {
      return this.CurrentFilamentSettings.usedFilamentSpools.filament.Find((Predicate<FilamentSpool>) (x => (int) x.filament_uid == (int) filament_uid));
    }

    public void OnShutdown()
    {
      this.SaveSettings();
    }

    public static bool SavePrintingObjectsDetails(JobParams jobParams, List<PrintDetails.ObjectDetails> objectList)
    {
      return SettingsManager.SavePrintingObjectsDetails(Path.Combine(M3D.Spooling.Core.Paths.QueuePath, jobParams.jobGuid) + "_printerview.xml", objectList);
    }

    public static bool SavePrintingObjectsDetails(string printerViewFile, List<PrintDetails.ObjectDetails> objectList)
    {
      PrintDetails.PrintJobObjectViewDetails objectViewDetails = new PrintDetails.PrintJobObjectViewDetails(objectList);
      try
      {
        TextWriter textWriter = (TextWriter) new StreamWriter(printerViewFile);
        XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
        namespaces.Add(string.Empty, string.Empty);
        PrintDetails.PrintJobObjectViewDetails.ClassSerializer.Serialize(textWriter, (object) objectViewDetails, namespaces);
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
        TextReader textReader = (TextReader) new StreamReader(filename);
        XmlSerializer classSerializer = PrintDetails.PrintJobObjectViewDetails.ClassSerializer;
        printerview_settings = (PrintDetails.PrintJobObjectViewDetails) classSerializer.Deserialize(textReader);
        textReader.Close();
        return true;
      }
      catch (Exception ex)
      {
        printerview_settings = (PrintDetails.PrintJobObjectViewDetails) null;
        return false;
      }
    }

    public static bool SavePrintJobInfo(JobParams jobParams, PrinterObject printer, string slicerProfileName, List<Slicer.General.KeyValuePair<string, string>> userKeyValuePairList)
    {
      return SettingsManager.SavePrintJobInfo(Path.Combine(M3D.Spooling.Core.Paths.QueuePath, jobParams.jobGuid) + "_printersettings.xml", jobParams, printer, slicerProfileName, userKeyValuePairList);
    }

    public static bool SavePrintJobInfo(string printerSettingsFile, JobParams jobParams, PrinterObject printer, string slicerProfileName, List<Slicer.General.KeyValuePair<string, string>> userKeyValuePairList)
    {
      PrintDetails.M3DSettings m3Dsettings = new PrintDetails.M3DSettings(jobParams, printer, slicerProfileName, userKeyValuePairList);
      try
      {
        TextWriter textWriter = (TextWriter) new StreamWriter(printerSettingsFile);
        XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
        namespaces.Add(string.Empty, string.Empty);
        PrintDetails.M3DSettings.ClassSerializer.Serialize(textWriter, (object) m3Dsettings, namespaces);
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
        return nullable;
      PrintDetails.M3DSettings m3Dsettings = nullable.Value;
      m3Dsettings.jobGuid = jobGuid;
      return new PrintDetails.M3DSettings?(m3Dsettings);
    }

    public static PrintDetails.M3DSettings? LoadPrintJobInfoFile(string printerSettingsFile)
    {
      try
      {
        TextReader textReader = (TextReader) new StreamReader(printerSettingsFile);
        PrintDetails.M3DSettings m3Dsettings = (PrintDetails.M3DSettings) PrintDetails.M3DSettings.ClassSerializer.Deserialize(textReader);
        textReader.Close();
        if (string.IsNullOrEmpty(m3Dsettings.profileName))
          m3Dsettings.profileName = "Micro";
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
        this.printSettings = new SerializableDictionary<string, SettingsManager.PrintSettings>();
        this.customFilamentProfiles = new SettingsManager.CustomFilamentProfiles();
        this.appearanceSettings = new SettingsManager.AppearanceSettings();
        this.miscSettings = new SettingsManager.MiscSettings();
        this.filamentSettings = new SettingsManager.FilamentSettings();
      }

      public SettingsManager.PrintSettings GetPrintSettingsSafe(string printerProfileName)
      {
        if (!this.printSettings.ContainsKey(printerProfileName))
          this.printSettings.Add(printerProfileName, new SettingsManager.PrintSettings());
        return this.printSettings[printerProfileName];
      }

      public static XmlSerializer ClassSerializer
      {
        get
        {
          if (SettingsManager.M3DSettings.__class_serializer == null)
            SettingsManager.M3DSettings.__class_serializer = new XmlSerializer(typeof (SettingsManager.M3DSettings));
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
        this.PrinterColor = "Automatic";
        this.ModelColor = "Automatic";
        this.IconColor = "Standard";
        this.StartFullScreen = false;
        this.ShowImproveHelpDialog = true;
        this.ShowPrinterMismatchWarning = true;
        this.ShowAllWarnings = true;
        this.ShowRemoveModelWarning = true;
        this.UseMultipleModels = true;
        this.UpdaterMode = Updater.UpdateSettings.DownloadNotInstall;
        this.auto_filament_color = new Color4((byte) 98, (byte) 181, (byte) 233, byte.MaxValue);
        this.Units = SettingsManager.GridUnit.MM;
        this.RenderMode = OpenGLRendererObject.OpenGLRenderMode.VBOs;
        this.AutoDetectModelUnits = true;
        this.AllowSDOnlyPrinting = false;
        this.CaseType = PrinterSizeProfile.CaseType.ProCase;
      }

      public PrinterSizeProfile.CaseType CaseType
      {
        get
        {
          return this._CaseType;
        }
        set
        {
          this._CaseType = value;
        }
      }

      [XmlElement("StartFullScreen")]
      public string StartFullScreenXML
      {
        set
        {
          this.StartFullScreen = value.ToLowerInvariant() == "true";
        }
        get
        {
          return !this.StartFullScreen ? "false" : "True";
        }
      }

      [XmlElement("ShowAllWarnings")]
      public string ShowAllWarningsXML
      {
        set
        {
          this.ShowAllWarnings = value.ToLowerInvariant() == "true";
        }
        get
        {
          return !this.ShowAllWarnings ? "false" : "True";
        }
      }
    }

    public class FileAssociationSettings
    {
      [XmlIgnore]
      public bool ShowFileAssociationsDialog;

      public FileAssociationSettings()
      {
        this.ShowFileAssociationsDialog = true;
      }

      [XmlElement("ShowFileAssociationsDialog")]
      public string ShowFileAssociationsDialogXML
      {
        set
        {
          this.ShowFileAssociationsDialog = value.ToLowerInvariant() == "true";
        }
        get
        {
          return !this.ShowFileAssociationsDialog ? "false" : "True";
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
        this.FileAssociations = new SettingsManager.FileAssociationSettings();
        this.LastProFeaturesFlag = new SerializableDictionary<string, uint>();
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
        this.profiles.Clear();
        foreach (var customValue in dictionary.CustomValues)
        {
          this.profiles.Add(new SettingsManager.Filament()
          {
            Type = customValue.Key.type.ToString(),
            Color = customValue.Key.color.ToString(),
            Temperature = customValue.Value.temperature
          });
        }
      }

      public void PushDataToDictionary(FilamentDictionary dictionary)
      {
        foreach (SettingsManager.Filament profile in this.profiles)
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
        this.Reset();
      }

      public void Reset()
      {
        this.WaveBonding = false;
        this.VerifyBed = true;
        this.UseHeatedBed = true;
        this.AutoUntetheredSupport = false;
      }

      [XmlElement("wavebonding")]
      public string WaveBondingXML
      {
        set
        {
          this.WaveBonding = value.ToLowerInvariant() == "true";
        }
        get
        {
          return !this.WaveBonding ? "false" : "True";
        }
      }

      [XmlElement("VerifyBed")]
      public string VerifyBedXML
      {
        set
        {
          this.VerifyBed = value.ToLowerInvariant() == "true";
        }
        get
        {
          return !this.VerifyBed ? "false" : "True";
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
        this.CleanNozzleAfterInsert = true;
        this.TrackFilament = true;
        this.usedFilamentSpools = new SettingsManager.UsedSpools();
        this.printerFilamentAssociations = new SerializableDictionary<PrinterSerialNumber, uint>();
      }
    }

    public class UsedSpools
    {
      [XmlElement("FilamentSpool", Type = typeof (FilamentSpool))]
      public List<FilamentSpool> filament;

      public UsedSpools()
      {
        this.filament = new List<FilamentSpool>();
      }
    }
  }
}
