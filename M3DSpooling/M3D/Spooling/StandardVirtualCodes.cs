// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.StandardVirtualCodes
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;
using M3D.Spooling.Core;
using M3D.Spooling.Core.Controllers;
using RepetierHost.model;
using System;
using System.Collections.Generic;

namespace M3D.Spooling
{
  internal static class StandardVirtualCodes
  {
    public static void SetExtruderCurrent500(GCode gcode, FirmwareController connection)
    {
      StandardVirtualCodes.SendOK((BaseController) connection);
      connection.SetExtruderCurrent((ushort) 500);
    }

    public static void SetExtruderCurrent660(GCode gcode, FirmwareController connection)
    {
      StandardVirtualCodes.SendOK((BaseController) connection);
      connection.SetExtruderCurrent((ushort) 660);
    }

    public static void SetFanConstantsHeineken(GCode gcode, FirmwareController connection)
    {
      StandardVirtualCodes.SendOK((BaseController) connection);
      connection.SetFanConstants(FanConstValues.FanType.HengLiXin);
    }

    public static void SetFanConstantsListener(GCode gcode, FirmwareController connection)
    {
      StandardVirtualCodes.SendOK((BaseController) connection);
      connection.SetFanConstants(FanConstValues.FanType.Listener);
    }

    public static void SetFanConstantsShinZoo(GCode gcode, FirmwareController connection)
    {
      StandardVirtualCodes.SendOK((BaseController) connection);
      connection.SetFanConstants(FanConstValues.FanType.Shenzhew);
    }

    public static void SetFanConstantsXinyujie(GCode gcode, FirmwareController connection)
    {
      StandardVirtualCodes.SendOK((BaseController) connection);
      connection.SetFanConstants(FanConstValues.FanType.Xinyujie);
    }

    public static void M576GetFilamentInformation(GCode gcode, FirmwareController connection)
    {
      connection.ProcessFilamentDataFromEEPROM();
      FilamentSpool currentFilament = connection.GetCurrentFilament();
      connection.WriteLog(string.Format(">> ok S:{0} P:{1} T:{2} E:{3} I:{4} UID:{5}", (object) (int) currentFilament.filament_color_code, (object) currentFilament.filament_type, (object) (currentFilament.filament_temperature - 100), (object) currentFilament.estimated_filament_length_printed, (object) currentFilament.filament_size, (object) currentFilament.filament_uid), Logger.TextType.Read);
    }

    public static void M578GetCurrentBedOffsets(GCode gcode, FirmwareController connection)
    {
      connection.ProcessBedOffsetDataFromEEPROM();
      connection.ProcessCalibrationOffset();
      Calibration calibrationDetails = connection.CalibrationDetails;
      string text = string.Format(">> ok BRO:{0} BLO:{1} FRO:{2} FLO:{3} ZO:{4}", (object) calibrationDetails.CORNER_HEIGHT_BACK_RIGHT_OFFSET, (object) calibrationDetails.CORNER_HEIGHT_BACK_LEFT_OFFSET, (object) calibrationDetails.CORNER_HEIGHT_FRONT_RIGHT_OFFSET, (object) calibrationDetails.CORNER_HEIGHT_FRONT_LEFT_OFFSET, (object) calibrationDetails.ENTIRE_Z_HEIGHT_OFFSET);
      if (calibrationDetails.UsesCalibrationOffset)
        text = string.Format("{0} CO:{1}", (object) text, (object) calibrationDetails.CALIBRATION_OFFSET);
      connection.WriteLog(text, Logger.TextType.Read);
    }

    public static void M573GetBedLevelingValues(GCode gcode, FirmwareController connection)
    {
      connection.ProcessBedCompensationDataFromEEPROM();
      Calibration calibrationDetails = connection.CalibrationDetails;
      connection.WriteLog(string.Format(">> ok BR:{0} BL:{1} FR:{2} FL:{3} V:{4}", (object) calibrationDetails.CORNER_HEIGHT_BACK_RIGHT, (object) calibrationDetails.CORNER_HEIGHT_BACK_LEFT, (object) calibrationDetails.CORNER_HEIGHT_FRONT_RIGHT, (object) calibrationDetails.CORNER_HEIGHT_FRONT_LEFT, (object) calibrationDetails.G32_VERSION), Logger.TextType.Read);
    }

    public static void M572GetBackLash(GCode gcode, FirmwareController connection)
    {
      connection.ProcessBacklashEEPROM();
      Calibration calibrationDetails = connection.CalibrationDetails;
      connection.WriteLog(string.Format(">> ok BX:{0} BY:{1}", (object) calibrationDetails.BACKLASH_X, (object) calibrationDetails.BACKLASH_Y), Logger.TextType.Read);
    }

