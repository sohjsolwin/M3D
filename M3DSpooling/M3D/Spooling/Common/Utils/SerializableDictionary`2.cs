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
      : base(other)
    {
    }

    public XmlSchema GetSchema()
    {
      return null;
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
        xmlSerializer1.Serialize(writer, key, namespaces);
        writer.WriteEndElement();
        writer.WriteStartElement("value");
        TValue obj = this[key];
        xmlSerializer2.Serialize(writer, obj);
        writer.WriteEndElement();
        writer.WriteEndElement();
      }
    }
  }
}
