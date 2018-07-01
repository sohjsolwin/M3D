// Decompiled with JetBrains decompiler
// Type: M3D.Slicer.Cura15_04.SmartSlicerSettingsCura15_04
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Properties;
using M3D.Slicer.General;
using M3D.SlicerConnectionCura.SlicerSettingsItems;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using M3D.Spooling.Printer_Profiles;
using System;
using System.Collections.Generic;
using System.IO;

namespace M3D.Slicer.Cura15_04
{
  public class SmartSlicerSettingsCura15_04 : SmartSlicerSettingsBase
  {
    private IPrinter m_CurrentPrinter;
    private const string sErrorNotFound = "Slicer setting does not exist";
    private const float fInitialLayerSpeedMultiplier = 0.25f;
    private const float fRaftSpeedMultiplier = 0.5f;
    private const float fTraversalSpeedMultiplier = 0.75f;

    public int MaxSpeed { get; private set; }

    public int WarningSpeed { get; private set; }

    public int DefaultSpeed { get; private set; }

    public int TraversalSpeed { get; private set; }

    public int DefaultRetractionSpeed { get; private set; }

    public int MaxRetractionSpeed { get; private set; }

    public override string ProfileName { get; protected set; }

    public SmartSlicerSettingsCura15_04()
    {
    }

    public SmartSlicerSettingsCura15_04(SmartSlicerSettingsCura15_04 otherSettings)
      : base((SmartSlicerSettingsBase) otherSettings)
    {
      this.MaxSpeed = otherSettings.MaxSpeed;
      this.WarningSpeed = otherSettings.WarningSpeed;
      this.DefaultSpeed = otherSettings.DefaultSpeed;
      this.TraversalSpeed = otherSettings.TraversalSpeed;
      this.DefaultRetractionSpeed = otherSettings.DefaultRetractionSpeed;
      this.MaxRetractionSpeed = otherSettings.MaxRetractionSpeed;
      this.m_CurrentPrinter = otherSettings.m_CurrentPrinter;
    }

    public override bool ParseFile(string filename)
    {
      StreamReader streamReader;
      try
      {
        streamReader = new StreamReader(filename);
      }
      catch (Exception ex)
      {
        return false;
      }
      string line;
      while ((line = streamReader.ReadLine()) != null)
      {
        try
        {
          string option = this.GetOption(line);
          if (!option.StartsWith("#"))
          {
            string val = this.ExtractParameter(line);
            if (val == "\"\"\"")
            {
              val = "";
              string str;
              while ((str = streamReader.ReadLine()) != null && !(str == "\"\"\""))
              {
                if (str.Length > 0)
                  val = val + str + Environment.NewLine;
              }
            }
            if (this.ContainsKey(option))
            {
              if (this[option].ReadSlicerSetting(val) == SettingReadResult.Failed)
              {
                streamReader.Close();
                return false;
              }
            }
          }
        }
        catch (InvalidOperationException ex)
        {
        }
      }
      streamReader.Close();
      return true;
    }

    public override string AdvancedPrintSettingsEmbeddedResource
    {
      get
      {
        return Resources.Cura15_04Settings;
      }
    }

    public override SmartSlicerSettingsBase Clone()
    {
      return (SmartSlicerSettingsBase) new SmartSlicerSettingsCura15_04(this);
    }

    public override void SmartCheckBoxCallBack(string checkBoxName, bool state, FilamentSpool filament)
    {
      if (!(checkBoxName == "InternalToGUI_enableRaft"))
      {
        if (!(checkBoxName == "InternalToGUI_enableSupport"))
        {
          if (!(checkBoxName == "InternalToGUI_enableSkirt"))
          {
            if (!(checkBoxName == "InternalToGUI_autoFanSettings"))
            {
              if (!(checkBoxName == "InternalToGUI_useNozzleSizeForExtrusionWidth"))
                return;
              if (state)
                this.EnableUseNozzleSizeForExtrusionWidth();
              else
                this.DisableUseNozzleSizeForExtrusionWidth();
            }
            else if (state)
              this.EnableAutoFanSettings();
            else
              this.DisableAutoFanSettings();
          }
          else if (state)
            this.EnableSkirt();
          else
            this.DisableSkirt();
        }
        else if (state)
          this.EnableSupport(M3D.Slicer.General.SupportType.GridSupport);
        else
          this.DisableSupport();
      }
      else if (state)
        this.EnableRaft(filament);
      else
        this.DisableRaft();
    }

