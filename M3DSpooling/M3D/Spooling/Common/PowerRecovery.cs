// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.PowerRecovery
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
