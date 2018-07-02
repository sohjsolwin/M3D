using System;
using System.Xml.Serialization;

namespace M3D.SlicerConnectionCura.SlicerSettingsItems
{
  [Serializable]
  public class XMLSettingsItem
  {
    [XmlAttribute]
    public string Suffix = "";
    [XmlAttribute]
    public string Tooltip = "";
    [XmlAttribute]
    public string GroupToggle = "";
    [XmlAttribute]
    public string Name;
    [XmlAttribute]
    public string Text;
    [XmlAttribute]
    public bool ReverseGroupToggle;
    [XmlElement("BoolRPC", Type = typeof (SettingsItemBoolRPCType))]
    [XmlElement("Bool", Type = typeof (SettingsItemBoolType))]
    [XmlElement("Integer", Type = typeof (SettingsItemIntType))]
    [XmlElement("FloatMM", Type = typeof (SettingsItemFloatMMType))]
    [XmlElement("Seconds", Type = typeof (SettingsItemFloatSecondsType))]
    [XmlElement("String", Type = typeof (SettingsItemStringType))]
    [XmlElement("GCodeFlavorTypeCura", Type = typeof (SettingsItemGCodeFlavorTypeCura))]
    [XmlElement("FillPatternTypeCura", Type = typeof (SettingsItemFillPatternTypeCura))]
    [XmlElement("SupportTypeCura", Type = typeof (SettingsItemSupportPatternTypeCura))]
    public SlicerSettingsItem SlicerSettingsItem;

    public XMLSettingsItem Clone()
    {
      return new XMLSettingsItem()
      {
        Name = Name,
        Text = Text,
        Suffix = Suffix,
        Tooltip = Tooltip,
        GroupToggle = GroupToggle,
        ReverseGroupToggle = ReverseGroupToggle,
        SlicerSettingsItem = SlicerSettingsItem.Clone()
      };
    }
  }
}
