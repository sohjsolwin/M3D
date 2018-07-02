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
      serializedData = new List<SerializableEEPROMMapping.SerializableData>();
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
