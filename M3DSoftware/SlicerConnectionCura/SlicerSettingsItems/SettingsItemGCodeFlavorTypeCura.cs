// Decompiled with JetBrains decompiler
// Type: M3D.SlicerConnectionCura.SlicerSettingsItems.SettingsItemGCodeFlavorTypeCura
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      this.value = GCodeFlavorCura.Reprap;
    }

    public SettingsItemGCodeFlavorTypeCura(GCodeFlavorCura _value)
      : base(typeof (GCodeFlavorCura))
    {
      this.value = _value;
    }

    protected override bool SetFromSlicerValue(string val)
    {
      this.formatError = false;
      if (val == "Reprap" || val == "0")
        this.value = GCodeFlavorCura.Reprap;
      else if (val == "Ultigcode" || val == "1")
        this.value = GCodeFlavorCura.Ultigcode;
      else if (val == "MakerBot" || val == "2")
        this.value = GCodeFlavorCura.MakerBot;
      else if (val == "BFB" || val == "3")
        this.value = GCodeFlavorCura.BFB;
      else if (val == "Mach3" || val == "4")
        this.value = GCodeFlavorCura.Mach3;
      else if (val == "Volumetric" || val == "5")
      {
        this.value = GCodeFlavorCura.Volumetric;
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
      return SettingItemType.GCodeFlavorType;
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
      this.formatError = false;
      if (val == "Reprap")
        this.value = GCodeFlavorCura.Reprap;
      else if (val == "Ultigcode")
        this.value = GCodeFlavorCura.Ultigcode;
      else if (val == "MakerBot")
        this.value = GCodeFlavorCura.MakerBot;
      else if (val == "BFB")
        this.value = GCodeFlavorCura.BFB;
      else if (val == "Mach3")
        this.value = GCodeFlavorCura.Mach3;
      else if (val == "Volumetric")
        this.value = GCodeFlavorCura.Volumetric;
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
      return (SlicerSettingsItem) new SettingsItemGCodeFlavorTypeCura(this.value);
    }
  }
}
