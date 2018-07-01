// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.FirstRunUpdates.FirstRunUpdateDefaultFan
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
        int serialDate = this.GetSerialDate(serial_number);
        switch (eeprom[(int) printerProfile.EEPROMConstants.GetEepromInfo("FANTYPE").EepromAddr])
        {
          case 0:
          case byte.MaxValue:
            FanConstValues.FanType index = FanConstValues.FanType.HengLiXin;
            if (serialDate >= 150602)
              index = FanConstValues.FanType.Shenzhew;
            FanConstValues.FanValues fanConstant = FanConstValues.FanConstants[index];
            byte num = (byte) index;
            int offset = fanConstant.Offset;
            float scale = fanConstant.Scale;
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
