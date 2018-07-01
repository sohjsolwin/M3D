// Decompiled with JetBrains decompiler
// Type: M3D.Slicer.General.SmartSlicerSettingsBase
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      this.ProfileName = "";
      this.LoadSettingsItemsFromFile();
    }

    public SmartSlicerSettingsBase(SmartSlicerSettingsBase other)
    {
      this.ProfileName = other.ProfileName;
      this.xmlSettings = other.xmlSettings.Clone();
      this.BuildInternalStructures();
    }

    public int Count
    {
      get
      {
        return this.internalSettingsDictionary.Count;
      }
    }

    public bool ContainsKey(string key)
    {
      return this.internalSettingsDictionary.ContainsKey(key);
    }

    public SlicerSettingsItem this[string key]
    {
      get
      {
        return this.internalSettingsDictionary[key];
      }
    }

    public IEnumerator<System.Collections.Generic.KeyValuePair<string, SlicerSettingsItem>> GetEnumerator()
    {
      return internalSettingsDictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator) this.GetEnumerator();
    }

    public void LoadSettingsItemsFromFile()
    {
      this.xmlSettings = XMLSetting.Load((TextReader) new StringReader(this.AdvancedPrintSettingsEmbeddedResource));
      if (this.xmlSettings == null)
        throw new Exception("Misbehaving slicer advanced print settings config file");
      this.BuildInternalStructures();
    }

    private void BuildInternalStructures()
    {
      this.internalSettingsDictionary = new Dictionary<string, SlicerSettingsItem>((IEqualityComparer<string>) StringComparer.InvariantCultureIgnoreCase);
      List<XMLSettingsItem> allSettings = this.xmlSettings.GetAllSettings();
      foreach (XMLSettingsItem xmlSettingsItem1 in allSettings)
      {
        XMLSettingsItem setting = xmlSettingsItem1;
        if (setting.SlicerSettingsItem is SettingsItemBoolRPCType)
          ((SettingsItemBoolRPCType) setting.SlicerSettingsItem).SetParentSettings(this);
        if (!setting.Name.StartsWith(XMLSetting.MagicInternalString, StringComparison.InvariantCultureIgnoreCase))
          this.internalSettingsDictionary.Add(setting.Name, setting.SlicerSettingsItem);
        if (!string.IsNullOrEmpty(setting.GroupToggle))
        {
          XMLSettingsItem xmlSettingsItem2 = allSettings.Find((Predicate<XMLSettingsItem>) (item => item.Name == setting.GroupToggle));
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
        List<XMLTabCollectionSettingsItem> collectionSettingsItemList = new List<XMLTabCollectionSettingsItem>(this.xmlSettings.VisibleSettings.Count);
        foreach (XMLTabCollectionSettingsItem visibleSetting in this.xmlSettings.VisibleSettings)
          collectionSettingsItemList.Add(visibleSetting);
        return collectionSettingsItemList;
      }
    }

    public abstract string ConfigurationFileName { get; }

    public abstract string ProfileName { get; protected set; }
  }
}
