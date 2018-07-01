// Decompiled with JetBrains decompiler
// Type: M3D.SlicerConnectionCura.SlicerSettingsItems.SettingsItemFillPatternTypeCura
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Slicer.General;
using System;
using System.Xml.Serialization;

namespace M3D.SlicerConnectionCura.SlicerSettingsItems
{
  [Serializable]
  public class SettingsItemFillPatternTypeCura : SlicerSettingsEnumItem
  {
    [XmlAttribute("Value")]
    public FillPaternCura value;

    public SettingsItemFillPatternTypeCura()
      : base(typeof (FillPaternCura))
    {
      this.value = FillPaternCura.Automatic;
    }

    public SettingsItemFillPatternTypeCura(FillPaternCura _value)
      : base(typeof (FillPaternCura))
    {
      this.value = _value;
    }

    protected override bool SetFromSlicerValue(string val)
    {
      this.formatError = false;
      if (val == "Automatic" || val == "0")
        this.value = FillPaternCura.Automatic;
      else if (val == "Grid" || val == "1")
        this.value = FillPaternCura.Grid;
      else if (val == "Lines" || val == "2")
        this.value = FillPaternCura.Lines;
      else if (val == "Concentric" || val == "3")
      {
        this.value = FillPaternCura.Concentric;
      }
      else
      {
        this.formatError = true;
        return false;
      }
      return true;
    }

    public override SettingItemType GetItemType()
    {
      return SettingItemType.FillPatternType;
    }

    public override string TranslateToSlicerValue()
    {
      return ((int) this.value).ToString();
    }

    public override string TranslateToUserValue()
    {
      return this.value.ToString();
    }

    public override void ParseUserValue(string val)
    {
      if (val == "Automatic")
        this.value = FillPaternCura.Automatic;
      else if (val == "Grid")
        this.value = FillPaternCura.Grid;
      else if (val == "Lines")
        this.value = FillPaternCura.Lines;
      else if (val == "Concentric")
        this.value = FillPaternCura.Concentric;
      else
        this.formatError = true;
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
      return (SlicerSettingsItem) new SettingsItemFillPatternTypeCura(this.value);
    }
  }
}
