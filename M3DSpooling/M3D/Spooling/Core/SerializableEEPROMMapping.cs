// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Core.SerializableEEPROMMapping
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System.Collections.Generic;
using System.Xml.Serialization;

namespace M3D.Spooling.Core
{
  public class SerializableEEPROMMapping
  {
    [XmlAttribute("profileName")]
    public string profileName;
    public List<SerializableEEPROMMapping.SerializableData> serializedData;

    public SerializableEEPROMMapping(string profileName)
      : this()
    {
      this.profileName = profileName;
    }

    public SerializableEEPROMMapping()
    {
      this.serializedData = new List<SerializableEEPROMMapping.SerializableData>();
    }

    public struct SerializableData
    {
      [XmlAttribute("Value")]
      public string value;
      [XmlAttribute("Key")]
      public string key;
    }
  }
}