    public override void ConfigureFromPrinterData(IPrinter oPrinter)
    {
      this.m_CurrentPrinter = oPrinter;
      PrinterInfo info = this.m_CurrentPrinter.Info;
      PrinterProfile myPrinterProfile = this.m_CurrentPrinter.MyPrinterProfile;
      this.ProfileName = myPrinterProfile.ProfileName;
      this.MaxSpeed = (int) ((double) myPrinterProfile.SpeedLimitConstants.FastestPossible / 60.0);
      this.DefaultSpeed = (int) ((double) myPrinterProfile.SpeedLimitConstants.DefaultSpeed / 60.0);
      this.TraversalSpeed = (int) ((double) this.MaxSpeed * 0.75);
      this.WarningSpeed = (int) ((double) this.MaxSpeed * 0.75);
      this.DefaultRetractionSpeed = (int) ((double) myPrinterProfile.SpeedLimitConstants.DEFAULT_FEEDRATE_E_Negative / 60.0);
      this.MaxRetractionSpeed = (int) ((double) myPrinterProfile.SpeedLimitConstants.MAX_FEEDRATE_E_Negative / 60.0);
      if (this.UsingNozzleSizeForExtrusionWidth)
        (this.GetSettingsItem("extrusionWidth") as SettingsItemFloatMMType).value = SmartSlicerSettingsCura15_04.micronsToMM(info.extruder.iNozzleSizeMicrons);
      SettingsItemIntType settingsItem1 = this.GetSettingsItem("initialLayerSpeed") as SettingsItemIntType;
      settingsItem1.value = this.nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.initialLayerSpeed, this.DefaultSpeed);
      settingsItem1.warning_range = new Range<int>(4, this.nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.initialLayerSpeed, this.WarningSpeed));
      settingsItem1.error_range = new Range<int>(1, this.MaxSpeed);
      SettingsItemIntType settingsItem2 = this.GetSettingsItem("printSpeed") as SettingsItemIntType;
      settingsItem2.value = this.nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.printSpeed, this.DefaultSpeed);
      settingsItem2.warning_range = new Range<int>(5, this.WarningSpeed);
      settingsItem2.error_range = new Range<int>(1, this.MaxSpeed);
      SettingsItemIntType settingsItem3 = this.GetSettingsItem("inset0Speed") as SettingsItemIntType;
      settingsItem3.value = this.nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.inset0Speed, this.DefaultSpeed);
      settingsItem3.warning_range = new Range<int>(5, this.WarningSpeed);
      settingsItem3.error_range = new Range<int>(1, this.MaxSpeed);
      SettingsItemIntType settingsItem4 = this.GetSettingsItem("insetXSpeed") as SettingsItemIntType;
      settingsItem4.value = this.nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.insetXSpeed, this.DefaultSpeed);
      settingsItem4.warning_range = new Range<int>(5, this.WarningSpeed);
      settingsItem4.error_range = new Range<int>(1, this.MaxSpeed);
      SettingsItemIntType settingsItem5 = this.GetSettingsItem("moveSpeed") as SettingsItemIntType;
      settingsItem5.value = this.nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.moveSpeed, this.TraversalSpeed);
      settingsItem5.warning_range = new Range<int>(10, this.TraversalSpeed);
      settingsItem5.error_range = new Range<int>(1, this.MaxSpeed);
      SettingsItemIntType settingsItem6 = this.GetSettingsItem("infillSpeed") as SettingsItemIntType;
      settingsItem6.value = this.nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.infillSpeed, this.DefaultSpeed);
      settingsItem6.warning_range = new Range<int>(5, this.WarningSpeed);
      settingsItem6.error_range = new Range<int>(1, this.MaxSpeed);
      SettingsItemIntType settingsItem7 = this.GetSettingsItem("retractionSpeed") as SettingsItemIntType;
      settingsItem7.value = this.nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.retractionSpeed, this.DefaultRetractionSpeed);
      settingsItem7.warning_range = new Range<int>(1, this.DefaultRetractionSpeed);
      settingsItem7.error_range = new Range<int>(1, this.MaxRetractionSpeed);
      SettingsItemIntType settingsItem8 = this.GetSettingsItem("skinSpeed") as SettingsItemIntType;
      settingsItem8.value = this.nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.skinSpeed, this.DefaultSpeed);
      settingsItem8.warning_range = new Range<int>(10, this.WarningSpeed);
      settingsItem8.error_range = new Range<int>(1, this.MaxSpeed);
      SettingsItemIntType settingsItem9 = this.GetSettingsItem("raftBaseSpeed") as SettingsItemIntType;
      settingsItem9.value = 0;
      settingsItem9.warning_range = new Range<int>(4, this.nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.raftBaseSpeed, this.WarningSpeed));
      settingsItem9.error_range = new Range<int>(1, this.MaxSpeed);
      SettingsItemIntType settingsItem10 = this.GetSettingsItem("raftSurfaceSpeed") as SettingsItemIntType;
      settingsItem10.value = 0;
      settingsItem10.warning_range = new Range<int>(4, this.WarningSpeed);
      settingsItem10.error_range = new Range<int>(1, this.MaxSpeed);
    }

    public override void SetPrintQuality(PrintQuality level, FilamentSpool filament, int iModelCount)
    {
      this.FilamentFlow = 100;
      int nProfileProposedSpeed = filament.filament_type != FilamentSpool.TypeEnum.ABS ? (filament.filament_location != FilamentSpool.Location.Internal ? this.DefaultSpeed : (int) ((double) this.DefaultSpeed * 0.899999976158142)) : (int) ((double) this.DefaultSpeed * 1.10000002384186);
      this.InitialSpeedupLayers = 4;
      this.InitialLayerSpeed = this.nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.initialLayerSpeed, (int) ((double) nProfileProposedSpeed * 0.800000011920929));
      this.PrintSpeed = this.nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.printSpeed, nProfileProposedSpeed);
      this.Inset0Speed = this.nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.inset0Speed, (int) ((double) nProfileProposedSpeed * 0.800000011920929));
      this.InsetXSpeed = this.nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.insetXSpeed, (int) ((double) nProfileProposedSpeed * 0.899999976158142));
      this.MoveSpeed = this.nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.moveSpeed, this.TraversalSpeed);
      this.InfillSpeed = this.nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.infillSpeed, nProfileProposedSpeed);
      this.SkinSpeed = this.nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.skinSpeed, nProfileProposedSpeed);
      if (!this.UsingAutoFanSettings)
        this.FanFullOnLayerNr = 2;
      this.SupportXYDistance = 0.7f;
      this.SupportZDistance = 0.15f;
      this.SupportExtruder = -1;
      this.RetractionSpeed = this.nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.retractionSpeed, this.DefaultRetractionSpeed);
      this.RetractionAmountPrime = 0.0f;
      if ("Micro" == this.ProfileName)
      {
        this.RetractionZHop = 0.0f;
        this.MinimalExtrusionBeforeRetraction = 0.1f;
      }
      if (iModelCount > 1)
      {
        this.RetractionAmount = 6f;
        this.RetractionMinimalDistance = 2f;
      }
      else
      {
        this.RetractionAmount = 2.4f;
        this.RetractionMinimalDistance = !("Micro" == this.ProfileName) ? 0.5f : 1.5f;
      }
      if (!this.UsingAutoFanSettings)
      {
        if (filament.filament_type == FilamentSpool.TypeEnum.ABS)
        {
          this.FanSpeedMin = 0;
          this.FanSpeedMax = 1;
        }
        else if (filament.filament_type == FilamentSpool.TypeEnum.PLA || filament.filament_type == FilamentSpool.TypeEnum.FLX || filament.filament_type == FilamentSpool.TypeEnum.TGH)
        {
          this.FanSpeedMin = 100;
          this.FanSpeedMax = 100;
          this.FanFullOnLayerNr = 1;
        }
      }
      if (!this.SupportedPrintQualities.Contains(level))
        return;
      switch (level)
      {
        case PrintQuality.Expert:
          this.LayerThickness = SmartSlicerSettingsCura15_04.micronsToMM(50);
          break;
        case PrintQuality.VeryHighQuality:
          this.LayerThickness = SmartSlicerSettingsCura15_04.micronsToMM(150);
          break;
        case PrintQuality.HighQuality:
          this.LayerThickness = SmartSlicerSettingsCura15_04.micronsToMM(200);
          break;
        case PrintQuality.MediumQuality:
          this.LayerThickness = SmartSlicerSettingsCura15_04.micronsToMM(250);
          break;
        case PrintQuality.FastPrint:
          this.LayerThickness = SmartSlicerSettingsCura15_04.micronsToMM(300);
          break;
        case PrintQuality.VeryFastPrint:
          this.LayerThickness = SmartSlicerSettingsCura15_04.micronsToMM(350);
          break;
        default:
          return;
      }
      this.InitialLayerThickness = 0.3f;
    }

    public override void SetToDefault()
    {
      this.LoadSettingsItemsFromFile();
    }

    public override void SetFillQuality(FillQuality level)
    {
      if (!this.SupportedFillQualities.Contains(level))
        return;
      switch (level)
      {
        case FillQuality.HollowThinWalls:
          this.DownSkinCount = this.mmToLayerCountConverter(0.75f);
          this.UpSkinCount = this.mmToLayerCountConverter(0.75f);
          this.InsetCount = 1;
          this.SparseInfillLineDistance = -1f;
          this.InfillPattern = FillPaternCura.Automatic;
          break;
        case FillQuality.HollowThickWalls:
          this.DownSkinCount = this.mmToLayerCountConverter(0.75f);
          this.UpSkinCount = this.mmToLayerCountConverter(1.25f);
          this.InsetCount = 3;
          this.SparseInfillLineDistance = -1f;
          this.InfillPattern = FillPaternCura.Automatic;
          break;
        case FillQuality.Solid:
          this.DownSkinCount = this.mmToLayerCountConverter(0.75f);
          this.UpSkinCount = this.mmToLayerCountConverter(0.75f);
          this.InsetCount = 3;
          this.SparseInfillLineDistance = 0.35f;
          this.InfillSpeed = this.DefaultSpeed;
          this.InfillPattern = FillPaternCura.Automatic;
          break;
        case FillQuality.ExtraHigh:
          this.DownSkinCount = this.mmToLayerCountConverter(1.5f);
          this.UpSkinCount = this.mmToLayerCountConverter(2f);
          this.InsetCount = 4;
          this.SparseInfillLineDistance = 1.5f;
          this.InfillSpeed = this.DefaultSpeed;
          this.InfillPattern = FillPaternCura.Automatic;
          break;
        case FillQuality.High:
          this.DownSkinCount = this.mmToLayerCountConverter(1.5f);
          this.UpSkinCount = this.mmToLayerCountConverter(2f);
          this.InsetCount = 4;
          this.SparseInfillLineDistance = 2.5f;
          this.InfillSpeed = this.DefaultSpeed;
          this.InfillPattern = FillPaternCura.Automatic;
          break;
        case FillQuality.Medium:
          this.DownSkinCount = this.mmToLayerCountConverter(1.5f);
          this.UpSkinCount = this.mmToLayerCountConverter(2f);
          this.InsetCount = 4;
          this.SparseInfillLineDistance = 4f;
          this.InfillSpeed = this.DefaultSpeed;
          this.InfillPattern = FillPaternCura.Automatic;
          break;
        case FillQuality.Low:
          this.DownSkinCount = this.mmToLayerCountConverter(1f);
          this.UpSkinCount = this.mmToLayerCountConverter(1.5f);
          this.InsetCount = 3;
          this.SparseInfillLineDistance = 5.5f;
          this.InfillSpeed = this.DefaultSpeed;
          this.InfillPattern = FillPaternCura.Automatic;
          break;
      }
    }

    public override void EnableRaft(FilamentSpool filament)
    {
      this.RaftMargin = 2f;
      this.RaftLineSpacing = 3f;
      this.RaftBaseThickness = 0.4f;
      this.RaftBaseLineWidth = 2.5f;
      this.RaftBaseSpeed = (int) ((double) this.DefaultSpeed * 0.5);
      this.RaftInterfaceThickness = 0.4f;
      this.RaftInterfaceLinewidth = 1f;
      this.RaftInterfaceLineSpacing = 2f;
      if (filament.filament_type == FilamentSpool.TypeEnum.PLA)
      {
        this.RaftFanSpeed = 100;
        this.RaftAirGapLayer0 = 0.285f;
        this.RaftAirGap = true;
      }
      else if (filament.filament_type == FilamentSpool.TypeEnum.TGH || filament.filament_type == FilamentSpool.TypeEnum.FLX)
      {
        this.RaftFanSpeed = 100;
        this.RaftAirGapLayer0 = 0.5f;
        this.RaftAirGap = true;
      }
      else
      {
        this.RaftFanSpeed = 0;
        this.RaftAirGapLayer0 = 0.5f;
        this.RaftAirGap = true;
      }
      this.RaftSurfaceThickness = 0.2f;
      this.RaftSurfaceLinewidth = 0.25f;
      this.RaftSurfaceLineSpacing = 0.25f;
      this.RaftSurfaceLayers = 2;
      this.RaftSurfaceSpeed = this.DefaultSpeed;
    }

    public override void EnableSkirt()
    {
      this.SkirtDistance = 2f;
      this.SkirtLineCount = 1;
      this.SkirtMinLength = 0.0f;
    }

    public override void DisableSkirt()
    {
      this.SkirtDistance = 0.0f;
      this.SkirtLineCount = 0;
      this.SkirtMinLength = 0.0f;
    }

    private void EnableNonRaftThickBase()
    {
      this.Layer0ExtrusionWidth = 1.5f;
      this.InitialLayerThickness = 0.4f;
      this.InitialLayerSpeed = (int) ((double) this.DefaultSpeed * 0.25);
    }

    public override void EnableSupport(M3D.Slicer.General.SupportType supportType)
    {
      this.SupportAngle = -1;
      this.SupportXYDistance = 0.7f;
      this.SupportZDistance = 0.15f;
      this.SupportExtruder = -1;
      if (supportType == M3D.Slicer.General.SupportType.LineSupport || supportType == M3D.Slicer.General.SupportType.LineSupportEveryWhere)
        this.SupportType = SupportPatternCura.Lines;
      else if (supportType == M3D.Slicer.General.SupportType.GridSupport || supportType == M3D.Slicer.General.SupportType.GridSupportEveryWhere)
        this.SupportType = SupportPatternCura.Grid;
      if (supportType == M3D.Slicer.General.SupportType.LineSupport || supportType == M3D.Slicer.General.SupportType.GridSupport)
      {
        this.SupportAngle = 50;
        this.SupportEverywhere = 0;
      }
      else
      {
        if (supportType != M3D.Slicer.General.SupportType.LineSupportEveryWhere && supportType != M3D.Slicer.General.SupportType.GridSupportEveryWhere)
          return;
        this.SupportEverywhere = 50;
        this.SupportAngle = 50;
      }
    }

    public override void DisableSupport()
    {
      this.SupportAngle = -1;
    }

    public override void DisableRaft()
    {
      this.RaftMargin = 0.0f;
      this.RaftLineSpacing = 0.0f;
      this.RaftBaseThickness = 0.0f;
      this.RaftBaseLineWidth = 0.0f;
      this.RaftBaseSpeed = 0;
      this.RaftInterfaceThickness = 0.0f;
      this.RaftInterfaceLinewidth = 0.0f;
      this.RaftInterfaceLineSpacing = 0.0f;
      this.RaftFanSpeed = 0;
      this.RaftAirGapLayer0 = 0.0f;
      this.RaftAirGap = false;
      this.RaftSurfaceThickness = 0.0f;
      this.RaftSurfaceLinewidth = 0.0f;
      this.RaftSurfaceLineSpacing = 0.0f;
      this.RaftSurfaceLayers = 0;
      this.RaftSurfaceSpeed = 0;
      this.EnableNonRaftThickBase();
    }

    public override void EnableAutoFanSettings()
    {
      this.FanFullOnLayerNr = -1;
      this.FanSpeedMax = -1;
      this.FanSpeedMin = -1;
    }

    public override void DisableAutoFanSettings()
    {
      this.FanFullOnLayerNr = 2;
      this.FanSpeedMax = 100;
      this.FanSpeedMin = 40;
    }

    public override void EnableUseNozzleSizeForExtrusionWidth()
    {
      if (this.m_CurrentPrinter == null)
        return;
      this.ExtrusionWidth = (float) this.m_CurrentPrinter.Info.extruder.iNozzleSizeMicrons / 1000f;
      int num = (int) this.m_CurrentPrinter.AddUpdateKeyValuePair((M3D.Spooling.Client.AsyncCallback) null, (object) null, "UsingNozzleSizeForExtrusionWidth", "true");
    }

    public override void DisableUseNozzleSizeForExtrusionWidth()
    {
      if (this.m_CurrentPrinter == null)
        return;
      int num = (int) this.m_CurrentPrinter.AddUpdateKeyValuePair((M3D.Spooling.Client.AsyncCallback) null, (object) null, "UsingNozzleSizeForExtrusionWidth", "false");
    }

    public int mmToLayerCountConverter(float mmLayerThickness)
    {
      float layerThickness = this.LayerThickness;
      int num = (int) ((double) mmLayerThickness / (double) layerThickness + 0.5);
      if (num < 3)
        num = 3;
      return num;
    }

    public override PrintQuality CurrentPrintQuality
    {
      get
      {
        PrintQuality result;
        if (Enum.TryParse<PrintQuality>(SmartSlicerSettingsCura15_04.millimetersToMicrons(this.LayerThickness).ToString(), out result) && this.SupportedPrintQualities.Contains(result))
          return result;
        return PrintQuality.Custom;
      }
    }

    public override FillQuality CurrentFillQuality
    {
      get
      {
        if ((double) this.SparseInfillLineDistance == -1.0)
        {
          if (this.InsetCount == 1)
            return FillQuality.HollowThinWalls;
          if (this.InsetCount == 3)
            return FillQuality.HollowThickWalls;
        }
        FillQuality result;
        if (Enum.TryParse<FillQuality>(SmartSlicerSettingsCura15_04.millimetersToMicrons(this.SparseInfillLineDistance).ToString(), out result) && this.SupportedFillQualities.Contains(result))
          return result;
        return FillQuality.Custom;
      }
    }

    public override bool HasRaftEnabled
    {
      get
      {
        if ((double) this.RaftMargin > 0.0 && (double) this.RaftLineSpacing > 0.0 && ((double) this.RaftBaseThickness > 0.0 && (double) this.RaftBaseLineWidth > 0.0) && (this.RaftBaseSpeed > 0 && (double) this.RaftInterfaceThickness > 0.0 && ((double) this.RaftInterfaceLinewidth > 0.0 && (double) this.RaftInterfaceLineSpacing > 0.0)) && ((double) this.RaftSurfaceThickness > 0.0 && (double) this.RaftSurfaceLinewidth > 0.0 && ((double) this.RaftSurfaceLineSpacing > 0.0 && this.RaftSurfaceLayers > 0)))
          return this.RaftSurfaceSpeed > 0;
        return false;
      }
    }

    public override bool HasSupport
    {
      get
      {
        return this.SupportAngle != -1;
      }
    }

    public override bool HasSkirt
    {
      get
      {
        if ((double) this.SkirtDistance > 0.0)
          return this.SkirtLineCount > 0;
        return false;
      }
    }

    public override bool UsingAutoFanSettings
    {
      get
      {
        if (this.FanFullOnLayerNr >= 0 && this.FanSpeedMax >= 0)
          return this.FanSpeedMin < 0;
        return true;
      }
    }

    public override bool UsingNozzleSizeForExtrusionWidth
    {
      get
      {
        if (this.m_CurrentPrinter != null)
        {
          string valueFromPrinter = this.m_CurrentPrinter.GetValidatedValueFromPrinter(nameof (UsingNozzleSizeForExtrusionWidth));
          if (valueFromPrinter != null)
            return "true" == valueFromPrinter;
          if (this.m_CurrentPrinter.MyPrinterProfile.AccessoriesConstants.NozzleConstants.bHasInterchangeableNozzle && (int) ((double) this.ExtrusionWidth * 1000.0) == this.m_CurrentPrinter.Info.extruder.iNozzleSizeMicrons)
          {
            int num = (int) this.m_CurrentPrinter.AddUpdateKeyValuePair((M3D.Spooling.Client.AsyncCallback) null, (object) null, nameof (UsingNozzleSizeForExtrusionWidth), "true");
            return true;
          }
          int num1 = (int) this.m_CurrentPrinter.AddUpdateKeyValuePair((M3D.Spooling.Client.AsyncCallback) null, (object) null, nameof (UsingNozzleSizeForExtrusionWidth), "false");
        }
        return false;
      }
    }

    public override bool CustomNozzleAvailable
    {
      get
      {
        if (this.m_CurrentPrinter == null)
          return false;
        return this.m_CurrentPrinter.MyPrinterProfile.AccessoriesConstants.NozzleConstants.bHasInterchangeableNozzle;
      }
    }

    public override bool UsingCustomExtrusionWidth
    {
      get
      {
        if (this.m_CurrentPrinter != null)
          return (double) this.ExtrusionWidth != (double) this.m_CurrentPrinter.Info.extruder.iNozzleSizeMicrons / 1000.0;
        return false;
      }
    }

    public override bool HasModelonModelSupport
    {
      get
      {
        if (this.SupportEverywhere > 0)
          return this.HasSupport;
        return false;
      }
    }

    public override List<PrintQuality> SupportedPrintQualities
    {
      get
      {
        List<PrintQuality> printQualityList;
        if ("Pro" == this.ProfileName)
          printQualityList = new List<PrintQuality>()
          {
            PrintQuality.FastPrint,
            PrintQuality.HighQuality,
            PrintQuality.VeryFastPrint
          };
        else
          printQualityList = new List<PrintQuality>((IEnumerable<PrintQuality>) Enum.GetValues(typeof (PrintQuality)));
        return printQualityList;
      }
    }

    public override List<FillQuality> SupportedFillQualities
    {
      get
      {
        return new List<FillQuality>((IEnumerable<FillQuality>) Enum.GetValues(typeof (FillQuality)));
      }
    }

    private float LayerThickness
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("layerThickness") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("layerThickness") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float InitialLayerThickness
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("initialLayerThickness") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("initialLayerThickness") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float FilamentDiameter
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("filamentDiameter") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("filamentDiameter") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int FilamentFlow
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("filamentFlow") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("filamentFlow") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float Layer0ExtrusionWidth
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("layer0extrusionWidth") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("layer0extrusionWidth") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float ExtrusionWidth
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("extrusionWidth") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("extrusionWidth") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int InsetCount
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("insetCount") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("insetCount") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int DownSkinCount
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("downSkinCount") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("downSkinCount") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int UpSkinCount
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("upSkinCount") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("upSkinCount") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float SkirtDistance
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("skirtDistance") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("skirtDistance") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int SkirtLineCount
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("skirtLineCount") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("skirtLineCount") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float SkirtMinLength
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("skirtMinLength") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("skirtMinLength") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int InitialSpeedupLayers
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("initialSpeedupLayers") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("initialSpeedupLayers") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int InitialLayerSpeed
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("initialLayerSpeed") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("initialLayerSpeed") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int PrintSpeed
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("printSpeed") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("printSpeed") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int SkinSpeed
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("skinSpeed") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("skinSpeed") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int Inset0Speed
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("inset0Speed") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("inset0Speed") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int InsetXSpeed
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("insetXSpeed") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("insetXSpeed") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int MoveSpeed
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("moveSpeed") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("moveSpeed") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int FanFullOnLayerNr
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("fanFullOnLayerNr") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("fanFullOnLayerNr") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float SparseInfillLineDistance
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("sparseInfillLineDistance") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("sparseInfillLineDistance") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int InfillOverlap
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("infillOverlap") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("infillOverlap") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int InfillSpeed
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("infillSpeed") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("infillSpeed") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private FillPaternCura InfillPattern
    {
      get
      {
        SettingsItemFillPatternTypeCura settingsItem = this.GetSettingsItem("infillPattern") as SettingsItemFillPatternTypeCura;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFillPatternTypeCura settingsItem = this.GetSettingsItem("infillPattern") as SettingsItemFillPatternTypeCura;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private SupportPatternCura SupportType
    {
      get
      {
        SettingsItemSupportPatternTypeCura settingsItem = this.GetSettingsItem("supportType") as SettingsItemSupportPatternTypeCura;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemSupportPatternTypeCura settingsItem = this.GetSettingsItem("supportType") as SettingsItemSupportPatternTypeCura;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int SupportAngle
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("supportAngle") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("supportAngle") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int SupportEverywhere
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("supportEverywhere") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("supportEverywhere") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float SupportLineDistance
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("supportLineDistance") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("supportLineDistance") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float SupportXYDistance
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("supportXYDistance") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("supportXYDistance") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float SupportZDistance
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("supportZDistance") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("supportZDistance") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int SupportExtruder
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("supportExtruder") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("supportExtruder") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float RetractionAmount
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("retractionAmount") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("retractionAmount") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float RetractionAmountPrime
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("retractionAmountPrime") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("retractionAmountPrime") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int RetractionSpeed
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("retractionSpeed") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("retractionSpeed") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float RetractionAmountExtruderSwitch
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("retractionAmountExtruderSwitch") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("retractionAmountExtruderSwitch") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float RetractionMinimalDistance
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("retractionMinimalDistance") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("retractionMinimalDistance") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float MinimalExtrusionBeforeRetraction
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("minimalExtrusionBeforeRetraction") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("minimalExtrusionBeforeRetraction") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float RetractionZHop
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("retractionZHop") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("retractionZHop") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private bool EnableCombing
    {
      get
      {
        SettingsItemBoolType settingsItem = this.GetSettingsItem("enableCombing") as SettingsItemBoolType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemBoolType settingsItem = this.GetSettingsItem("enableCombing") as SettingsItemBoolType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int FixHorrible
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("fixHorrible") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("fixHorrible") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int SpiralizeMode
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("spiralizeMode") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("spiralizeMode") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int SimpleMode
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("simpleMode") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("simpleMode") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private GCodeFlavorCura GCodeFlavor
    {
      get
      {
        SettingsItemGCodeFlavorTypeCura settingsItem = this.GetSettingsItem("gcodeFlavor") as SettingsItemGCodeFlavorTypeCura;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemGCodeFlavorTypeCura settingsItem = this.GetSettingsItem("gcodeFlavor") as SettingsItemGCodeFlavorTypeCura;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float ObjectSink
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("objectSink") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("objectSink") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private bool AutoCenter
    {
      get
      {
        SettingsItemBoolType settingsItem = this.GetSettingsItem("autoCenter") as SettingsItemBoolType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemBoolType settingsItem = this.GetSettingsItem("autoCenter") as SettingsItemBoolType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float RaftMargin
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("raftMargin") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("raftMargin") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float RaftLineSpacing
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("raftLineSpacing") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("raftLineSpacing") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float RaftBaseThickness
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("raftBaseThickness") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("raftBaseThickness") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float RaftBaseLineWidth
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("raftBaseLinewidth") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("raftBaseLinewidth") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float RaftInterfaceThickness
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("raftInterfaceThickness") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("raftInterfaceThickness") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float RaftInterfaceLinewidth
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("raftInterfaceLinewidth") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("raftInterfaceLinewidth") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float RaftInterfaceLineSpacing
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("raftInterfaceLineSpacing") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("raftInterfaceLineSpacing") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private bool RaftAirGap
    {
      get
      {
        SettingsItemBoolType settingsItem = this.GetSettingsItem("raftAirGap") as SettingsItemBoolType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemBoolType settingsItem = this.GetSettingsItem("raftAirGap") as SettingsItemBoolType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float RaftAirGapLayer0
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("raftAirGapLayer0") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("raftAirGapLayer0") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int RaftBaseSpeed
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("raftBaseSpeed") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("raftBaseSpeed") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int RaftFanSpeed
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("raftFanSpeed") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("raftFanSpeed") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float RaftSurfaceThickness
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("raftSurfaceThickness") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("raftSurfaceThickness") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float RaftSurfaceLinewidth
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("raftSurfaceLinewidth") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("raftSurfaceLinewidth") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float RaftSurfaceLineSpacing
    {
      get
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("raftSurfaceLineSpacing") as SettingsItemFloatMMType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatMMType settingsItem = this.GetSettingsItem("raftSurfaceLineSpacing") as SettingsItemFloatMMType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int RaftSurfaceLayers
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("raftSurfaceLayers") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("raftSurfaceLayers") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int RaftSurfaceSpeed
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("raftSurfaceSpeed") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("raftSurfaceSpeed") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int MinimalLayerTime
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("minimalLayerTime") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("minimalLayerTime") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int MinimalFeedrate
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("minimalFeedrate") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("minimalFeedrate") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private float CoolHeadLift
    {
      get
      {
        SettingsItemFloatSecondsType settingsItem = this.GetSettingsItem("coolHeadLift") as SettingsItemFloatSecondsType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemFloatSecondsType settingsItem = this.GetSettingsItem("coolHeadLift") as SettingsItemFloatSecondsType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int FanSpeedMin
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("fanSpeedMin") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("fanSpeedMin") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int FanSpeedMax
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("fanSpeedMax") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("fanSpeedMax") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private bool EnableOozeShield
    {
      get
      {
        SettingsItemBoolType settingsItem = this.GetSettingsItem("enableOozeShield") as SettingsItemBoolType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemBoolType settingsItem = this.GetSettingsItem("enableOozeShield") as SettingsItemBoolType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int WipeTowerSize
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("wipeTowerSize") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("wipeTowerSize") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private int MultiVolumeOverlap
    {
      get
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("multiVolumeOverlap") as SettingsItemIntType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemIntType settingsItem = this.GetSettingsItem("multiVolumeOverlap") as SettingsItemIntType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private string StartCode
    {
      get
      {
        SettingsItemStringType settingsItem = this.GetSettingsItem("startCode") as SettingsItemStringType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemStringType settingsItem = this.GetSettingsItem("startCode") as SettingsItemStringType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    private string EndCode
    {
      get
      {
        SettingsItemStringType settingsItem = this.GetSettingsItem("endCode") as SettingsItemStringType;
        if (settingsItem != null)
          return settingsItem.value;
        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        SettingsItemStringType settingsItem = this.GetSettingsItem("endCode") as SettingsItemStringType;
        if (settingsItem == null)
          throw new Exception("Slicer setting does not exist");
        settingsItem.value = value;
      }
    }

    public static float micronsToMM(int microns)
    {
      return (float) microns / 1000f;
    }

    public static int millimetersToMicrons(float mm)
    {
      return (int) ((double) mm * 1000.0);
    }

    public override string ConfigurationFileName
    {
      get
      {
        return string.Format("defaultCura15_04{0}.cfg", (object) this.ProfileName);
      }
    }

    public override List<General.KeyValuePair<string, string>> GenerateUserKeyValuePairList()
    {
      var keyValuePairList = new List<General.KeyValuePair<string, string>>();
      foreach (System.Collections.Generic.KeyValuePair<string, SlicerSettingsItem> keyValuePair in (SmartSlicerSettingsBase) this)
      {
        if (!(keyValuePair.Value is SettingsItemStringType))
          keyValuePairList.Add(new General.KeyValuePair<string, string>(keyValuePair.Key, keyValuePair.Value.TranslateToUserValue()));
      }
      return keyValuePairList;
    }

    public override void LoadFromUserKeyValuePairList(List<General.KeyValuePair<string, string>> list)
    {
      foreach (General.KeyValuePair<string, string> keyValuePair in list)
        this.GetSettingsItem(keyValuePair.Key)?.ParseUserValue(keyValuePair.Value);
    }

    public SlicerSettingsItem GetSettingsItem(string name)
    {
      if (this.ContainsKey(name))
        return this[name];
      return (SlicerSettingsItem) null;
    }

    public override void SerializeToSlicer(StreamWriter streamwriter)
    {
      streamwriter.WriteLine("# Generated by M3D Printer Software");
      foreach (System.Collections.Generic.KeyValuePair<string, SlicerSettingsItem> keyValuePair in (SmartSlicerSettingsBase) this)
      {
        string str = string.Format("{0} = {1}", (object) keyValuePair.Key, (object) keyValuePair.Value.TranslateToSlicerValue());
        streamwriter.WriteLine(str);
      }
    }

    private string ExtractParameter(string line)
    {
      int num = line.IndexOf("=");
      if (num < 0)
        throw new InvalidOperationException(SmartSlicerSettingsCura15_04.sErrorReading(line));
      int startIndex = num + 1;
      while (startIndex < line.Length && line[startIndex] == ' ')
        ++startIndex;
      if (startIndex >= line.Length)
        throw new InvalidOperationException(SmartSlicerSettingsCura15_04.sErrorReading(line));
      return line.Substring(startIndex);
    }

    private string GetOption(string line)
    {
      int length = line.IndexOf(' ');
      if (length <= 0)
        throw new InvalidOperationException(SmartSlicerSettingsCura15_04.sErrorReading(line));
      return line.Substring(0, length);
    }

    private static string sErrorReading(string sLine)
    {
      return "Option without parameter: '" + sLine + "'";
    }

    private int nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem eWhichSpeed, int nProfileProposedSpeed)
    {
      int val2 = nProfileProposedSpeed;
      if ("Pro" == this.ProfileName)
      {
        switch (eWhichSpeed)
        {
          case SmartSlicerSettingsCura15_04.SpeedItem.initialLayerSpeed:
          case SmartSlicerSettingsCura15_04.SpeedItem.raftBaseSpeed:
            val2 = Math.Min(20, val2);
            break;
          case SmartSlicerSettingsCura15_04.SpeedItem.skinSpeed:
            val2 = 42;
            break;
          case SmartSlicerSettingsCura15_04.SpeedItem.inset0Speed:
            val2 = Math.Min(24, val2);
            break;
          case SmartSlicerSettingsCura15_04.SpeedItem.insetXSpeed:
            val2 = Math.Min(36, val2);
            break;
          case SmartSlicerSettingsCura15_04.SpeedItem.moveSpeed:
            val2 = Math.Min(60, val2);
            break;
        }
      }
      else if ("Micro+" == this.ProfileName)
      {
        switch (eWhichSpeed)
        {
          case SmartSlicerSettingsCura15_04.SpeedItem.initialLayerSpeed:
          case SmartSlicerSettingsCura15_04.SpeedItem.raftBaseSpeed:
            val2 = Math.Min(18, val2);
            break;
          case SmartSlicerSettingsCura15_04.SpeedItem.skinSpeed:
            val2 = Math.Min(30, val2);
            break;
          case SmartSlicerSettingsCura15_04.SpeedItem.inset0Speed:
            val2 = Math.Min(20, val2);
            break;
          case SmartSlicerSettingsCura15_04.SpeedItem.insetXSpeed:
            val2 = Math.Min(28, val2);
            break;
          case SmartSlicerSettingsCura15_04.SpeedItem.moveSpeed:
            val2 = Math.Min(30, val2);
            break;
        }
      }
      return val2;
    }

    private enum SpeedItem
    {
      initialLayerSpeed,
      printSpeed,
      skinSpeed,
      inset0Speed,
      insetXSpeed,
      moveSpeed,
      infillSpeed,
      retractionSpeed,
      raftBaseSpeed,
      raftSurfaceSpeed,
    }
  }
}