    public static void M581GetBackLashSpeed(GCode gcode, FirmwareController connection)
    {
      connection.ProcessBacklashSpeedEEPROM();
      Calibration calibrationDetails = connection.CalibrationDetails;
      connection.WriteLog(string.Format(">> ok BS:{0}", (object) calibrationDetails.BACKLASH_SPEED), Logger.TextType.Read);
    }

    public static void M580SetBackLashSpeed(GCode gcode, FirmwareController connection)
    {
      connection.SetBacklashSpeed(gcode.X);
      EepromAddressInfo eepromInfo = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("BacklashSpeed");
      int num = (int) connection.WriteManualCommands("M618 S" + (object) eepromInfo.EepromAddr + " P" + (object) EEPROMMapping.FloatToBinaryInt(gcode.X) + " T" + (object) eepromInfo.Size);
      StandardVirtualCodes.SendOK((BaseController) connection);
    }

    public static void M683GetLimitingSpeed(GCode gcode, FirmwareController connection)
    {
      string text = string.Format(">> ok X:{0} Y:{1} Z:{2} E:{3} R:{4}", (object) connection.FloatFromEEPROM("SpeedLimitX").ToString("0.00"), (object) connection.FloatFromEEPROM("SpeedLimitY").ToString("0.00"), (object) connection.FloatFromEEPROM("SpeedLimitZ").ToString("0.00"), (object) connection.FloatFromEEPROM("SpeedLimitEp").ToString("0.00"), (object) connection.FloatFromEEPROM("SpeedLimitEn").ToString("0.00"));
      connection.WriteLog(text, Logger.TextType.Read);
    }

    public static void M684PrintAllEepromValues(GCode gcode, FirmwareController connection)
    {
      SortedList<int, EepromAddressInfo> allData = connection.MyPrinterProfile.EEPROMConstants.GetAllData();
      string text = ">> ok ";
      foreach (EepromAddressInfo eepromAddressInfo in (IEnumerable<EepromAddressInfo>) allData.Values)
      {
        if (eepromAddressInfo.EepromAddr <= (ushort) 512)
        {
          text = text + eepromAddressInfo.Name + ": ";
          if (eepromAddressInfo.Type.Equals(typeof (float)))
            text = text + connection.FloatFromEEPROM(eepromAddressInfo.Name).ToString("0.00") + "\n";
          else if (eepromAddressInfo.Type.Equals(typeof (uint)) || eepromAddressInfo.Type.Equals(typeof (int)) || (eepromAddressInfo.Type.Equals(typeof (ushort)) || eepromAddressInfo.Type.Equals(typeof (short))) || eepromAddressInfo.Type.Equals(typeof (byte)))
          {
            text = text + connection.eeprom_mapping.GetUInt32(eepromAddressInfo.Name).ToString() + "\n";
          }
          else
          {
            if (!eepromAddressInfo.Type.Equals(typeof (char)))
              throw new Exception("Unexpected type");
            text = text + connection.eeprom_mapping.GetInt32(eepromAddressInfo.Name).ToString() + "\n";
          }
        }
      }
      connection.WriteLog(text, Logger.TextType.Read);
    }

    public static void M682SetLimitingSpeed(GCode gcode, FirmwareController connection)
    {
      EepromAddressInfo eepromInfo1 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("SpeedLimitX");
      EepromAddressInfo eepromInfo2 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("SpeedLimitY");
      EepromAddressInfo eepromInfo3 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("SpeedLimitZ");
      EepromAddressInfo eepromInfo4 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("SpeedLimitEp");
      EepromAddressInfo eepromInfo5 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("SpeedLimitEn");
      if (gcode.hasX)
      {
        int num1 = (int) connection.WriteManualCommands("M618 S" + (object) (int) eepromInfo1.EepromAddr + " P" + (object) EEPROMMapping.FloatToBinaryInt(gcode.X) + " T" + (object) eepromInfo1.Size);
      }
      if (gcode.hasY)
      {
        int num2 = (int) connection.WriteManualCommands("M618 S" + (object) (int) eepromInfo2.EepromAddr + " P" + (object) EEPROMMapping.FloatToBinaryInt(gcode.Y) + " T" + (object) eepromInfo2.Size);
      }
      if (gcode.hasZ)
      {
        int num3 = (int) connection.WriteManualCommands("M618 S" + (object) (int) eepromInfo3.EepromAddr + " P" + (object) EEPROMMapping.FloatToBinaryInt(gcode.Z) + " T" + (object) eepromInfo3.Size);
      }
      if (gcode.hasE)
      {
        int num4 = (int) connection.WriteManualCommands("M618 S" + (object) (int) eepromInfo4.EepromAddr + " P" + (object) EEPROMMapping.FloatToBinaryInt(gcode.E) + " T" + (object) eepromInfo4.Size);
      }
      if (gcode.hasR)
      {
        int num5 = (int) connection.WriteManualCommands("M618 S" + (object) (int) eepromInfo5.EepromAddr + " P" + (object) EEPROMMapping.FloatToBinaryInt(gcode.R) + " T" + (object) eepromInfo5.Size);
      }
      StandardVirtualCodes.SendOK((BaseController) connection);
    }

