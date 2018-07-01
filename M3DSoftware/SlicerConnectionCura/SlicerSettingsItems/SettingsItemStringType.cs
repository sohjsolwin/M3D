// Decompiled with JetBrains decompiler
// Type: M3D.SlicerConnectionCura.SlicerSettingsItems.SettingsItemStringType
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      this.value = "";
      this.multiline = false;
    }

    public SettingsItemStringType(string _value, bool multiline)
    {
      this.value = _value;
      this.multiline = multiline;
    }

    protected override bool SetFromSlicerValue(string val)
    {
      this.value = val;
      return true;
    }

    public override SettingItemType GetItemType()
    {
      return SettingItemType.StringType;
    }

    public override string TranslateToSlicerValue()
    {
      return "\"\"\"" + Environment.NewLine + this.value + "\"\"\"";
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
      return (SlicerSettingsItem) new SettingsItemStringType(this.value, this.isMultiline);
    }

    public bool isMultiline
    {
      get
      {
        return this.multiline;
      }
    }

    [XmlAttribute("Text")]
    public string xmlValue
    {
      get
      {
        return Regex.Escape(this.value);
      }
      set
      {
        this.value = Regex.Unescape(value);
      }
    }
  }
}
