// Decompiled with JetBrains decompiler
// Type: M3D.SlicerConnectionCura.SlicerSettingsItems.SlicerSettingsItem
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Slicer.General;
using System;
using System.Xml.Serialization;

namespace M3D.SlicerConnectionCura.SlicerSettingsItems
{
  public abstract class SlicerSettingsItem
  {
    [XmlIgnore]
    public bool ReverseGroupToggle;

    public SlicerSettingsItem()
    {
      this.GroupToggleSetting = (SlicerSettingsItem) null;
    }

    [XmlIgnore]
    public SlicerSettingsItem GroupToggleSetting { get; set; }

    [XmlIgnore]
    public bool IsSettingOn
    {
      get
      {
        if (this.GroupToggleSetting == null)
          return true;
        string userValue = this.GroupToggleSetting.TranslateToUserValue();
        bool flag = !userValue.Equals("false", StringComparison.InvariantCultureIgnoreCase) && !userValue.Equals("0");
        if (this.ReverseGroupToggle)
          return !flag;
        return flag;
      }
    }

    public SettingReadResult ReadSlicerSetting(string val)
    {
      return !this.SetFromSlicerValue(val) ? SettingReadResult.Failed : SettingReadResult.OK_Processed;
    }

    protected abstract bool SetFromSlicerValue(string val);

    protected virtual bool SetFromSlicerValueX(string val)
    {
      return this.SetFromSlicerValue(val);
    }

    protected virtual bool SetFromSlicerValueY(string val)
    {
      return this.SetFromSlicerValue(val);
    }

    public abstract SettingItemType GetItemType();

    public abstract string TranslateToSlicerValue();

    public virtual string TranslateToUserValue()
    {
      return this.TranslateToSlicerValue();
    }

    public virtual void ParseUserValue(string value)
    {
      this.SetFromSlicerValue(value);
    }

    public abstract bool HasWarning { get; }

    public abstract bool HasError { get; }

    public abstract string GetErrorMsg();

    public abstract SlicerSettingsItem Clone();
  }
}
