// Decompiled with JetBrains decompiler
// Type: M3D.Slicer.General.KeyValuePair`2
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
