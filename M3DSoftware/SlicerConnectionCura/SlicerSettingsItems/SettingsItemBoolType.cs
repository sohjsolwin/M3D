// Decompiled with JetBrains decompiler
// Type: M3D.SlicerConnectionCura.SlicerSettingsItems.SettingsItemBoolType
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Slicer.General;
using System;
using System.Xml.Serialization;

namespace M3D.SlicerConnectionCura.SlicerSettingsItems
{
  [Serializable]
  public class SettingsItemBoolType : SlicerSettingsItem
  {
    [XmlAttribute]
    public bool value;

    public SettingsItemBoolType()
    {
      value = false;
    }

    public SettingsItemBoolType(bool _value)
    {
      value = _value;
    }

    protected override bool SetFromSlicerValue(string val)
    {
      value = !(val == "0");
      return true;
    }

    public override SettingItemType GetItemType()
    {
      return SettingItemType.BoolType;
    }

    public override string TranslateToSlicerValue()
    {
      return !value ? "0" : "1";
    }

    public override string TranslateToUserValue()
    {
      return !value ? "false" : "true";
    }

    public override void ParseUserValue(string value)
    {
      if (value.ToLowerInvariant() == "true")
      {
        this.value = true;
      }
      else
      {
        this.value = false;
      }
    }

    public override bool HasWarning
    {
      get
      {
        return false;
      }
    }

    public override bool HasError
    {
      get
      {
        return false;
      }
    }

    public override string GetErrorMsg()
    {
      return "";
    }

    public override SlicerSettingsItem Clone()
    {
      return (SlicerSettingsItem) new SettingsItemBoolType(value);
    }
  }
}
