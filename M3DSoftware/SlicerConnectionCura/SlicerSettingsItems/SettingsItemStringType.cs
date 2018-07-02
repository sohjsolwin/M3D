using M3D.Slicer.General;
using System;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace M3D.SlicerConnectionCura.SlicerSettingsItems
{
  [Serializable]
  public class SettingsItemStringType : SlicerSettingsItem
  {
    [XmlIgnore]
    public string value;
    private bool multiline;

    public SettingsItemStringType()
    {
      value = "";
      multiline = false;
    }

    public SettingsItemStringType(string _value, bool multiline)
    {
      value = _value;
      this.multiline = multiline;
    }

    protected override bool SetFromSlicerValue(string val)
    {
      value = val;
      return true;
    }

    public override SettingItemType GetItemType()
    {
      return SettingItemType.StringType;
    }

    public override string TranslateToSlicerValue()
    {
      return "\"\"\"" + Environment.NewLine + value + "\"\"\"";
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
      return (SlicerSettingsItem) new SettingsItemStringType(value, isMultiline);
    }

    public bool isMultiline
    {
      get
      {
        return multiline;
      }
    }

    [XmlAttribute("Text")]
    public string xmlValue
    {
      get
      {
        return Regex.Escape(value);
      }
      set
      {
        this.value = Regex.Unescape(value);
      }
    }
  }
}
