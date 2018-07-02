// Decompiled with JetBrains decompiler
// Type: M3D.SlicerConnectionCura.SlicerSettingsItems.XMLSetting
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace M3D.SlicerConnectionCura.SlicerSettingsItems
{
  [Serializable]
  public class XMLSetting
  {
    [XmlArray("UserViewableSettings")]
    [XmlArrayItem("Tab")]
    public List<XMLTabCollectionSettingsItem> VisibleSettings = new List<XMLTabCollectionSettingsItem>();
    [XmlArray("InvisibleSlicerSettings")]
    [XmlArrayItem("Setting")]
    public List<XMLSettingsItem> InvisibleSettings = new List<XMLSettingsItem>();

    [XmlIgnore]
    public static string MagicInternalString
    {
      get
      {
        return "InternalToGUI_";
      }
    }

    public XMLSetting Clone()
    {
      var xmlSetting = new XMLSetting();
      foreach (XMLTabCollectionSettingsItem visibleSetting in VisibleSettings)
      {
        xmlSetting.VisibleSettings.Add(visibleSetting.Clone());
      }

      foreach (XMLSettingsItem invisibleSetting in InvisibleSettings)
      {
        xmlSetting.InvisibleSettings.Add(invisibleSetting.Clone());
      }

      return xmlSetting;
    }

    public static XMLSetting Load(string filePath)
    {
      try
      {
        return XMLSetting.Load((TextReader) File.OpenText(filePath));
      }
      catch (Exception ex)
      {
        if (ex is ThreadAbortException)
        {
          throw ex;
        }

        return (XMLSetting) null;
      }
    }

    public static XMLSetting Load(TextReader inputReader)
    {
      var xmlSetting = (XMLSetting) null;
      try
      {
        using (var xmlReader = XmlReader.Create(inputReader))
        {
          xmlSetting = (XMLSetting) new XmlSerializer(typeof (XMLSetting)).Deserialize(xmlReader);
        }
      }
      catch (Exception ex)
      {
        if (ex is ThreadAbortException)
        {
          throw ex;
        }

        return (XMLSetting) null;
      }
      var stringSet = new HashSet<string>();
      var crossCheckName = new HashSet<string>();
      for (var index = 0; index < xmlSetting.VisibleSettings.Count; ++index)
      {
        XMLTabCollectionSettingsItem visibleSetting = xmlSetting.VisibleSettings[index];
        var header = visibleSetting.Header;
        if (header == null)
        {
          throw new Exception("XMLSetting.UserViewableSettings contains a tab without a Header string!");
        }

        if (stringSet.Contains(header))
        {
          throw new Exception(string.Format("Tabs must have unique text! \"{0}\" has duplicates", (object) header));
        }

        stringSet.Add(header);
        XMLSetting.CheckNamesAndText_Helper(crossCheckName, visibleSetting.Items, true);
      }
      XMLSetting.CheckNamesAndText_Helper(crossCheckName, xmlSetting.InvisibleSettings, false);
      return xmlSetting;
    }

    private static void CheckNamesAndText_Helper(HashSet<string> crossCheckName, List<XMLSettingsItem> Items, bool TextRequired)
    {
      for (var index = 0; index < Items.Count; ++index)
      {
        var name = Items[index].Name;
        if (name == null)
        {
          throw new Exception("XMLSetting.UserViewableSettings.Setting is missing a Name string!");
        }

        if (TextRequired && Items[index].Text == null)
        {
          throw new Exception("XMLSetting.UserViewableSettings.Setting is missing a Text string!");
        }

        if (crossCheckName.Contains(name))
        {
          throw new Exception(string.Format("Setting names must be unique! \"{0}\" has duplicates", (object) name));
        }

        crossCheckName.Add(name);
      }
    }

    public List<XMLSettingsItem> GetAllSettings()
    {
      var count = InvisibleSettings.Count;
      for (var index = 0; index < VisibleSettings.Count; ++index)
      {
        count += VisibleSettings[index].Items.Count;
      }

      var xmlSettingsItemList = new List<XMLSettingsItem>(count);
      for (var index = 0; index < VisibleSettings.Count; ++index)
      {
        xmlSettingsItemList.AddRange((IEnumerable<XMLSettingsItem>)VisibleSettings[index].Items);
      }

      xmlSettingsItemList.AddRange((IEnumerable<XMLSettingsItem>)InvisibleSettings);
      return xmlSettingsItemList;
    }

    public List<XMLSettingsItem> GetAllNonGUISettings()
    {
      List<XMLSettingsItem> allSettings = GetAllSettings();
      allSettings.RemoveAll((Predicate<XMLSettingsItem>) (item => item.Name.StartsWith(XMLSetting.MagicInternalString, StringComparison.InvariantCultureIgnoreCase)));
      return allSettings;
    }

    internal List<XMLSettingsItem> GetGUIOnlySettings()
    {
      var xmlSettingsItemList = new List<XMLSettingsItem>();
      for (var index1 = 0; index1 < VisibleSettings.Count; ++index1)
      {
        for (var index2 = 0; index2 < VisibleSettings[index1].Items.Count; ++index2)
        {
          XMLSettingsItem xmlSettingsItem = VisibleSettings[index1].Items[index2];
          if (xmlSettingsItem.Name.StartsWith(XMLSetting.MagicInternalString, StringComparison.InvariantCultureIgnoreCase))
          {
            xmlSettingsItemList.Add(xmlSettingsItem);
          }
        }
      }
      return xmlSettingsItemList;
    }
  }
}
