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
        var uint32 = (int) BitConverter.ToUInt32(eeprom, printerProfile.EEPROMConstants.GetEepromInfo("FirmwareVersion").EepromAddr);
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
