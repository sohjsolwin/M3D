using M3D.Boot;
using M3D.Spooling.Core;
using M3D.Spooling.Printer_Profiles;
using System;

namespace M3D.Spooling.FirstRunUpdates
{
  internal class FirstRunUpdateSetBatchto500mA : IFirstRunUpdater
  {
    public override bool CheckForUpdate(string serial_number, byte[] eeprom, Bootloader bootloader_conn, InternalPrinterProfile printerProfile)
    {
      try
      {
        var str = serial_number.Substring(0, 13);
        var eepromAddr = (int) printerProfile.EEPROMConstants.GetEepromInfo("ExtruderCurrent").EepromAddr;
        if (!(str == "BK15033001100") && !(str == "BK15040201050") && (!(str == "BK15040301050") && !(str == "BK15040602050")) && (!(str == "BK15040801050") && !(str == "BK15040802100") && (!(str == "GR15032702100") && !(str == "GR15033101100"))) && (!(str == "GR15040601100") && !(str == "GR15040701100") && !(str == "OR15032701100")))
        {
          if (!(str == "SL15032601050"))
          {
            goto label_7;
          }
        }
        if (BitConverter.ToUInt16(eeprom, eepromAddr) != 500)
        {
          ushort num = 500;
          bootloader_conn.WriteToEEPROM((ushort) eepromAddr, BitConverter.GetBytes(num));
          eeprom = bootloader_conn.ReadAllReadableEEPROM();
          return BitConverter.ToUInt16(eeprom, eepromAddr) == 500;
        }
      }
      catch (Exception ex)
      {
        ErrorLogger.LogErrorMsg("Exception in FirstRunUpdateSetBatchto500ma.CheckForUpdate " + ex.Message, "Exception");
        return false;
      }
label_7:
      return true;
    }
  }
}
