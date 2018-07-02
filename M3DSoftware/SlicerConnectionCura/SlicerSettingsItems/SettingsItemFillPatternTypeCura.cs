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
      value = FillPaternCura.Automatic;
    }

    public SettingsItemFillPatternTypeCura(FillPaternCura _value)
      : base(typeof (FillPaternCura))
    {
      value = _value;
    }

    protected override bool SetFromSlicerValue(string val)
    {
      formatError = false;
      if (val == "Automatic" || val == "0")
      {
        value = FillPaternCura.Automatic;
      }
      else if (val == "Grid" || val == "1")
      {
        value = FillPaternCura.Grid;
      }
      else if (val == "Lines" || val == "2")
      {
        value = FillPaternCura.Lines;
      }
      else if (val == "Concentric" || val == "3")
      {
        value = FillPaternCura.Concentric;
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
      return SettingItemType.FillPatternType;
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
      if (val == "Automatic")
      {
        value = FillPaternCura.Automatic;
      }
      else if (val == "Grid")
      {
        value = FillPaternCura.Grid;
      }
      else if (val == "Lines")
      {
        value = FillPaternCura.Lines;
      }
      else if (val == "Concentric")
      {
        value = FillPaternCura.Concentric;
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
      return new SettingsItemFillPatternTypeCura(value);
    }
  }
}
