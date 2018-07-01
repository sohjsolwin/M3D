// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Hardware
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
        List<ResetCauseEnum> resetCauseEnumList = new List<ResetCauseEnum>();
        int num = 1;
        while (num <= 32)
        {
          ResetCauseEnum resetCauseEnum = (ResetCauseEnum) (num & (int) this.LastResetCauseMask);
          if (resetCauseEnum != ResetCauseEnum.None)
            resetCauseEnumList.Add(resetCauseEnum);
          num <<= 1;
        }
        return resetCauseEnumList;
      }
    }

    public Hardware(Hardware rhs)
    {
      this.com_port = rhs.com_port;
      this.machine_type = rhs.machine_type;
      this.LastResetCauseMask = rhs.LastResetCauseMask;
      this.firmware_version = rhs.firmware_version;
      this.firmware_name = rhs.firmware_name;
      this.firmware_url = rhs.firmware_url;
      this.protocol_version = rhs.protocol_version;
      this.extruder_count = rhs.extruder_count;
      this.repetier_protocol = rhs.repetier_protocol;
    }

    public Hardware()
    {
      this.com_port = "";
      this.machine_type = "";
      this.LastResetCauseMask = (ushort) 0;
      this.firmware_version = 0U;
      this.firmware_name = "unknown";
      this.firmware_url = "unknown";
      this.protocol_version = "unknown";
      this.extruder_count = 1;
      this.repetier_protocol = 2;
    }
  }
}
