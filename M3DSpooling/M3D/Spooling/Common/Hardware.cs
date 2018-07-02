using System.Collections.Generic;
using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public class Hardware
  {
    [XmlAttribute("PORT")]
    public string com_port;
    [XmlAttribute("MACHINE_TYPE")]
    public string machine_type;
    [XmlAttribute]
    public uint firmware_version;
    [XmlAttribute]
    public ushort LastResetCauseMask;
    [XmlAttribute]
    public string firmware_name;
    [XmlAttribute]
    public string firmware_url;
    [XmlAttribute]
    public string protocol_version;
    [XmlAttribute]
    public int extruder_count;
    [XmlAttribute]
    public int repetier_protocol;

    [XmlIgnore]
    public List<ResetCauseEnum> LastResetCause
    {
      get
      {
        var resetCauseEnumList = new List<ResetCauseEnum>();
        var num = 1;
        while (num <= 32)
        {
          var resetCauseEnum = (ResetCauseEnum) (num & LastResetCauseMask);
          if (resetCauseEnum != ResetCauseEnum.None)
          {
            resetCauseEnumList.Add(resetCauseEnum);
          }

          num <<= 1;
        }
        return resetCauseEnumList;
      }
    }

    public Hardware(Hardware rhs)
    {
      com_port = rhs.com_port;
      machine_type = rhs.machine_type;
      LastResetCauseMask = rhs.LastResetCauseMask;
      firmware_version = rhs.firmware_version;
      firmware_name = rhs.firmware_name;
      firmware_url = rhs.firmware_url;
      protocol_version = rhs.protocol_version;
      extruder_count = rhs.extruder_count;
      repetier_protocol = rhs.repetier_protocol;
    }

    public Hardware()
    {
      com_port = "";
      machine_type = "";
      LastResetCauseMask = 0;
      firmware_version = 0U;
      firmware_name = "unknown";
      firmware_url = "unknown";
      protocol_version = "unknown";
      extruder_count = 1;
      repetier_protocol = 2;
    }
  }
}
