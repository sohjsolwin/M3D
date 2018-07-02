using System;
using System.Xml.Serialization;

namespace M3D.Slicer.General
{
  [XmlType(TypeName = "KeyValuePair")]
  [Serializable]
  public struct KeyValuePair<K, V>
  {
    public KeyValuePair(K key, V value)
    {
      this = new KeyValuePair<K, V>();
      Key = key;
      Value = value;
    }

    [XmlAttribute("Key")]
    public K Key { get; set; }

    [XmlAttribute("Value")]
    public V Value { get; set; }
  }
}
