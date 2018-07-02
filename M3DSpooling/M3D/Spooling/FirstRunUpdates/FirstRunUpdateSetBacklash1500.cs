// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.FirstRunUpdates.FirstRunUpdateSetBacklash1500
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Boot;
using M3D.Spooling.Core;
using M3D.Spooling.Printer_Profiles;
using System;

namespace M3D.Spooling.FirstRunUpdates
{
  internal class FirstRunUpdateSetBacklash1500 : IFirstRunUpdater
  {
    public override bool CheckForUpdate(string serial_number, byte[] eeprom, Bootloader bootloader_conn, InternalPrinterProfile printerProfile)
    {
      try
      {
        var uint32 = (int) BitConverter.ToUInt32(eeprom, (int) printerProfile.EEPROMConstants.GetEepromInfo("FirmwareVersion").EepromAddr);
        var num1 = 1500f;
        var num2 = 2015080402;
        if ((uint) uint32 < (uint) num2)
        {
          bootloader_conn.WriteToEEPROM(printerProfile.EEPROMConstants.GetEepromInfo("BacklashSpeed").EepromAddr, BitConverter.GetBytes(num1));
        }
      }
      catch (Exception ex)
      {
        ErrorLogger.LogErrorMsg("Exception in FirstRunUpdateSetBacklash1500.CheckForUpdate " + ex.Message, "Exception");
        return false;
      }
      return true;
    }
  }
}
