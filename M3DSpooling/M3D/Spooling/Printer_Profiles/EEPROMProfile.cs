using M3D.Spooling.Core;
using System.Collections.Generic;
using System.Diagnostics;

namespace M3D.Spooling.Printer_Profiles
{
  internal class EEPROMProfile
  {
    private List<EepromAddressInfo> addressList = new List<EepromAddressInfo>();
    public readonly int PrinterReadyIndex;
    public readonly string ProfileName;
    public readonly int BytesPerEEPROMAddress;
    public readonly ushort EndOfBootloaderReadableEEPROM;
    public readonly ushort EndOfFirmwareReadableEEPROM;

    public EEPROMProfile(string profileName, int printerReadyIndex, ushort endOfBootloaderReadableEEPROM, ushort endOfFirmwareReadableEEPROM, int bytesPerEEPROMAddress)
    {
      BytesPerEEPROMAddress = bytesPerEEPROMAddress;
      EndOfBootloaderReadableEEPROM = endOfBootloaderReadableEEPROM;
      EndOfFirmwareReadableEEPROM = endOfFirmwareReadableEEPROM;
      PrinterReadyIndex = printerReadyIndex;
      ProfileName = profileName;
    }

    public EepromAddressInfo GetEepromInfoFromLocation(int address)
    {
      foreach (EepromAddressInfo address1 in addressList)
      {
        if (address1.EepromAddr == address)
        {
          return address1;
        }
      }
      if (Debugger.IsAttached)
      {
        Debugger.Break();
      }

      return null;
    }

    public EepromAddressInfo GetEepromInfo(string name)
    {
      foreach (EepromAddressInfo address in addressList)
      {
        if (address.Name == name)
        {
          return address;
        }
      }
      return null;
    }

    public SortedList<int, EepromAddressInfo> GetAllData()
    {
      var sortedList = new SortedList<int, EepromAddressInfo>();
      foreach (EepromAddressInfo address in addressList)
      {
        sortedList.Add(address.EepromAddr, address);
      }

      return sortedList;
    }

    protected void AddEepromAddressInfo(EepromAddressInfo eepromInfo)
    {
      addressList.Add(eepromInfo);
    }
  }
}
