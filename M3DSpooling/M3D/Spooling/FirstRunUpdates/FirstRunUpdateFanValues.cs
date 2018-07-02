// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.FirstRunUpdates.FirstRunUpdateFanValues
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Boot;
using M3D.Spooling.Core;
using M3D.Spooling.Printer_Profiles;
using System;

namespace M3D.Spooling.FirstRunUpdates
{
  internal class FirstRunUpdateFanValues : IFirstRunUpdater
  {
    public override bool CheckForUpdate(string serial_number, byte[] eeprom, Bootloader bootloader_conn, InternalPrinterProfile printerProfile)
    {
      try
      {
        var num = eeprom[(int) printerProfile.EEPROMConstants.GetEepromInfo("FANTYPE").EepromAddr];
        var flag = false;
        FanConstValues.FanType index;
        FanConstValues.FanValues fanConstant;
        if (Enum.IsDefined(typeof (FanConstValues.FanType), (object) (int) num))
        {
          index = (FanConstValues.FanType) num;
          fanConstant = FanConstValues.FanConstants[index];
        }
        else
        {
          index = FanConstValues.FanType.None;
          fanConstant = FanConstValues.FanConstants[FanConstValues.FanType.HengLiXin];
        }
        var fanValues = new FanConstValues.FanValues
        {
          Scale = BitConverter.ToSingle(eeprom, (int)printerProfile.EEPROMConstants.GetEepromInfo("FANSCALE").EepromAddr),
          Offset = (int)eeprom[(int)printerProfile.EEPROMConstants.GetEepromInfo("FANOFFSET").EepromAddr]
        };
        if (Math.Abs(fanValues.Offset - fanConstant.Offset) >= 1)
        {
          flag = true;
        }

        if ((double) Math.Abs(fanValues.Scale - fanConstant.Scale) >= 1.40129846432482E-45)
        {
          flag = true;
        }

        if (flag)
        {
          bootloader_conn.WriteToEEPROM(printerProfile.EEPROMConstants.GetEepromInfo("FANTYPE").EepromAddr, BitConverter.GetBytes((short) (byte) index));
          bootloader_conn.WriteToEEPROM(printerProfile.EEPROMConstants.GetEepromInfo("FANOFFSET").EepromAddr, BitConverter.GetBytes((short) (byte) fanConstant.Offset));
          bootloader_conn.WriteToEEPROM(printerProfile.EEPROMConstants.GetEepromInfo("FANSCALE").EepromAddr, BitConverter.GetBytes(fanConstant.Scale));
        }
      }
      catch (Exception ex)
      {
        ErrorLogger.LogErrorMsg("Exception in FirstRunUpdateFanValues.CheckForUpdate " + ex.Message, "Exception");
        return false;
      }
      return true;
    }
  }
}
