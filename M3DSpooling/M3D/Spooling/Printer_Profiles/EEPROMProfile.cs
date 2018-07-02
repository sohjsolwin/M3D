// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Printer_Profiles.EEPROMProfile
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
        if ((int) address1.EepromAddr == address)
        {
          return address1;
        }
      }
      if (Debugger.IsAttached)
      {
        Debugger.Break();
      }

      return (EepromAddressInfo) null;
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
      return (EepromAddressInfo) null;
    }

    public SortedList<int, EepromAddressInfo> GetAllData()
    {
      var sortedList = new SortedList<int, EepromAddressInfo>();
      foreach (EepromAddressInfo address in addressList)
      {
        sortedList.Add((int) address.EepromAddr, address);
      }

      return sortedList;
    }

    protected void AddEepromAddressInfo(EepromAddressInfo eepromInfo)
    {
      addressList.Add(eepromInfo);
    }
  }
}
