// Decompiled with JetBrains decompiler
// Type: M3D.SlicerConnectionCura.SlicerSettingsItems.SettingsItemSupportPatternTypeCura
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Slicer.General;
using System;
using System.Xml.Serialization;

namespace M3D.SlicerConnectionCura.SlicerSettingsItems
{
  [Serializable]
  public class SettingsItemSupportPatternTypeCura : SlicerSettingsEnumItem
  {
    [XmlAttribute]
    public SupportPatternCura value;

    public SettingsItemSupportPatternTypeCura()
      : base(typeof (SupportPatternCura))
    {
      value = SupportPatternCura.Lines;
    }

    public SettingsItemSupportPatternTypeCura(SupportPatternCura _value)
      : base(typeof (SupportPatternCura))
    {
      value = _value;
    }

    protected override bool SetFromSlicerValue(string val)
    {
      formatError = false;
      if (val == "Grid" || val == "0")
      {
        value = SupportPatternCura.Grid;
      }
      else if (val == "Lines" || val == "1")
      {
        value = SupportPatternCura.Lines;
      }
      else
      {
        formatError = true;
        return false;
      }
      return true;
    }

    public override SettingItemType GetItemType()
    {
      return SettingItemType.SupportPatternType;
    }

    public override string TranslateToSlicerValue()
    {
      return ((int)value).ToString();
    }

    public override string TranslateToUserValue()
    {
      return value.ToString();
    }

    public override void ParseUserValue(string val)
    {
      formatError = false;
      if (val == "Grid")
      {
        value = SupportPatternCura.Grid;
      }
      else if (val == "Lines")
      {
        value = SupportPatternCura.Lines;
      }
      else
      {
        formatError = true;
      }
    }

    public override bool HasWarning
    {
      get
      {
        return false;
      }
    }

    public override SlicerSettingsItem Clone()
    {
      return (SlicerSettingsItem) new SettingsItemSupportPatternTypeCura(value);
    }
  }
}
