using M3D.Boot;
using M3D.Spooling.Core;
using M3D.Spooling.Printer_Profiles;
using System;

namespace M3D.Spooling.FirstRunUpdates
{
  internal abstract class IFirstRunUpdater
  {
    public abstract bool CheckForUpdate(string serial_number, byte[] eeprom, Bootloader bootloader_conn, InternalPrinterProfile printerProfile);

    public int GetSerialDate(string serial_number)
    {
      var s = serial_number.Substring(2, 6);
      try
      {
        return int.Parse(s);
      }
      catch (Exception ex)
      {
        ErrorLogger.LogErrorMsg("Exception in IFirstRunUpdater.GetSerialDate " + ex.Message, "Exception");
        return 0;
      }
    }
  }
}
