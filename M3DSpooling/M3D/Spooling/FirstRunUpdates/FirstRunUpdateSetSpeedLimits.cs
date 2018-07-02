using M3D.Boot;
using M3D.Spooling.Printer_Profiles;
using System;

namespace M3D.Spooling.FirstRunUpdates
{
  internal class FirstRunUpdateSetSpeedLimits : IFirstRunUpdater
  {
    public override bool CheckForUpdate(string serial_number, byte[] eeprom, Bootloader bootloader_conn, InternalPrinterProfile printerProfile)
    {
      try
      {
        var eepromAddr1 = (int) printerProfile.EEPROMConstants.GetEepromInfo("SpeedLimitX").EepromAddr;
        var eepromAddr2 = (int) printerProfile.EEPROMConstants.GetEepromInfo("SpeedLimitY").EepromAddr;
        var eepromAddr3 = (int) printerProfile.EEPROMConstants.GetEepromInfo("SpeedLimitZ").EepromAddr;
        var eepromAddr4 = (int) printerProfile.EEPROMConstants.GetEepromInfo("SpeedLimitEp").EepromAddr;
        var eepromAddr5 = (int) printerProfile.EEPROMConstants.GetEepromInfo("SpeedLimitEn").EepromAddr;
        var single1 = BitConverter.ToSingle(eeprom, eepromAddr1);
        var single2 = BitConverter.ToSingle(eeprom, eepromAddr2);
        var single3 = BitConverter.ToSingle(eeprom, eepromAddr3);
        var single4 = BitConverter.ToSingle(eeprom, eepromAddr4);
        var single5 = BitConverter.ToSingle(eeprom, eepromAddr5);
        if (isNotValid(single1, printerProfile.SpeedLimitConstants.MIN_FEEDRATE_X, printerProfile.SpeedLimitConstants.MAX_FEEDRATE_X))
        {
          bootloader_conn.WriteToEEPROM((ushort) eepromAddr1, BitConverter.GetBytes(printerProfile.SpeedLimitConstants.DEFAULT_FEEDRATE_X));
        }

        if (isNotValid(single2, printerProfile.SpeedLimitConstants.MIN_FEEDRATE_Y, printerProfile.SpeedLimitConstants.MAX_FEEDRATE_Y))
        {
          bootloader_conn.WriteToEEPROM((ushort) eepromAddr2, BitConverter.GetBytes(printerProfile.SpeedLimitConstants.DEFAULT_FEEDRATE_Y));
        }

        if (isNotValid(single3, printerProfile.SpeedLimitConstants.MIN_FEEDRATE_Z, printerProfile.SpeedLimitConstants.MAX_FEEDRATE_Z))
        {
          bootloader_conn.WriteToEEPROM((ushort) eepromAddr3, BitConverter.GetBytes(printerProfile.SpeedLimitConstants.DEFAULT_FEEDRATE_Z));
        }

        if (isNotValid(single4, printerProfile.SpeedLimitConstants.MIN_FEEDRATE_E_Positive, printerProfile.SpeedLimitConstants.MAX_FEEDRATE_E_Positive))
        {
          bootloader_conn.WriteToEEPROM((ushort) eepromAddr4, BitConverter.GetBytes(printerProfile.SpeedLimitConstants.DEFAULT_FEEDRATE_E_Positive));
        }

        if (isNotValid(single5, printerProfile.SpeedLimitConstants.MIN_FEEDRATE_E_Negative, printerProfile.SpeedLimitConstants.MAX_FEEDRATE_E_Negative))
        {
          bootloader_conn.WriteToEEPROM((ushort) eepromAddr5, BitConverter.GetBytes(printerProfile.SpeedLimitConstants.DEFAULT_FEEDRATE_E_Negative));
        }
      }
      catch (Exception ex)
      {
        return false;
      }
      return true;
    }

    public bool isNotValid(float num, float min, float max)
    {
      if (!float.IsNaN(num) && (double) num >= (double) min)
      {
        return (double) num > (double) max;
      }

      return true;
    }
  }
}
