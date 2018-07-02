using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public class Synchronization
  {
    [XmlAttribute("LastCompletedRPCID")]
    public uint LastCompletedRPCID;
    [XmlAttribute("Locked")]
    public bool Locked;

    public Synchronization(Synchronization rhs)
    {
      LastCompletedRPCID = rhs.LastCompletedRPCID;
      Locked = rhs.Locked;
    }

    public Synchronization()
    {
      LastCompletedRPCID = 0U;
      Locked = false;
    }
  }
}
