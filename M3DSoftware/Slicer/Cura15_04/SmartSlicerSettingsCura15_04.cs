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
      : base(otherSettings)
    {
      MaxSpeed = otherSettings.MaxSpeed;
      WarningSpeed = otherSettings.WarningSpeed;
      DefaultSpeed = otherSettings.DefaultSpeed;
      TraversalSpeed = otherSettings.TraversalSpeed;
      DefaultRetractionSpeed = otherSettings.DefaultRetractionSpeed;
      MaxRetractionSpeed = otherSettings.MaxRetractionSpeed;
      m_CurrentPrinter = otherSettings.m_CurrentPrinter;
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
          var option = GetOption(line);
          if (!option.StartsWith("#"))
          {
            var val = ExtractParameter(line);
            if (val == "\"\"\"")
            {
              val = "";
              string str;
              while ((str = streamReader.ReadLine()) != null && !(str == "\"\"\""))
              {
                if (str.Length > 0)
                {
                  val = val + str + Environment.NewLine;
                }
              }
            }
            if (ContainsKey(option))
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
      return new SmartSlicerSettingsCura15_04(this);
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
              {
                return;
              }

              if (state)
              {
                EnableUseNozzleSizeForExtrusionWidth();
              }
              else
              {
                DisableUseNozzleSizeForExtrusionWidth();
              }
            }
            else if (state)
            {
              EnableAutoFanSettings();
            }
            else
            {
              DisableAutoFanSettings();
            }
          }
          else if (state)
          {
            EnableSkirt();
          }
          else
          {
            DisableSkirt();
          }
        }
        else if (state)
        {
          EnableSupport(M3D.Slicer.General.SupportType.GridSupport);
        }
        else
        {
          DisableSupport();
        }
      }
      else if (state)
      {
        EnableRaft(filament);
      }
      else
      {
        DisableRaft();
      }
    }

    public override void ConfigureFromPrinterData(IPrinter oPrinter)
    {
      m_CurrentPrinter = oPrinter;
      PrinterInfo info = m_CurrentPrinter.Info;
      PrinterProfile myPrinterProfile = m_CurrentPrinter.MyPrinterProfile;
      ProfileName = myPrinterProfile.ProfileName;
      MaxSpeed = (int)(myPrinterProfile.SpeedLimitConstants.FastestPossible / 60.0);
      DefaultSpeed = (int)(myPrinterProfile.SpeedLimitConstants.DefaultSpeed / 60.0);
      TraversalSpeed = (int)(MaxSpeed * 0.75);
      WarningSpeed = (int)(MaxSpeed * 0.75);
      DefaultRetractionSpeed = (int)(myPrinterProfile.SpeedLimitConstants.DEFAULT_FEEDRATE_E_Negative / 60.0);
      MaxRetractionSpeed = (int)(myPrinterProfile.SpeedLimitConstants.MAX_FEEDRATE_E_Negative / 60.0);
      if (UsingNozzleSizeForExtrusionWidth)
      {
        (GetSettingsItem("extrusionWidth") as SettingsItemFloatMMType).value = SmartSlicerSettingsCura15_04.micronsToMM(info.extruder.iNozzleSizeMicrons);
      }

      var settingsItem1 = GetSettingsItem("initialLayerSpeed") as SettingsItemIntType;
      settingsItem1.value = nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.initialLayerSpeed, DefaultSpeed);
      settingsItem1.warning_range = new Range<int>(4, nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.initialLayerSpeed, WarningSpeed));
      settingsItem1.error_range = new Range<int>(1, MaxSpeed);
      var settingsItem2 = GetSettingsItem("printSpeed") as SettingsItemIntType;
      settingsItem2.value = nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.printSpeed, DefaultSpeed);
      settingsItem2.warning_range = new Range<int>(5, WarningSpeed);
      settingsItem2.error_range = new Range<int>(1, MaxSpeed);
      var settingsItem3 = GetSettingsItem("inset0Speed") as SettingsItemIntType;
      settingsItem3.value = nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.inset0Speed, DefaultSpeed);
      settingsItem3.warning_range = new Range<int>(5, WarningSpeed);
      settingsItem3.error_range = new Range<int>(1, MaxSpeed);
      var settingsItem4 = GetSettingsItem("insetXSpeed") as SettingsItemIntType;
      settingsItem4.value = nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.insetXSpeed, DefaultSpeed);
      settingsItem4.warning_range = new Range<int>(5, WarningSpeed);
      settingsItem4.error_range = new Range<int>(1, MaxSpeed);
      var settingsItem5 = GetSettingsItem("moveSpeed") as SettingsItemIntType;
      settingsItem5.value = nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.moveSpeed, TraversalSpeed);
      settingsItem5.warning_range = new Range<int>(10, TraversalSpeed);
      settingsItem5.error_range = new Range<int>(1, MaxSpeed);
      var settingsItem6 = GetSettingsItem("infillSpeed") as SettingsItemIntType;
      settingsItem6.value = nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.infillSpeed, DefaultSpeed);
      settingsItem6.warning_range = new Range<int>(5, WarningSpeed);
      settingsItem6.error_range = new Range<int>(1, MaxSpeed);
      var settingsItem7 = GetSettingsItem("retractionSpeed") as SettingsItemIntType;
      settingsItem7.value = nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.retractionSpeed, DefaultRetractionSpeed);
      settingsItem7.warning_range = new Range<int>(1, DefaultRetractionSpeed);
      settingsItem7.error_range = new Range<int>(1, MaxRetractionSpeed);
      var settingsItem8 = GetSettingsItem("skinSpeed") as SettingsItemIntType;
      settingsItem8.value = nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.skinSpeed, DefaultSpeed);
      settingsItem8.warning_range = new Range<int>(10, WarningSpeed);
      settingsItem8.error_range = new Range<int>(1, MaxSpeed);
      var settingsItem9 = GetSettingsItem("raftBaseSpeed") as SettingsItemIntType;
      settingsItem9.value = 0;
      settingsItem9.warning_range = new Range<int>(4, nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.raftBaseSpeed, WarningSpeed));
      settingsItem9.error_range = new Range<int>(1, MaxSpeed);
      var settingsItem10 = GetSettingsItem("raftSurfaceSpeed") as SettingsItemIntType;
      settingsItem10.value = 0;
      settingsItem10.warning_range = new Range<int>(4, WarningSpeed);
      settingsItem10.error_range = new Range<int>(1, MaxSpeed);
    }

    public override void SetPrintQuality(PrintQuality level, FilamentSpool filament, int iModelCount)
    {
      FilamentFlow = 100;
      var nProfileProposedSpeed = filament.filament_type != FilamentSpool.TypeEnum.ABS ? (filament.filament_location != FilamentSpool.Location.Internal ? DefaultSpeed : (int)(DefaultSpeed * 0.899999976158142)) : (int)(DefaultSpeed * 1.10000002384186);
      InitialSpeedupLayers = 4;
      InitialLayerSpeed = nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.initialLayerSpeed, (int)(nProfileProposedSpeed * 0.800000011920929));
      PrintSpeed = nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.printSpeed, nProfileProposedSpeed);
      Inset0Speed = nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.inset0Speed, (int)(nProfileProposedSpeed * 0.800000011920929));
      InsetXSpeed = nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.insetXSpeed, (int)(nProfileProposedSpeed * 0.899999976158142));
      MoveSpeed = nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.moveSpeed, TraversalSpeed);
      InfillSpeed = nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.infillSpeed, nProfileProposedSpeed);
      SkinSpeed = nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.skinSpeed, nProfileProposedSpeed);
      if (!UsingAutoFanSettings)
      {
        FanFullOnLayerNr = 2;
      }

      SupportXYDistance = 0.7f;
      SupportZDistance = 0.15f;
      SupportExtruder = -1;
      RetractionSpeed = nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem.retractionSpeed, DefaultRetractionSpeed);
      RetractionAmountPrime = 0.0f;
      if ("Micro" == ProfileName)
      {
        RetractionZHop = 0.0f;
        MinimalExtrusionBeforeRetraction = 0.1f;
      }
      if (iModelCount > 1)
      {
        RetractionAmount = 6f;
        RetractionMinimalDistance = 2f;
      }
      else
      {
        RetractionAmount = 2.4f;
        RetractionMinimalDistance = !("Micro" == ProfileName) ? 0.5f : 1.5f;
      }
      if (!UsingAutoFanSettings)
      {
        if (filament.filament_type == FilamentSpool.TypeEnum.ABS)
        {
          FanSpeedMin = 0;
          FanSpeedMax = 1;
        }
        else if (filament.filament_type == FilamentSpool.TypeEnum.PLA || filament.filament_type == FilamentSpool.TypeEnum.FLX || filament.filament_type == FilamentSpool.TypeEnum.TGH)
        {
          FanSpeedMin = 100;
          FanSpeedMax = 100;
          FanFullOnLayerNr = 1;
        }
      }
      if (!SupportedPrintQualities.Contains(level))
      {
        return;
      }

      switch (level)
      {
        case PrintQuality.Expert:
          LayerThickness = SmartSlicerSettingsCura15_04.micronsToMM(50);
          break;
        case PrintQuality.VeryHighQuality:
          LayerThickness = SmartSlicerSettingsCura15_04.micronsToMM(150);
          break;
        case PrintQuality.HighQuality:
          LayerThickness = SmartSlicerSettingsCura15_04.micronsToMM(200);
          break;
        case PrintQuality.MediumQuality:
          LayerThickness = SmartSlicerSettingsCura15_04.micronsToMM(250);
          break;
        case PrintQuality.FastPrint:
          LayerThickness = SmartSlicerSettingsCura15_04.micronsToMM(300);
          break;
        case PrintQuality.VeryFastPrint:
          LayerThickness = SmartSlicerSettingsCura15_04.micronsToMM(350);
          break;
        default:
          return;
      }
      InitialLayerThickness = 0.3f;
    }

    public override void SetToDefault()
    {
      LoadSettingsItemsFromFile();
    }

    public override void SetFillQuality(FillQuality level)
    {
      if (!SupportedFillQualities.Contains(level))
      {
        return;
      }

      switch (level)
      {
        case FillQuality.HollowThinWalls:
          DownSkinCount = mmToLayerCountConverter(0.75f);
          UpSkinCount = mmToLayerCountConverter(0.75f);
          InsetCount = 1;
          SparseInfillLineDistance = -1f;
          InfillPattern = FillPaternCura.Automatic;
          break;
        case FillQuality.HollowThickWalls:
          DownSkinCount = mmToLayerCountConverter(0.75f);
          UpSkinCount = mmToLayerCountConverter(1.25f);
          InsetCount = 3;
          SparseInfillLineDistance = -1f;
          InfillPattern = FillPaternCura.Automatic;
          break;
        case FillQuality.Solid:
          DownSkinCount = mmToLayerCountConverter(0.75f);
          UpSkinCount = mmToLayerCountConverter(0.75f);
          InsetCount = 3;
          SparseInfillLineDistance = 0.35f;
          InfillSpeed = DefaultSpeed;
          InfillPattern = FillPaternCura.Automatic;
          break;
        case FillQuality.ExtraHigh:
          DownSkinCount = mmToLayerCountConverter(1.5f);
          UpSkinCount = mmToLayerCountConverter(2f);
          InsetCount = 4;
          SparseInfillLineDistance = 1.5f;
          InfillSpeed = DefaultSpeed;
          InfillPattern = FillPaternCura.Automatic;
          break;
        case FillQuality.High:
          DownSkinCount = mmToLayerCountConverter(1.5f);
          UpSkinCount = mmToLayerCountConverter(2f);
          InsetCount = 4;
          SparseInfillLineDistance = 2.5f;
          InfillSpeed = DefaultSpeed;
          InfillPattern = FillPaternCura.Automatic;
          break;
        case FillQuality.Medium:
          DownSkinCount = mmToLayerCountConverter(1.5f);
          UpSkinCount = mmToLayerCountConverter(2f);
          InsetCount = 4;
          SparseInfillLineDistance = 4f;
          InfillSpeed = DefaultSpeed;
          InfillPattern = FillPaternCura.Automatic;
          break;
        case FillQuality.Low:
          DownSkinCount = mmToLayerCountConverter(1f);
          UpSkinCount = mmToLayerCountConverter(1.5f);
          InsetCount = 3;
          SparseInfillLineDistance = 5.5f;
          InfillSpeed = DefaultSpeed;
          InfillPattern = FillPaternCura.Automatic;
          break;
      }
    }

    public override void EnableRaft(FilamentSpool filament)
    {
      RaftMargin = 2f;
      RaftLineSpacing = 3f;
      RaftBaseThickness = 0.4f;
      RaftBaseLineWidth = 2.5f;
      RaftBaseSpeed = (int)(DefaultSpeed * 0.5);
      RaftInterfaceThickness = 0.4f;
      RaftInterfaceLinewidth = 1f;
      RaftInterfaceLineSpacing = 2f;
      if (filament.filament_type == FilamentSpool.TypeEnum.PLA)
      {
        RaftFanSpeed = 100;
        RaftAirGapLayer0 = 0.285f;
        RaftAirGap = true;
      }
      else if (filament.filament_type == FilamentSpool.TypeEnum.TGH || filament.filament_type == FilamentSpool.TypeEnum.FLX)
      {
        RaftFanSpeed = 100;
        RaftAirGapLayer0 = 0.5f;
        RaftAirGap = true;
      }
      else
      {
        RaftFanSpeed = 0;
        RaftAirGapLayer0 = 0.5f;
        RaftAirGap = true;
      }
      RaftSurfaceThickness = 0.2f;
      RaftSurfaceLinewidth = 0.25f;
      RaftSurfaceLineSpacing = 0.25f;
      RaftSurfaceLayers = 2;
      RaftSurfaceSpeed = DefaultSpeed;
    }

    public override void EnableSkirt()
    {
      SkirtDistance = 2f;
      SkirtLineCount = 1;
      SkirtMinLength = 0.0f;
    }

    public override void DisableSkirt()
    {
      SkirtDistance = 0.0f;
      SkirtLineCount = 0;
      SkirtMinLength = 0.0f;
    }

    private void EnableNonRaftThickBase()
    {
      Layer0ExtrusionWidth = 1.5f;
      InitialLayerThickness = 0.4f;
      InitialLayerSpeed = (int)(DefaultSpeed * 0.25);
    }

    public override void EnableSupport(M3D.Slicer.General.SupportType supportType)
    {
      SupportAngle = -1;
      SupportXYDistance = 0.7f;
      SupportZDistance = 0.15f;
      SupportExtruder = -1;
      if (supportType == M3D.Slicer.General.SupportType.LineSupport || supportType == M3D.Slicer.General.SupportType.LineSupportEveryWhere)
      {
        SupportType = SupportPatternCura.Lines;
      }
      else if (supportType == M3D.Slicer.General.SupportType.GridSupport || supportType == M3D.Slicer.General.SupportType.GridSupportEveryWhere)
      {
        SupportType = SupportPatternCura.Grid;
      }

      if (supportType == M3D.Slicer.General.SupportType.LineSupport || supportType == M3D.Slicer.General.SupportType.GridSupport)
      {
        SupportAngle = 50;
        SupportEverywhere = 0;
      }
      else
      {
        if (supportType != M3D.Slicer.General.SupportType.LineSupportEveryWhere && supportType != M3D.Slicer.General.SupportType.GridSupportEveryWhere)
        {
          return;
        }

        SupportEverywhere = 50;
        SupportAngle = 50;
      }
    }

    public override void DisableSupport()
    {
      SupportAngle = -1;
    }

    public override void DisableRaft()
    {
      RaftMargin = 0.0f;
      RaftLineSpacing = 0.0f;
      RaftBaseThickness = 0.0f;
      RaftBaseLineWidth = 0.0f;
      RaftBaseSpeed = 0;
      RaftInterfaceThickness = 0.0f;
      RaftInterfaceLinewidth = 0.0f;
      RaftInterfaceLineSpacing = 0.0f;
      RaftFanSpeed = 0;
      RaftAirGapLayer0 = 0.0f;
      RaftAirGap = false;
      RaftSurfaceThickness = 0.0f;
      RaftSurfaceLinewidth = 0.0f;
      RaftSurfaceLineSpacing = 0.0f;
      RaftSurfaceLayers = 0;
      RaftSurfaceSpeed = 0;
      EnableNonRaftThickBase();
    }

    public override void EnableAutoFanSettings()
    {
      FanFullOnLayerNr = -1;
      FanSpeedMax = -1;
      FanSpeedMin = -1;
    }

    public override void DisableAutoFanSettings()
    {
      FanFullOnLayerNr = 2;
      FanSpeedMax = 100;
      FanSpeedMin = 40;
    }

    public override void EnableUseNozzleSizeForExtrusionWidth()
    {
      if (m_CurrentPrinter == null)
      {
        return;
      }

      ExtrusionWidth = m_CurrentPrinter.Info.extruder.iNozzleSizeMicrons / 1000f;
      var num = (int)m_CurrentPrinter.AddUpdateKeyValuePair(null, null, "UsingNozzleSizeForExtrusionWidth", "true");
    }

    public override void DisableUseNozzleSizeForExtrusionWidth()
    {
      if (m_CurrentPrinter == null)
      {
        return;
      }

      var num = (int)m_CurrentPrinter.AddUpdateKeyValuePair(null, null, "UsingNozzleSizeForExtrusionWidth", "false");
    }

    public int mmToLayerCountConverter(float mmLayerThickness)
    {
      var layerThickness = LayerThickness;
      var num = (int)(mmLayerThickness / (double)layerThickness + 0.5);
      if (num < 3)
      {
        num = 3;
      }

      return num;
    }

    public override PrintQuality CurrentPrintQuality
    {
      get
      {
        if (Enum.TryParse<PrintQuality>(SmartSlicerSettingsCura15_04.millimetersToMicrons(LayerThickness).ToString(), out PrintQuality result) && SupportedPrintQualities.Contains(result))
        {
          return result;
        }

        return PrintQuality.Custom;
      }
    }

    public override FillQuality CurrentFillQuality
    {
      get
      {
        if (SparseInfillLineDistance == -1.0)
        {
          if (InsetCount == 1)
          {
            return FillQuality.HollowThinWalls;
          }

          if (InsetCount == 3)
          {
            return FillQuality.HollowThickWalls;
          }
        }
        if (Enum.TryParse<FillQuality>(SmartSlicerSettingsCura15_04.millimetersToMicrons(SparseInfillLineDistance).ToString(), out FillQuality result) && SupportedFillQualities.Contains(result))
        {
          return result;
        }

        return FillQuality.Custom;
      }
    }

    public override bool HasRaftEnabled
    {
      get
      {
        if (RaftMargin > 0.0 && RaftLineSpacing > 0.0 && (RaftBaseThickness > 0.0 && RaftBaseLineWidth > 0.0) && (RaftBaseSpeed > 0 && RaftInterfaceThickness > 0.0 && (RaftInterfaceLinewidth > 0.0 && RaftInterfaceLineSpacing > 0.0)) && (RaftSurfaceThickness > 0.0 && RaftSurfaceLinewidth > 0.0 && (RaftSurfaceLineSpacing > 0.0 && RaftSurfaceLayers > 0)))
        {
          return RaftSurfaceSpeed > 0;
        }

        return false;
      }
    }

    public override bool HasSupport
    {
      get
      {
        return SupportAngle != -1;
      }
    }

    public override bool HasSkirt
    {
      get
      {
        if (SkirtDistance > 0.0)
        {
          return SkirtLineCount > 0;
        }

        return false;
      }
    }

    public override bool UsingAutoFanSettings
    {
      get
      {
        if (FanFullOnLayerNr >= 0 && FanSpeedMax >= 0)
        {
          return FanSpeedMin < 0;
        }

        return true;
      }
    }

    public override bool UsingNozzleSizeForExtrusionWidth
    {
      get
      {
        if (m_CurrentPrinter != null)
        {
          var valueFromPrinter = m_CurrentPrinter.GetValidatedValueFromPrinter(nameof (UsingNozzleSizeForExtrusionWidth));
          if (valueFromPrinter != null)
          {
            return "true" == valueFromPrinter;
          }

          if (m_CurrentPrinter.MyPrinterProfile.AccessoriesConstants.NozzleConstants.bHasInterchangeableNozzle && (int)(ExtrusionWidth * 1000.0) == m_CurrentPrinter.Info.extruder.iNozzleSizeMicrons)
          {
            var num = (int)m_CurrentPrinter.AddUpdateKeyValuePair(null, null, nameof (UsingNozzleSizeForExtrusionWidth), "true");
            return true;
          }
          var num1 = (int)m_CurrentPrinter.AddUpdateKeyValuePair(null, null, nameof (UsingNozzleSizeForExtrusionWidth), "false");
        }
        return false;
      }
    }

    public override bool CustomNozzleAvailable
    {
      get
      {
        if (m_CurrentPrinter == null)
        {
          return false;
        }

        return m_CurrentPrinter.MyPrinterProfile.AccessoriesConstants.NozzleConstants.bHasInterchangeableNozzle;
      }
    }

    public override bool UsingCustomExtrusionWidth
    {
      get
      {
        if (m_CurrentPrinter != null)
        {
          return ExtrusionWidth != m_CurrentPrinter.Info.extruder.iNozzleSizeMicrons / 1000.0;
        }

        return false;
      }
    }

    public override bool HasModelonModelSupport
    {
      get
      {
        if (SupportEverywhere > 0)
        {
          return HasSupport;
        }

        return false;
      }
    }

    public override List<PrintQuality> SupportedPrintQualities
    {
      get
      {
        List<PrintQuality> printQualityList;
        if ("Pro" == ProfileName)
        {
          printQualityList = new List<PrintQuality>()
          {
            PrintQuality.FastPrint,
            PrintQuality.HighQuality,
            PrintQuality.VeryFastPrint
          };
        }
        else
        {
          printQualityList = new List<PrintQuality>((IEnumerable<PrintQuality>) Enum.GetValues(typeof (PrintQuality)));
        }

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
        if (GetSettingsItem("layerThickness") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("layerThickness") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float InitialLayerThickness
    {
      get
      {
        if (GetSettingsItem("initialLayerThickness") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("initialLayerThickness") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float FilamentDiameter
    {
      get
      {
        if (GetSettingsItem("filamentDiameter") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("filamentDiameter") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int FilamentFlow
    {
      get
      {
        if (GetSettingsItem("filamentFlow") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("filamentFlow") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float Layer0ExtrusionWidth
    {
      get
      {
        if (GetSettingsItem("layer0extrusionWidth") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("layer0extrusionWidth") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float ExtrusionWidth
    {
      get
      {
        if (GetSettingsItem("extrusionWidth") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("extrusionWidth") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int InsetCount
    {
      get
      {
        if (GetSettingsItem("insetCount") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("insetCount") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int DownSkinCount
    {
      get
      {
        if (GetSettingsItem("downSkinCount") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("downSkinCount") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int UpSkinCount
    {
      get
      {
        if (GetSettingsItem("upSkinCount") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("upSkinCount") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float SkirtDistance
    {
      get
      {
        if (GetSettingsItem("skirtDistance") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("skirtDistance") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int SkirtLineCount
    {
      get
      {
        if (GetSettingsItem("skirtLineCount") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("skirtLineCount") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float SkirtMinLength
    {
      get
      {
        if (GetSettingsItem("skirtMinLength") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("skirtMinLength") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int InitialSpeedupLayers
    {
      get
      {
        if (GetSettingsItem("initialSpeedupLayers") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("initialSpeedupLayers") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int InitialLayerSpeed
    {
      get
      {
        if (GetSettingsItem("initialLayerSpeed") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("initialLayerSpeed") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int PrintSpeed
    {
      get
      {
        if (GetSettingsItem("printSpeed") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("printSpeed") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int SkinSpeed
    {
      get
      {
        if (GetSettingsItem("skinSpeed") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("skinSpeed") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int Inset0Speed
    {
      get
      {
        if (GetSettingsItem("inset0Speed") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("inset0Speed") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int InsetXSpeed
    {
      get
      {
        if (GetSettingsItem("insetXSpeed") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("insetXSpeed") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int MoveSpeed
    {
      get
      {
        if (GetSettingsItem("moveSpeed") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("moveSpeed") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int FanFullOnLayerNr
    {
      get
      {
        if (GetSettingsItem("fanFullOnLayerNr") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("fanFullOnLayerNr") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float SparseInfillLineDistance
    {
      get
      {
        if (GetSettingsItem("sparseInfillLineDistance") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("sparseInfillLineDistance") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int InfillOverlap
    {
      get
      {
        if (GetSettingsItem("infillOverlap") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("infillOverlap") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int InfillSpeed
    {
      get
      {
        if (GetSettingsItem("infillSpeed") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("infillSpeed") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private FillPaternCura InfillPattern
    {
      get
      {
        if (GetSettingsItem("infillPattern") is SettingsItemFillPatternTypeCura settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("infillPattern") as SettingsItemFillPatternTypeCura;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private SupportPatternCura SupportType
    {
      get
      {
        if (GetSettingsItem("supportType") is SettingsItemSupportPatternTypeCura settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("supportType") as SettingsItemSupportPatternTypeCura;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int SupportAngle
    {
      get
      {
        if (GetSettingsItem("supportAngle") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("supportAngle") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int SupportEverywhere
    {
      get
      {
        if (GetSettingsItem("supportEverywhere") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("supportEverywhere") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float SupportLineDistance
    {
      get
      {
        if (GetSettingsItem("supportLineDistance") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("supportLineDistance") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float SupportXYDistance
    {
      get
      {
        if (GetSettingsItem("supportXYDistance") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("supportXYDistance") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float SupportZDistance
    {
      get
      {
        if (GetSettingsItem("supportZDistance") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("supportZDistance") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int SupportExtruder
    {
      get
      {
        if (GetSettingsItem("supportExtruder") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("supportExtruder") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float RetractionAmount
    {
      get
      {
        if (GetSettingsItem("retractionAmount") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("retractionAmount") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float RetractionAmountPrime
    {
      get
      {
        if (GetSettingsItem("retractionAmountPrime") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("retractionAmountPrime") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int RetractionSpeed
    {
      get
      {
        if (GetSettingsItem("retractionSpeed") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("retractionSpeed") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float RetractionAmountExtruderSwitch
    {
      get
      {
        if (GetSettingsItem("retractionAmountExtruderSwitch") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("retractionAmountExtruderSwitch") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float RetractionMinimalDistance
    {
      get
      {
        if (GetSettingsItem("retractionMinimalDistance") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("retractionMinimalDistance") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float MinimalExtrusionBeforeRetraction
    {
      get
      {
        if (GetSettingsItem("minimalExtrusionBeforeRetraction") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("minimalExtrusionBeforeRetraction") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float RetractionZHop
    {
      get
      {
        if (GetSettingsItem("retractionZHop") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("retractionZHop") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private bool EnableCombing
    {
      get
      {
        if (GetSettingsItem("enableCombing") is SettingsItemBoolType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("enableCombing") as SettingsItemBoolType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int FixHorrible
    {
      get
      {
        if (GetSettingsItem("fixHorrible") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("fixHorrible") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int SpiralizeMode
    {
      get
      {
        if (GetSettingsItem("spiralizeMode") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("spiralizeMode") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int SimpleMode
    {
      get
      {
        if (GetSettingsItem("simpleMode") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("simpleMode") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private GCodeFlavorCura GCodeFlavor
    {
      get
      {
        if (GetSettingsItem("gcodeFlavor") is SettingsItemGCodeFlavorTypeCura settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("gcodeFlavor") as SettingsItemGCodeFlavorTypeCura;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float ObjectSink
    {
      get
      {
        if (GetSettingsItem("objectSink") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("objectSink") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private bool AutoCenter
    {
      get
      {
        if (GetSettingsItem("autoCenter") is SettingsItemBoolType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("autoCenter") as SettingsItemBoolType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float RaftMargin
    {
      get
      {
        if (GetSettingsItem("raftMargin") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("raftMargin") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float RaftLineSpacing
    {
      get
      {
        if (GetSettingsItem("raftLineSpacing") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("raftLineSpacing") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float RaftBaseThickness
    {
      get
      {
        if (GetSettingsItem("raftBaseThickness") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("raftBaseThickness") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float RaftBaseLineWidth
    {
      get
      {
        if (GetSettingsItem("raftBaseLinewidth") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("raftBaseLinewidth") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float RaftInterfaceThickness
    {
      get
      {
        if (GetSettingsItem("raftInterfaceThickness") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("raftInterfaceThickness") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float RaftInterfaceLinewidth
    {
      get
      {
        if (GetSettingsItem("raftInterfaceLinewidth") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("raftInterfaceLinewidth") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float RaftInterfaceLineSpacing
    {
      get
      {
        if (GetSettingsItem("raftInterfaceLineSpacing") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("raftInterfaceLineSpacing") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private bool RaftAirGap
    {
      get
      {
        if (GetSettingsItem("raftAirGap") is SettingsItemBoolType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("raftAirGap") as SettingsItemBoolType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float RaftAirGapLayer0
    {
      get
      {
        if (GetSettingsItem("raftAirGapLayer0") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("raftAirGapLayer0") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int RaftBaseSpeed
    {
      get
      {
        if (GetSettingsItem("raftBaseSpeed") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("raftBaseSpeed") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int RaftFanSpeed
    {
      get
      {
        if (GetSettingsItem("raftFanSpeed") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("raftFanSpeed") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float RaftSurfaceThickness
    {
      get
      {
        if (GetSettingsItem("raftSurfaceThickness") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("raftSurfaceThickness") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float RaftSurfaceLinewidth
    {
      get
      {
        if (GetSettingsItem("raftSurfaceLinewidth") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("raftSurfaceLinewidth") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float RaftSurfaceLineSpacing
    {
      get
      {
        if (GetSettingsItem("raftSurfaceLineSpacing") is SettingsItemFloatMMType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("raftSurfaceLineSpacing") as SettingsItemFloatMMType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int RaftSurfaceLayers
    {
      get
      {
        if (GetSettingsItem("raftSurfaceLayers") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("raftSurfaceLayers") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int RaftSurfaceSpeed
    {
      get
      {
        if (GetSettingsItem("raftSurfaceSpeed") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("raftSurfaceSpeed") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int MinimalLayerTime
    {
      get
      {
        if (GetSettingsItem("minimalLayerTime") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("minimalLayerTime") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int MinimalFeedrate
    {
      get
      {
        if (GetSettingsItem("minimalFeedrate") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("minimalFeedrate") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private float CoolHeadLift
    {
      get
      {
        if (GetSettingsItem("coolHeadLift") is SettingsItemFloatSecondsType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("coolHeadLift") as SettingsItemFloatSecondsType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int FanSpeedMin
    {
      get
      {
        if (GetSettingsItem("fanSpeedMin") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("fanSpeedMin") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int FanSpeedMax
    {
      get
      {
        if (GetSettingsItem("fanSpeedMax") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("fanSpeedMax") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private bool EnableOozeShield
    {
      get
      {
        if (GetSettingsItem("enableOozeShield") is SettingsItemBoolType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("enableOozeShield") as SettingsItemBoolType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int WipeTowerSize
    {
      get
      {
        if (GetSettingsItem("wipeTowerSize") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("wipeTowerSize") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private int MultiVolumeOverlap
    {
      get
      {
        if (GetSettingsItem("multiVolumeOverlap") is SettingsItemIntType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("multiVolumeOverlap") as SettingsItemIntType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private string StartCode
    {
      get
      {
        if (GetSettingsItem("startCode") is SettingsItemStringType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("startCode") as SettingsItemStringType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    private string EndCode
    {
      get
      {
        if (GetSettingsItem("endCode") is SettingsItemStringType settingsItem)
        {
          return settingsItem.value;
        }

        throw new Exception("Slicer setting does not exist");
      }
      set
      {
        var settingsItem = GetSettingsItem("endCode") as SettingsItemStringType;
        if (settingsItem == null)
        {
          throw new Exception("Slicer setting does not exist");
        }

        settingsItem.value = value;
      }
    }

    public static float micronsToMM(int microns)
    {
      return microns / 1000f;
    }

    public static int millimetersToMicrons(float mm)
    {
      return (int)(mm * 1000.0);
    }

    public override string ConfigurationFileName
    {
      get
      {
        return string.Format("defaultCura15_04{0}.cfg", ProfileName);
      }
    }

    public override List<General.KeyValuePair<string, string>> GenerateUserKeyValuePairList()
    {
      var keyValuePairList = new List<General.KeyValuePair<string, string>>();
      foreach (System.Collections.Generic.KeyValuePair<string, SlicerSettingsItem> keyValuePair in this)
      {
        if (!(keyValuePair.Value is SettingsItemStringType))
        {
          keyValuePairList.Add(new General.KeyValuePair<string, string>(keyValuePair.Key, keyValuePair.Value.TranslateToUserValue()));
        }
      }
      return keyValuePairList;
    }

    public override void LoadFromUserKeyValuePairList(List<General.KeyValuePair<string, string>> list)
    {
      foreach (General.KeyValuePair<string, string> keyValuePair in list)
      {
        GetSettingsItem(keyValuePair.Key)?.ParseUserValue(keyValuePair.Value);
      }
    }

    public SlicerSettingsItem GetSettingsItem(string name)
    {
      if (ContainsKey(name))
      {
        return this[name];
      }

      return null;
    }

    public override void SerializeToSlicer(StreamWriter streamwriter)
    {
      streamwriter.WriteLine("# Generated by M3D Printer Software");
      foreach (System.Collections.Generic.KeyValuePair<string, SlicerSettingsItem> keyValuePair in this)
      {
        var str = string.Format("{0} = {1}", keyValuePair.Key, keyValuePair.Value.TranslateToSlicerValue());
        streamwriter.WriteLine(str);
      }
    }

    private string ExtractParameter(string line)
    {
      var num = line.IndexOf("=");
      if (num < 0)
      {
        throw new InvalidOperationException(SmartSlicerSettingsCura15_04.sErrorReading(line));
      }

      var startIndex = num + 1;
      while (startIndex < line.Length && line[startIndex] == ' ')
      {
        ++startIndex;
      }

      if (startIndex >= line.Length)
      {
        throw new InvalidOperationException(SmartSlicerSettingsCura15_04.sErrorReading(line));
      }

      return line.Substring(startIndex);
    }

    private string GetOption(string line)
    {
      var length = line.IndexOf(' ');
      if (length <= 0)
      {
        throw new InvalidOperationException(SmartSlicerSettingsCura15_04.sErrorReading(line));
      }

      return line.Substring(0, length);
    }

    private static string sErrorReading(string sLine)
    {
      return "Option without parameter: '" + sLine + "'";
    }

    private int nLocalSpeedLimit(SmartSlicerSettingsCura15_04.SpeedItem eWhichSpeed, int nProfileProposedSpeed)
    {
      var val2 = nProfileProposedSpeed;
      if ("Pro" == ProfileName)
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
      else if ("Micro+" == ProfileName)
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
