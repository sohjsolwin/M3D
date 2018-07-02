// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Utils.SerializableDictionary`2
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace M3D.Spooling.Common.Utils
{
  [XmlRoot("dictionary")]
  public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
  {
    public SerializableDictionary()
    {
    }

    public SerializableDictionary(Dictionary<TKey, TValue> other)
      : base((IDictionary<TKey, TValue>) other)
    {
    }

    public XmlSchema GetSchema()
    {
      return (XmlSchema) null;
    }

    public void ReadXml(XmlReader reader)
    {
      var xmlSerializer1 = new XmlSerializer(typeof (TKey));
      var xmlSerializer2 = new XmlSerializer(typeof (TValue));
      var num = reader.IsEmptyElement ? 1 : 0;
      reader.Read();
      if (num != 0)
      {
        return;
      }

      while (reader.NodeType != XmlNodeType.EndElement)
      {
        reader.ReadStartElement("item");
        reader.ReadStartElement("key");
        var key = (TKey) xmlSerializer1.Deserialize(reader);
        reader.ReadEndElement();
        reader.ReadStartElement("value");
        var obj = (TValue) xmlSerializer2.Deserialize(reader);
        reader.ReadEndElement();
        Add(key, obj);
        reader.ReadEndElement();
        var content = (int) reader.MoveToContent();
      }
      reader.ReadEndElement();
    }

    public void WriteXml(XmlWriter writer)
    {
      var xmlSerializer1 = new XmlSerializer(typeof (TKey));
      var xmlSerializer2 = new XmlSerializer(typeof (TValue));
      var namespaces = new XmlSerializerNamespaces();
      namespaces.Add(string.Empty, string.Empty);
      foreach (TKey key in Keys)
      {
        writer.WriteStartElement("item");
        writer.WriteStartElement("key");
        xmlSerializer1.Serialize(writer, (object) key, namespaces);
        writer.WriteEndElement();
        writer.WriteStartElement("value");
        TValue obj = this[key];
        xmlSerializer2.Serialize(writer, (object) obj);
        writer.WriteEndElement();
        writer.WriteEndElement();
      }
    }
  }
}