    public static void M571SetBacklashConstants(GCode gcode, FirmwareController connection)
    {
      EepromAddressInfo eepromInfo1 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("BacklashX");
      EepromAddressInfo eepromInfo2 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("BacklashY");
      connection.SetBacklash(gcode.X, gcode.Y);
      int num1 = (int) connection.WriteManualCommands("M618 S" + (object) eepromInfo1.EepromAddr + " P" + (object) EEPROMMapping.FloatToBinaryInt(gcode.X) + " T" + (object) eepromInfo1.Size);
      int num2 = (int) connection.WriteManualCommands("M618 S" + (object) eepromInfo2.EepromAddr + " P" + (object) EEPROMMapping.FloatToBinaryInt(gcode.Y) + " T" + (object) eepromInfo2.Size);
      StandardVirtualCodes.SendOK((BaseController) connection);
    }

    public static void M575SetFilament(GCode gcode, FirmwareController connection)
    {
      EepromAddressInfo eepromInfo1 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("FilamentColorID");
      EepromAddressInfo eepromInfo2 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("FilamentTypeID");
      EepromAddressInfo eepromInfo3 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("FilamentTemperature");
      EepromAddressInfo eepromInfo4 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("FilamentAmount");
      int num1 = (int) connection.WriteManualCommands("M618 S" + (object) eepromInfo1.EepromAddr + " P" + (object) gcode.S + " T" + (object) eepromInfo1.Size, "M618 S" + (object) eepromInfo2.EepromAddr + " P" + (object) gcode.P + " T" + (object) eepromInfo2.Size, "M618 S" + (object) eepromInfo3.EepromAddr + " P" + (object) gcode.T + " T" + (object) eepromInfo3.Size);
      int int32 = BitConverter.ToInt32(BitConverter.GetBytes(gcode.E), 0);
      int num2 = (int) connection.WriteManualCommands("M618 S" + (object) eepromInfo4.EepromAddr + " P" + (object) int32 + " T" + (object) eepromInfo4.Size);
      EepromAddressInfo eepromInfo5 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("FilamentSize");
      int num3 = (int) connection.WriteManualCommands("M618 S" + (object) eepromInfo5.EepromAddr + " P" + (object) gcode.I + " T" + (object) eepromInfo5.Size);
      StandardVirtualCodes.SendOK((BaseController) connection);
    }

    public static void M570SetFilamentUID(GCode gcode, FirmwareController connection)
    {
      EepromAddressInfo eepromInfo = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("FilamentUID");
      connection.AddManualCommandToFront("M618 S" + (object) eepromInfo.EepromAddr + " P" + (object) gcode.P + " T" + (object) eepromInfo.Size);
      StandardVirtualCodes.SendOK((BaseController) connection);
    }

