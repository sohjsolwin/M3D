using M3D.Boot;
using M3D.Spooling.Core;
using M3D.Spooling.Printer_Profiles;
using System;

namespace M3D.Spooling.FirstRunUpdates
{
  internal class FirstRunUpdateDefaultFan : IFirstRunUpdater
  {
    public override bool CheckForUpdate(string serial_number, byte[] eeprom, Bootloader bootloader_conn, InternalPrinterProfile printerProfile)
    {
      try
      {
        var serialDate = GetSerialDate(serial_number);
        switch (eeprom[(int) printerProfile.EEPROMConstants.GetEepromInfo("FANTYPE").EepromAddr])
        {
          case 0:
          case byte.MaxValue:
            FanConstValues.FanType index = FanConstValues.FanType.HengLiXin;
            if (serialDate >= 150602)
            {
              index = FanConstValues.FanType.Shenzhew;
            }

            FanConstValues.FanValues fanConstant = FanConstValues.FanConstants[index];
            var num = (byte) index;
            var offset = fanConstant.Offset;
            var scale = fanConstant.Scale;
            bootloader_conn.WriteToEEPROM(printerProfile.EEPROMConstants.GetEepromInfo("FANTYPE").EepromAddr, BitConverter.GetBytes((short) num));
            bootloader_conn.WriteToEEPROM(printerProfile.EEPROMConstants.GetEepromInfo("FANOFFSET").EepromAddr, BitConverter.GetBytes(offset));
            bootloader_conn.WriteToEEPROM(printerProfile.EEPROMConstants.GetEepromInfo("FANSCALE").EepromAddr, BitConverter.GetBytes(scale));
            break;
        }
      }
      catch (Exception ex)
      {
        ErrorLogger.LogErrorMsg("Exception in FirstrunUpdateDefaulFan.CheckForUpdate " + ex.Message, "Exception");
        return false;
      }
      return true;
    }
  }
}
