// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.FirstRunUpdates.IFirstRunUpdater
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
