using M3D.Boot;
using M3D.Spooling.Core;
using M3D.Spooling.Printer_Profiles;
using System;
using System.IO;

namespace M3D.Spooling.FirstRunUpdates
{
  internal class FirstRunUpdateResetOffsets : IFirstRunUpdater
  {
    public override bool CheckForUpdate(string serial_number, byte[] eeprom, Bootloader bootloader_conn, InternalPrinterProfile printerProfile)
    {
      try
      {
        var uint32 = (int) BitConverter.ToUInt32(eeprom, (int) printerProfile.EEPROMConstants.GetEepromInfo("FirmwareVersion").EepromAddr);
        var eepromAddr1 = (int) printerProfile.EEPROMConstants.GetEepromInfo("ZCalibrationBLO").EepromAddr;
        var eepromAddr2 = (int) printerProfile.EEPROMConstants.GetEepromInfo("ZCalibrationBRO").EepromAddr;
        var eepromAddr3 = (int) printerProfile.EEPROMConstants.GetEepromInfo("ZCalibrationFLO").EepromAddr;
        var eepromAddr4 = (int) printerProfile.EEPROMConstants.GetEepromInfo("ZCalibrationFRO").EepromAddr;
        var eepromAddr5 = (int) printerProfile.EEPROMConstants.GetEepromInfo("ZCalibrationZO").EepromAddr;
        var single1 = BitConverter.ToSingle(eeprom, eepromAddr1);
        var single2 = BitConverter.ToSingle(eeprom, eepromAddr2);
        var single3 = BitConverter.ToSingle(eeprom, eepromAddr3);
        var single4 = BitConverter.ToSingle(eeprom, eepromAddr4);
        var single5 = BitConverter.ToSingle(eeprom, eepromAddr5);
        var num1 = 0.0f;
        var num2 = 2015080402;
        if ((uint) uint32 < (uint) num2)
        {
          if ((double) Math.Abs(single1) > 1.40129846432482E-45 || (double) Math.Abs(single2) > 1.40129846432482E-45 || ((double) Math.Abs(single3) > 1.40129846432482E-45 || (double) Math.Abs(single4) > 1.40129846432482E-45) || (double) Math.Abs(single5) > 1.40129846432482E-45)
          {
            try
            {
              StreamWriter text = File.CreateText(Path.Combine(Paths.SpoolerFolder, "user_offsets-" + serial_number + ".txt"));
              text.WriteLine("Old Bed level offsets");
              text.WriteLine("  Back Left: " + single1.ToString("0.###"));
              text.WriteLine("  Back Right: " + single2.ToString("0.###"));
              text.WriteLine("  Front Left: " + single3.ToString("0.###"));
              text.WriteLine("  Front Right: " + single4.ToString("0.###"));
              text.WriteLine("  Z: " + single5.ToString("0.###"));
              text.Close();
            }
            catch (Exception ex)
            {
              ErrorLogger.LogErrorMsg("Exception in FirstRunUpdateResetOffsets.CheckForUpdate " + ex.Message, "Exception");
            }
          }
          bootloader_conn.WriteToEEPROM((ushort) eepromAddr1, BitConverter.GetBytes(num1));
          bootloader_conn.WriteToEEPROM((ushort) eepromAddr2, BitConverter.GetBytes(num1));
          bootloader_conn.WriteToEEPROM((ushort) eepromAddr3, BitConverter.GetBytes(num1));
          bootloader_conn.WriteToEEPROM((ushort) eepromAddr4, BitConverter.GetBytes(num1));
          bootloader_conn.WriteToEEPROM((ushort) eepromAddr5, BitConverter.GetBytes(num1));
        }
      }
      catch (Exception ex)
      {
        ErrorLogger.LogErrorMsg("Exception in FirstRunUpdateResetOffsets.CheckForUpdate " + ex.Message, "Exception");
        return false;
      }
      return true;
    }
  }
}
