using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public class PowerRecovery
  {
    [XmlAttribute("RC")]
    public int iRC_Code;
    [XmlAttribute("RR")]
    public int iRR_Code;
    [XmlAttribute("PrintingStatus")]
    public PowerRecovery.PowerResetState PrintingStatus;

    [XmlIgnore]
    public bool bPowerOutageWhilePrinting
    {
      get
      {
        return (uint)PrintingStatus > 0U;
      }
    }

    public PowerRecovery(PowerRecovery rhs)
    {
      PrintingStatus = rhs.PrintingStatus;
      iRC_Code = rhs.iRC_Code;
      iRR_Code = rhs.iRR_Code;
    }

    public PowerRecovery()
    {
      PrintingStatus = PowerRecovery.PowerResetState.NotPrinting;
      iRC_Code = 0;
      iRR_Code = 0;
    }

    public enum PowerResetState
    {
      NotPrinting,
      PowerFailureSDPrint,
      PowerFailureStreamedPrint,
      Unknown,
    }
  }
}
