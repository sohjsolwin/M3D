using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace M3D.SlicerConnectionCura.SlicerSettingsItems
{
  [Serializable]
  public class XMLTabCollectionSettingsItem
  {
    [XmlArray("SlicerSettings")]
    [XmlArrayItem("Setting")]
    public List<XMLSettingsItem> Items = new List<XMLSettingsItem>();
    [XmlAttribute]
    public string Header;

    public XMLTabCollectionSettingsItem Clone()
    {
      var collectionSettingsItem = new XMLTabCollectionSettingsItem
      {
        Header = Header
      };
      foreach (XMLSettingsItem xmlSettingsItem in Items)
      {
        collectionSettingsItem.Items.Add(xmlSettingsItem.Clone());
      }

      return collectionSettingsItem;
    }
  }
}
