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
      StandardVirtualCodes.SendOK(connection);
      connection.SetExtruderCurrent(500);
    }

    public static void SetExtruderCurrent660(GCode gcode, FirmwareController connection)
    {
      StandardVirtualCodes.SendOK(connection);
      connection.SetExtruderCurrent(660);
    }

    public static void SetFanConstantsHeineken(GCode gcode, FirmwareController connection)
    {
      StandardVirtualCodes.SendOK(connection);
      connection.SetFanConstants(FanConstValues.FanType.HengLiXin);
    }

    public static void SetFanConstantsListener(GCode gcode, FirmwareController connection)
    {
      StandardVirtualCodes.SendOK(connection);
      connection.SetFanConstants(FanConstValues.FanType.Listener);
    }

    public static void SetFanConstantsShinZoo(GCode gcode, FirmwareController connection)
    {
      StandardVirtualCodes.SendOK(connection);
      connection.SetFanConstants(FanConstValues.FanType.Shenzhew);
    }

    public static void SetFanConstantsXinyujie(GCode gcode, FirmwareController connection)
    {
      StandardVirtualCodes.SendOK(connection);
      connection.SetFanConstants(FanConstValues.FanType.Xinyujie);
    }

    public static void M576GetFilamentInformation(GCode gcode, FirmwareController connection)
    {
      connection.ProcessFilamentDataFromEEPROM();
      FilamentSpool currentFilament = connection.GetCurrentFilament();
      connection.WriteLog(string.Format(">> ok S:{0} P:{1} T:{2} E:{3} I:{4} UID:{5}", (int)currentFilament.filament_color_code, currentFilament.filament_type, currentFilament.filament_temperature - 100, currentFilament.estimated_filament_length_printed, currentFilament.filament_size, currentFilament.filament_uid), Logger.TextType.Read);
    }

    public static void M578GetCurrentBedOffsets(GCode gcode, FirmwareController connection)
    {
      connection.ProcessBedOffsetDataFromEEPROM();
      connection.ProcessCalibrationOffset();
      Calibration calibrationDetails = connection.CalibrationDetails;
      var text = string.Format(">> ok BRO:{0} BLO:{1} FRO:{2} FLO:{3} ZO:{4}", calibrationDetails.CORNER_HEIGHT_BACK_RIGHT_OFFSET, calibrationDetails.CORNER_HEIGHT_BACK_LEFT_OFFSET, calibrationDetails.CORNER_HEIGHT_FRONT_RIGHT_OFFSET, calibrationDetails.CORNER_HEIGHT_FRONT_LEFT_OFFSET, calibrationDetails.ENTIRE_Z_HEIGHT_OFFSET);
      if (calibrationDetails.UsesCalibrationOffset)
      {
        text = string.Format("{0} CO:{1}", text, calibrationDetails.CALIBRATION_OFFSET);
      }

      connection.WriteLog(text, Logger.TextType.Read);
    }

    public static void M573GetBedLevelingValues(GCode gcode, FirmwareController connection)
    {
      connection.ProcessBedCompensationDataFromEEPROM();
      Calibration calibrationDetails = connection.CalibrationDetails;
      connection.WriteLog(string.Format(">> ok BR:{0} BL:{1} FR:{2} FL:{3} V:{4}", calibrationDetails.CORNER_HEIGHT_BACK_RIGHT, calibrationDetails.CORNER_HEIGHT_BACK_LEFT, calibrationDetails.CORNER_HEIGHT_FRONT_RIGHT, calibrationDetails.CORNER_HEIGHT_FRONT_LEFT, calibrationDetails.G32_VERSION), Logger.TextType.Read);
    }

    public static void M572GetBackLash(GCode gcode, FirmwareController connection)
    {
      connection.ProcessBacklashEEPROM();
      Calibration calibrationDetails = connection.CalibrationDetails;
      connection.WriteLog(string.Format(">> ok BX:{0} BY:{1}", calibrationDetails.BACKLASH_X, calibrationDetails.BACKLASH_Y), Logger.TextType.Read);
    }

    public static void M581GetBackLashSpeed(GCode gcode, FirmwareController connection)
    {
      connection.ProcessBacklashSpeedEEPROM();
      Calibration calibrationDetails = connection.CalibrationDetails;
      connection.WriteLog(string.Format(">> ok BS:{0}", calibrationDetails.BACKLASH_SPEED), Logger.TextType.Read);
    }

    public static void M580SetBackLashSpeed(GCode gcode, FirmwareController connection)
    {
      connection.SetBacklashSpeed(gcode.X);
      EepromAddressInfo eepromInfo = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("BacklashSpeed");
      var num = (int) connection.WriteManualCommands("M618 S" + eepromInfo.EepromAddr + " P" + EEPROMMapping.FloatToBinaryInt(gcode.X) + " T" + eepromInfo.Size);
      StandardVirtualCodes.SendOK(connection);
    }

    public static void M683GetLimitingSpeed(GCode gcode, FirmwareController connection)
    {
      var text = string.Format(">> ok X:{0} Y:{1} Z:{2} E:{3} R:{4}", connection.FloatFromEEPROM("SpeedLimitX").ToString("0.00"), connection.FloatFromEEPROM("SpeedLimitY").ToString("0.00"), connection.FloatFromEEPROM("SpeedLimitZ").ToString("0.00"), connection.FloatFromEEPROM("SpeedLimitEp").ToString("0.00"), connection.FloatFromEEPROM("SpeedLimitEn").ToString("0.00"));
      connection.WriteLog(text, Logger.TextType.Read);
    }

    public static void M684PrintAllEepromValues(GCode gcode, FirmwareController connection)
    {
      SortedList<int, EepromAddressInfo> allData = connection.MyPrinterProfile.EEPROMConstants.GetAllData();
      var text = ">> ok ";
      foreach (EepromAddressInfo eepromAddressInfo in allData.Values)
      {
        if (eepromAddressInfo.EepromAddr <= 512)
        {
          text = text + eepromAddressInfo.Name + ": ";
          if (eepromAddressInfo.Type.Equals(typeof (float)))
          {
            text = text + connection.FloatFromEEPROM(eepromAddressInfo.Name).ToString("0.00") + "\n";
          }
          else if (eepromAddressInfo.Type.Equals(typeof (uint)) || eepromAddressInfo.Type.Equals(typeof (int)) || (eepromAddressInfo.Type.Equals(typeof (ushort)) || eepromAddressInfo.Type.Equals(typeof (short))) || eepromAddressInfo.Type.Equals(typeof (byte)))
          {
            text = text + connection.eeprom_mapping.GetUInt32(eepromAddressInfo.Name).ToString() + "\n";
          }
          else
          {
            if (!eepromAddressInfo.Type.Equals(typeof (char)))
            {
              throw new Exception("Unexpected type");
            }

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
      if (gcode.HasX)
      {
        var num1 = (int) connection.WriteManualCommands("M618 S" + (int)eepromInfo1.EepromAddr + " P" + EEPROMMapping.FloatToBinaryInt(gcode.X) + " T" + eepromInfo1.Size);
      }
      if (gcode.HasY)
      {
        var num2 = (int) connection.WriteManualCommands("M618 S" + (int)eepromInfo2.EepromAddr + " P" + EEPROMMapping.FloatToBinaryInt(gcode.Y) + " T" + eepromInfo2.Size);
      }
      if (gcode.HasZ)
      {
        var num3 = (int) connection.WriteManualCommands("M618 S" + (int)eepromInfo3.EepromAddr + " P" + EEPROMMapping.FloatToBinaryInt(gcode.Z) + " T" + eepromInfo3.Size);
      }
      if (gcode.HasE)
      {
        var num4 = (int) connection.WriteManualCommands("M618 S" + (int)eepromInfo4.EepromAddr + " P" + EEPROMMapping.FloatToBinaryInt(gcode.E) + " T" + eepromInfo4.Size);
      }
      if (gcode.HasR)
      {
        var num5 = (int) connection.WriteManualCommands("M618 S" + (int)eepromInfo5.EepromAddr + " P" + EEPROMMapping.FloatToBinaryInt(gcode.R) + " T" + eepromInfo5.Size);
      }
      StandardVirtualCodes.SendOK(connection);
    }

    public static void M571SetBacklashConstants(GCode gcode, FirmwareController connection)
    {
      EepromAddressInfo eepromInfo1 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("BacklashX");
      EepromAddressInfo eepromInfo2 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("BacklashY");
      connection.SetBacklash(gcode.X, gcode.Y);
      var num1 = (int) connection.WriteManualCommands("M618 S" + eepromInfo1.EepromAddr + " P" + EEPROMMapping.FloatToBinaryInt(gcode.X) + " T" + eepromInfo1.Size);
      var num2 = (int) connection.WriteManualCommands("M618 S" + eepromInfo2.EepromAddr + " P" + EEPROMMapping.FloatToBinaryInt(gcode.Y) + " T" + eepromInfo2.Size);
      StandardVirtualCodes.SendOK(connection);
    }

    public static void M575SetFilament(GCode gcode, FirmwareController connection)
    {
      EepromAddressInfo eepromInfo1 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("FilamentColorID");
      EepromAddressInfo eepromInfo2 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("FilamentTypeID");
      EepromAddressInfo eepromInfo3 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("FilamentTemperature");
      EepromAddressInfo eepromInfo4 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("FilamentAmount");
      var num1 = (int) connection.WriteManualCommands("M618 S" + eepromInfo1.EepromAddr + " P" + gcode.S + " T" + eepromInfo1.Size, "M618 S" + eepromInfo2.EepromAddr + " P" + gcode.P + " T" + eepromInfo2.Size, "M618 S" + eepromInfo3.EepromAddr + " P" + gcode.T + " T" + eepromInfo3.Size);
      var int32 = BitConverter.ToInt32(BitConverter.GetBytes(gcode.E), 0);
      var num2 = (int) connection.WriteManualCommands("M618 S" + eepromInfo4.EepromAddr + " P" + int32 + " T" + eepromInfo4.Size);
      EepromAddressInfo eepromInfo5 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("FilamentSize");
      var num3 = (int) connection.WriteManualCommands("M618 S" + eepromInfo5.EepromAddr + " P" + gcode.I + " T" + eepromInfo5.Size);
      StandardVirtualCodes.SendOK(connection);
    }

    public static void M570SetFilamentUID(GCode gcode, FirmwareController connection)
    {
      EepromAddressInfo eepromInfo = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("FilamentUID");
      connection.AddManualCommandToFront("M618 S" + eepromInfo.EepromAddr + " P" + gcode.P + " T" + eepromInfo.Size);
      StandardVirtualCodes.SendOK(connection);
    }

    public static void M577SetBedOffsets(GCode gcode, FirmwareController connection)
    {
      EepromAddressInfo eepromInfo1 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("ZCalibrationBLO");
      EepromAddressInfo eepromInfo2 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("ZCalibrationBRO");
      EepromAddressInfo eepromInfo3 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("ZCalibrationFRO");
      EepromAddressInfo eepromInfo4 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("ZCalibrationFLO");
      EepromAddressInfo eepromInfo5 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("ZCalibrationZO");
      var stringList = new List<string>();
      if (gcode.HasX && eepromInfo1 != null)
      {
        var int32 = BitConverter.ToInt32(BitConverter.GetBytes(gcode.X), 0);
        stringList.Add("M618 S" + eepromInfo1.EepromAddr + " P" + int32 + " T" + eepromInfo1.Size);
      }
      if (gcode.HasY && eepromInfo2 != null)
      {
        var int32 = BitConverter.ToInt32(BitConverter.GetBytes(gcode.Y), 0);
        stringList.Add("M618 S" + eepromInfo2.EepromAddr + " P" + int32 + " T" + eepromInfo2.Size);
      }
      if (gcode.HasZ && eepromInfo3 != null)
      {
        var int32 = BitConverter.ToInt32(BitConverter.GetBytes(gcode.Z), 0);
        stringList.Add("M618 S" + eepromInfo3.EepromAddr + " P" + int32 + " T" + eepromInfo3.Size);
      }
      if (gcode.HasE && eepromInfo4 != null)
      {
        var int32 = BitConverter.ToInt32(BitConverter.GetBytes(gcode.E), 0);
        stringList.Add("M618 S" + eepromInfo4.EepromAddr + " P" + int32 + " T" + eepromInfo4.Size);
      }
      if (gcode.hasF && eepromInfo5 != null)
      {
        var int32 = BitConverter.ToInt32(BitConverter.GetBytes(gcode.F), 0);
        stringList.Add("M618 S" + eepromInfo5.EepromAddr + " P" + int32 + " T" + eepromInfo5.Size);
      }
      if (connection.CalibrationDetails.UsesCalibrationOffset)
      {
        EepromAddressInfo eepromInfo6 = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("CalibrationOffset");
        if (gcode.HasI && eepromInfo6 != null)
        {
          var int32 = BitConverter.ToInt32(BitConverter.GetBytes(gcode.I), 0);
          stringList.Add("M618 S" + eepromInfo6.EepromAddr + " P" + int32 + " T" + eepromInfo6.Size);
        }
      }
      if (stringList.Count > 0)
      {
        connection.AddManualCommandToFront(stringList.ToArray());
      }

      StandardVirtualCodes.SendOK(connection);
    }

    public static void M582SetNozzleWidth(GCode gcode, FirmwareController connection)
    {
      EepromAddressInfo eepromInfo = connection.MyPrinterProfile.EEPROMConstants.GetEepromInfo("NozzleSizeExtrusionWidth");
      if (eepromInfo == null)
      {
        connection.WriteLog(string.Format(">> Error:Printer doesn't have adjustable nozzle."), Logger.TextType.Read);
      }
      else
      {
        connection.AddManualCommandToFront("M618 S" + eepromInfo.EepromAddr + " P" + gcode.S + " T" + eepromInfo.Size);
      }

      StandardVirtualCodes.SendOK(connection);
    }

    public static void M583GetNozzleWidth(GCode gcode, FirmwareController connection)
    {
      connection.ProcessNozzleSizeExtrusionWidth();
      var nozzleSizeMicrons = connection.CurrentPrinterInfo.extruder.iNozzleSizeMicrons;
      connection.WriteLog(string.Format(">> ok S:{0}", nozzleSizeMicrons), Logger.TextType.Read);
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
      connection.VStopwatchReset();
      StandardVirtualCodes.SendOK(connection);
    }

    public static void M1014StopwatchStop(GCode gcode, FirmwareController connection)
    {
      var num = connection.NStopwatchReturnTime();
      if (0L <= num)
      {
        connection.WriteLog(string.Format(">> ok M:{0}", num), Logger.TextType.Read);
      }
      else
      {
        connection.WriteLog(">> stopwatch not running", Logger.TextType.Read);
      }
    }

    public static void M303SetGantryClipsToOff(GCode gcode, FirmwareController connection)
    {
      connection.SetGantryClips(true);
      StandardVirtualCodes.SendOK(connection);
    }

    public static void M304SetGantryClipsToOn(GCode gcode, FirmwareController connection)
    {
      connection.SetGantryClips(false);
      StandardVirtualCodes.SendOK(connection);
    }

    public static void M5680GetHoursUsed(GCode gcode, FirmwareController connection)
    {
      var num = connection.PersistantDetails.hours_used / 6f;
      connection.WriteLog(">> ok H:" + num.ToString(PrinterCompatibleString.PRINTER_CULTURE), Logger.TextType.Error);
    }

    private static void SendOK(BaseController controller)
    {
      controller.WriteLog(string.Format(">> ok"), Logger.TextType.Read);
    }
  }
}
