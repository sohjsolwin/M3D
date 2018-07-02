using M3D.SlicerConnectionCura.SlicerSettingsItems;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace M3D.Slicer.General
{
  public abstract class SmartSlicerSettingsBase : IEnumerable<System.Collections.Generic.KeyValuePair<string, SlicerSettingsItem>>, IEnumerable
  {
    private Dictionary<string, SlicerSettingsItem> internalSettingsDictionary;
    private XMLSetting xmlSettings;

    public SmartSlicerSettingsBase()
    {
      ProfileName = "";
      LoadSettingsItemsFromFile();
    }

    public SmartSlicerSettingsBase(SmartSlicerSettingsBase other)
    {
      ProfileName = other.ProfileName;
      xmlSettings = other.xmlSettings.Clone();
      BuildInternalStructures();
    }

    public int Count
    {
      get
      {
        return internalSettingsDictionary.Count;
      }
    }

    public bool ContainsKey(string key)
    {
      return internalSettingsDictionary.ContainsKey(key);
    }

    public SlicerSettingsItem this[string key]
    {
      get
      {
        return internalSettingsDictionary[key];
      }
    }

    public IEnumerator<System.Collections.Generic.KeyValuePair<string, SlicerSettingsItem>> GetEnumerator()
    {
      return internalSettingsDictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public void LoadSettingsItemsFromFile()
    {
      xmlSettings = XMLSetting.Load(new StringReader(AdvancedPrintSettingsEmbeddedResource));
      if (xmlSettings == null)
      {
        throw new Exception("Misbehaving slicer advanced print settings config file");
      }

      BuildInternalStructures();
    }

    private void BuildInternalStructures()
    {
      internalSettingsDictionary = new Dictionary<string, SlicerSettingsItem>(StringComparer.InvariantCultureIgnoreCase);
      List<XMLSettingsItem> allSettings = xmlSettings.GetAllSettings();
      foreach (XMLSettingsItem xmlSettingsItem1 in allSettings)
      {
        XMLSettingsItem setting = xmlSettingsItem1;
        if (setting.SlicerSettingsItem is SettingsItemBoolRPCType)
        {
          ((SettingsItemBoolRPCType) setting.SlicerSettingsItem).SetParentSettings(this);
        }

        if (!setting.Name.StartsWith(XMLSetting.MagicInternalString, StringComparison.InvariantCultureIgnoreCase))
        {
          internalSettingsDictionary.Add(setting.Name, setting.SlicerSettingsItem);
        }

        if (!string.IsNullOrEmpty(setting.GroupToggle))
        {
          XMLSettingsItem xmlSettingsItem2 = allSettings.Find(item => item.Name == setting.GroupToggle);
          if (xmlSettingsItem2 != null)
          {
            setting.SlicerSettingsItem.GroupToggleSetting = xmlSettingsItem2.SlicerSettingsItem;
            setting.SlicerSettingsItem.ReverseGroupToggle = setting.ReverseGroupToggle;
          }
        }
      }
    }

    public abstract bool ParseFile(string filename);

    public abstract string AdvancedPrintSettingsEmbeddedResource { get; }

    public abstract SmartSlicerSettingsBase Clone();

    public abstract void ConfigureFromPrinterData(IPrinter oPrinter);

    public abstract void SetPrintQuality(PrintQuality PrintQuality, FilamentSpool filament, int iModelCount);

    public abstract void SetFillQuality(FillQuality FillQuality);

    public abstract void SetToDefault();

    public abstract void EnableRaft(FilamentSpool filament);

    public abstract void DisableRaft();

    public abstract bool HasRaftEnabled { get; }

    public abstract void EnableSkirt();

    public abstract void DisableSkirt();

    public abstract bool HasSkirt { get; }

    public abstract void EnableSupport(SupportType supportType);

    public abstract void DisableSupport();

    public abstract bool HasSupport { get; }

    public abstract bool HasModelonModelSupport { get; }

    public abstract void EnableAutoFanSettings();

    public abstract void DisableAutoFanSettings();

    public abstract bool UsingAutoFanSettings { get; }

    public abstract bool UsingNozzleSizeForExtrusionWidth { get; }

    public abstract bool CustomNozzleAvailable { get; }

    public abstract bool UsingCustomExtrusionWidth { get; }

    public abstract void EnableUseNozzleSizeForExtrusionWidth();

    public abstract void DisableUseNozzleSizeForExtrusionWidth();

    public abstract void SmartCheckBoxCallBack(string checkBoxName, bool state, FilamentSpool filament);

    public abstract List<KeyValuePair<string, string>> GenerateUserKeyValuePairList();

    public abstract void LoadFromUserKeyValuePairList(List<KeyValuePair<string, string>> list);

    public abstract void SerializeToSlicer(StreamWriter writer);

    public abstract PrintQuality CurrentPrintQuality { get; }

    public abstract FillQuality CurrentFillQuality { get; }

    public abstract List<PrintQuality> SupportedPrintQualities { get; }

    public abstract List<FillQuality> SupportedFillQualities { get; }

    public List<XMLTabCollectionSettingsItem> VisualSettings
    {
      get
      {
        var collectionSettingsItemList = new List<XMLTabCollectionSettingsItem>(xmlSettings.VisibleSettings.Count);
        foreach (XMLTabCollectionSettingsItem visibleSetting in xmlSettings.VisibleSettings)
        {
          collectionSettingsItemList.Add(visibleSetting);
        }

        return collectionSettingsItemList;
      }
    }

    public abstract string ConfigurationFileName { get; }

    public abstract string ProfileName { get; protected set; }
  }
}
