using M3D.Spooling.ConnectionManager;
using System.Collections.Generic;

namespace M3D.Spooling.Printer_Profiles
{
  internal class PrinterProfileDictionary
  {
    private List<InternalPrinterProfile> m_profile_list;

    public PrinterProfileDictionary()
    {
      m_profile_list = new List<InternalPrinterProfile>
      {
        new Micro1PrinterProfile(),
        new ProPrinterProfile(),
        new MicroPlusPrinterProfile()
      };
    }

    public VID_PID[] GenerateVID_PID_List()
    {
      VID_PID[] vidPidArray = new VID_PID[m_profile_list.Count];
      for (var index = 0; index < vidPidArray.Length; ++index)
      {
        vidPidArray[index] = new VID_PID(m_profile_list[index].ProductConstants.VID, m_profile_list[index].ProductConstants.PID);
      }

      return vidPidArray;
    }

    public InternalPrinterProfile Get(VID_PID vid_pid)
    {
      foreach (InternalPrinterProfile profile in m_profile_list)
      {
        if ((int) profile.ProductConstants.VID == (int) vid_pid.VID && (int) profile.ProductConstants.PID == (int) vid_pid.PID)
        {
          return profile;
        }
      }
      return null;
    }

    public List<PrinterProfile> CreateProfileList()
    {
      var printerProfileList = new List<PrinterProfile>();
      foreach (InternalPrinterProfile profile in m_profile_list)
      {
        printerProfileList.Add(new PrinterProfile(profile));
      }

      return printerProfileList;
    }
  }
}
