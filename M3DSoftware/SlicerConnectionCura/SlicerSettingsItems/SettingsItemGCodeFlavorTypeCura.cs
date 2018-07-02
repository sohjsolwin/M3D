using M3D.Slicer.General;
using System;
using System.Xml.Serialization;

namespace M3D.SlicerConnectionCura.SlicerSettingsItems
{
  [Serializable]
  public class SettingsItemGCodeFlavorTypeCura : SlicerSettingsEnumItem
  {
    [XmlAttribute("Value")]
    public GCodeFlavorCura value;

    public SettingsItemGCodeFlavorTypeCura()
      : base(typeof (GCodeFlavorCura))
    {
      value = GCodeFlavorCura.Reprap;
    }

    public SettingsItemGCodeFlavorTypeCura(GCodeFlavorCura _value)
      : base(typeof (GCodeFlavorCura))
    {
      value = _value;
    }

    protected override bool SetFromSlicerValue(string val)
    {
      formatError = false;
      if (val == "Reprap" || val == "0")
      {
        value = GCodeFlavorCura.Reprap;
      }
      else if (val == "Ultigcode" || val == "1")
      {
        value = GCodeFlavorCura.Ultigcode;
      }
      else if (val == "MakerBot" || val == "2")
      {
        value = GCodeFlavorCura.MakerBot;
      }
      else if (val == "BFB" || val == "3")
      {
        value = GCodeFlavorCura.BFB;
      }
      else if (val == "Mach3" || val == "4")
      {
        value = GCodeFlavorCura.Mach3;
      }
      else if (val == "Volumetric" || val == "5")
      {
        value = GCodeFlavorCura.Volumetric;
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
      return SettingItemType.GCodeFlavorType;
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
      if (val == "Reprap")
      {
        value = GCodeFlavorCura.Reprap;
      }
      else if (val == "Ultigcode")
      {
        value = GCodeFlavorCura.Ultigcode;
      }
      else if (val == "MakerBot")
      {
        value = GCodeFlavorCura.MakerBot;
      }
      else if (val == "BFB")
      {
        value = GCodeFlavorCura.BFB;
      }
      else if (val == "Mach3")
      {
        value = GCodeFlavorCura.Mach3;
      }
      else if (val == "Volumetric")
      {
        value = GCodeFlavorCura.Volumetric;
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
      return (SlicerSettingsItem) new SettingsItemGCodeFlavorTypeCura(value);
    }
  }
}
