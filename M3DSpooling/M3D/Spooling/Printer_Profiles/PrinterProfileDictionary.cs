// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Printer_Profiles.PrinterProfileDictionary
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.ConnectionManager;
using System.Collections.Generic;

namespace M3D.Spooling.Printer_Profiles
{
  internal class PrinterProfileDictionary
  {
    private List<InternalPrinterProfile> m_profile_list;

    public PrinterProfileDictionary()
    {
      this.m_profile_list = new List<InternalPrinterProfile>();
      this.m_profile_list.Add((InternalPrinterProfile) new Micro1PrinterProfile());
      this.m_profile_list.Add((InternalPrinterProfile) new ProPrinterProfile());
      this.m_profile_list.Add((InternalPrinterProfile) new MicroPlusPrinterProfile());
    }

    public VID_PID[] GenerateVID_PID_List()
    {
      VID_PID[] vidPidArray = new VID_PID[this.m_profile_list.Count];
      for (int index = 0; index < vidPidArray.Length; ++index)
        vidPidArray[index] = new VID_PID(this.m_profile_list[index].ProductConstants.VID, this.m_profile_list[index].ProductConstants.PID);
      return vidPidArray;
    }

    public InternalPrinterProfile Get(VID_PID vid_pid)
    {
      foreach (InternalPrinterProfile profile in this.m_profile_list)
      {
        if ((int) profile.ProductConstants.VID == (int) vid_pid.VID && (int) profile.ProductConstants.PID == (int) vid_pid.PID)
          return profile;
      }
      return (InternalPrinterProfile) null;
    }

    public List<PrinterProfile> CreateProfileList()
    {
      List<PrinterProfile> printerProfileList = new List<PrinterProfile>();
      foreach (InternalPrinterProfile profile in this.m_profile_list)
        printerProfileList.Add(new PrinterProfile((PrinterProfile) profile));
      return printerProfileList;
    }
  }
}
