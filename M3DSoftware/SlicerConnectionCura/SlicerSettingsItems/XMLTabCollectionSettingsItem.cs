// Decompiled with JetBrains decompiler
// Type: M3D.SlicerConnectionCura.SlicerSettingsItems.XMLTabCollectionSettingsItem
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      XMLTabCollectionSettingsItem collectionSettingsItem = new XMLTabCollectionSettingsItem();
      collectionSettingsItem.Header = this.Header;
      foreach (XMLSettingsItem xmlSettingsItem in this.Items)
        collectionSettingsItem.Items.Add(xmlSettingsItem.Clone());
      return collectionSettingsItem;
    }
  }
}
