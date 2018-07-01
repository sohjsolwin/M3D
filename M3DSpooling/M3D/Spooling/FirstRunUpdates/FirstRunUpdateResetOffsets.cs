// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.FirstRunUpdates.FirstRunUpdateResetOffsets
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
        int uint32 = (int) BitConverter.ToUInt32(eeprom, (int) printerProfile.EEPROMConstants.GetEepromInfo("FirmwareVersion").EepromAddr);
        int eepromAddr1 = (int) printerProfile.EEPROMConstants.GetEepromInfo("ZCalibrationBLO").EepromAddr;
        int eepromAddr2 = (int) printerProfile.EEPROMConstants.GetEepromInfo("ZCalibrationBRO").EepromAddr;
        int eepromAddr3 = (int) printerProfile.EEPROMConstants.GetEepromInfo("ZCalibrationFLO").EepromAddr;
        int eepromAddr4 = (int) printerProfile.EEPROMConstants.GetEepromInfo("ZCalibrationFRO").EepromAddr;
        int eepromAddr5 = (int) printerProfile.EEPROMConstants.GetEepromInfo("ZCalibrationZO").EepromAddr;
        float single1 = BitConverter.ToSingle(eeprom, eepromAddr1);
        float single2 = BitConverter.ToSingle(eeprom, eepromAddr2);
        float single3 = BitConverter.ToSingle(eeprom, eepromAddr3);
        float single4 = BitConverter.ToSingle(eeprom, eepromAddr4);
        float single5 = BitConverter.ToSingle(eeprom, eepromAddr5);
        float num1 = 0.0f;
        int num2 = 2015080402;
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