    public static void M577SetBedOffsets(GCode gcode, FirmwareController connection)
    {
      EepromAddressInfo eepromInfo1 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("ZCalibrationBLO");
      EepromAddressInfo eepromInfo2 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("ZCalibrationBRO");
      EepromAddressInfo eepromInfo3 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("ZCalibrationFRO");
      EepromAddressInfo eepromInfo4 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("ZCalibrationFLO");
      EepromAddressInfo eepromInfo5 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("ZCalibrationZO");
      List<string> stringList = new List<string>();
      if (gcode.hasX && eepromInfo1 != null)
      {
        int int32 = BitConverter.ToInt32(BitConverter.GetBytes(gcode.X), 0);
        stringList.Add("M618 S" + (object) eepromInfo1.EepromAddr + " P" + (object) int32 + " T" + (object) eepromInfo1.Size);
      }
      if (gcode.hasY && eepromInfo2 != null)
      {
        int int32 = BitConverter.ToInt32(BitConverter.GetBytes(gcode.Y), 0);
        stringList.Add("M618 S" + (object) eepromInfo2.EepromAddr + " P" + (object) int32 + " T" + (object) eepromInfo2.Size);
      }
      if (gcode.hasZ && eepromInfo3 != null)
      {
        int int32 = BitConverter.ToInt32(BitConverter.GetBytes(gcode.Z), 0);
        stringList.Add("M618 S" + (object) eepromInfo3.EepromAddr + " P" + (object) int32 + " T" + (object) eepromInfo3.Size);
      }
      if (gcode.hasE && eepromInfo4 != null)
      {
        int int32 = BitConverter.ToInt32(BitConverter.GetBytes(gcode.E), 0);
        stringList.Add("M618 S" + (object) eepromInfo4.EepromAddr + " P" + (object) int32 + " T" + (object) eepromInfo4.Size);
      }
      if (gcode.hasF && eepromInfo5 != null)
      {
        int int32 = BitConverter.ToInt32(BitConverter.GetBytes(gcode.F), 0);
        stringList.Add("M618 S" + (object) eepromInfo5.EepromAddr + " P" + (object) int32 + " T" + (object) eepromInfo5.Size);
      }
      if (connection.CalibrationDetails.UsesCalibrationOffset)
      {
        EepromAddressInfo eepromInfo6 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("CalibrationOffset");
        if (gcode.hasI && eepromInfo6 != null)
        {
          int int32 = BitConverter.ToInt32(BitConverter.GetBytes(gcode.I), 0);
          stringList.Add("M618 S" + (object) eepromInfo6.EepromAddr + " P" + (object) int32 + " T" + (object) eepromInfo6.Size);
        }
      }
      if (stringList.Count > 0)
        connection.AddManualCommandToFront(stringList.ToArray());
      StandardVirtualCodes.SendOK((BaseController) connection);
    }

    public static void M582SetNozzleWidth(GCode gcode, FirmwareController connection)
    {
      EepromAddressInfo eepromInfo = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("NozzleSizeExtrusionWidth");
      if (eepromInfo == null)
        connection.WriteLog(string.Format(">> Error:Printer doesn't have adjustable nozzle."), Logger.TextType.Read);
      else
        connection.AddManualCommandToFront("M618 S" + (object) eepromInfo.EepromAddr + " P" + (object) gcode.S + " T" + (object) eepromInfo.Size);
      StandardVirtualCodes.SendOK((BaseController) connection);
    }

    public static void M583GetNozzleWidth(GCode gcode, FirmwareController connection)
    {
      connection.ProcessNozzleSizeExtrusionWidth();
      int nozzleSizeMicrons = connection.CurrentPrinterInfo.extruder.iNozzleSizeMicrons;
      connection.WriteLog(string.Format(">> ok S:{0}", (object) nozzleSizeMicrons), Logger.TextType.Read);
    }

    public static void M1011EnableBoundsChecking(GCode gcode, FirmwareController connection)
    {
      connection.BoundsCheckingEnabled = true;
    }

    public static void M1012DisableBoundsChecking(GCode gcode, FirmwareController connection)
    {
      connection.BoundsCheckingEnabled = false;
    }

    public static void M1013StopwatchStart(GCode gcode, FirmwareController connection)
    {
      connection.vStopwatchReset();
      StandardVirtualCodes.SendOK((BaseController) connection);
    }

    public static void M1014StopwatchStop(GCode gcode, FirmwareController connection)
    {
      long num = connection.nStopwatchReturnTime();
      if (0L <= num)
        connection.WriteLog(string.Format(">> ok M:{0}", (object) num), Logger.TextType.Read);
      else
        connection.WriteLog(">> stopwatch not running", Logger.TextType.Read);
    }

    public static void M303SetGantryClipsToOff(GCode gcode, FirmwareController connection)
    {
      connection.SetGantryClips(true);
      StandardVirtualCodes.SendOK((BaseController) connection);
    }

    public static void M304SetGantryClipsToOn(GCode gcode, FirmwareController connection)
    {
      connection.SetGantryClips(false);
      StandardVirtualCodes.SendOK((BaseController) connection);
    }

    public static void M5680GetHoursUsed(GCode gcode, FirmwareController connection)
    {
      float num = connection.PersistantDetails.hours_used / 6f;
      connection.WriteLog(">> ok H:" + num.ToString((IFormatProvider) PrinterCompatibleString.PRINTER_CULTURE), Logger.TextType.Error);
    }

    private static void SendOK(BaseController controller)
    {
      controller.WriteLog(string.Format(">> ok"), Logger.TextType.Read);
    }
  }
}
