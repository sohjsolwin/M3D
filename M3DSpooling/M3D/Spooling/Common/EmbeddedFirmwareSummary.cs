using System.Collections.Generic;

namespace M3D.Spooling.Common
{
  public class EmbeddedFirmwareSummary
  {
    public string Name;
    public List<FirmwareBoardVersionKVP> FirmwareVersions;

    public EmbeddedFirmwareSummary()
    {
      FirmwareVersions = new List<FirmwareBoardVersionKVP>();
    }

    public EmbeddedFirmwareSummary(string name)
      : this()
    {
      Name = name;
    }

    public override string ToString()
    {
      return "M3D " + Name;
    }
  }
}
