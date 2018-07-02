using M3D.Spooling.Core;

namespace M3D.Spooling.Printer_Profiles
{
  internal class MicroEEPROMConstants : EEPROMProfile
  {
    public MicroEEPROMConstants()
      : base("Micro", 98, 767, 512, 1)
    {
      AddEepromAddressInfo(new EepromAddressInfo("FirmwareVersion", 0, 4, typeof (uint)));
      AddEepromAddressInfo(new EepromAddressInfo("FirmwareCRC", 4, 4, typeof (uint)));
      AddEepromAddressInfo(new EepromAddressInfo("LastRecordedZValue", 8, 4, typeof (int)));
      AddEepromAddressInfo(new EepromAddressInfo("BacklashX", 12, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BacklashY", 16, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BedCompensationBackRight", 20, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BedCompensationBackLeft", 24, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BedCompensationFrontLeft", 28, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BedCompensationFrontRight", 32, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("FilamentColorID", 36, 4, typeof (uint)));
      AddEepromAddressInfo(new EepromAddressInfo("FilamentTypeID", 40, 1, typeof (byte)));
      AddEepromAddressInfo(new EepromAddressInfo("FilamentTemperature", 41, 1, typeof (byte)));
      AddEepromAddressInfo(new EepromAddressInfo("FilamentAmount", 42, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BacklashExpansionXPlus", 46, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BacklashExpansionYLPlus", 50, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BacklashExpansionYRPlus", 54, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BacklashExpansionYRMinus", 58, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BacklashExpansionZ", 62, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BacklashExpansionE", 66, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("ZCalibrationBLO", 70, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("ZCalibrationBRO", 74, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("ZCalibrationFRO", 78, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("ZCalibrationFLO", 82, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("ZCalibrationZO", 86, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("ReservedForSpooler", 90, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BacklashSpeed", 94, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BedCompensationVersion", 98, 1, typeof (byte)));
      AddEepromAddressInfo(new EepromAddressInfo("SpeedLimitX", 102, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("SpeedLimitY", 106, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("SpeedLimitZ", 110, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("SpeedLimitEp", 114, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("SpeedLimitEn", 118, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("FilamentSize", 130, 1, typeof (byte)));
      AddEepromAddressInfo(new EepromAddressInfo("FilamentUID", 131, 4, typeof (uint)));
      AddEepromAddressInfo(new EepromAddressInfo("G32FirstSample", 262, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("FANTYPE", 683, 1, typeof (byte)));
      AddEepromAddressInfo(new EepromAddressInfo("FANOFFSET", 684, 1, typeof (byte)));
      AddEepromAddressInfo(new EepromAddressInfo("FANSCALE", 685, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("HeaterCalibrationMode", 689, 1, typeof (byte)));
      AddEepromAddressInfo(new EepromAddressInfo("XMotorCurrent", 690, 2, typeof (ushort)));
      AddEepromAddressInfo(new EepromAddressInfo("YMotorCurrent", 692, 2, typeof (ushort)));
      AddEepromAddressInfo(new EepromAddressInfo("ZMotorCurrent", 694, 2, typeof (ushort)));
      AddEepromAddressInfo(new EepromAddressInfo("HardwareStatus", 696, 2, typeof (ushort)));
      AddEepromAddressInfo(new EepromAddressInfo("HeaterTempMeasure_B", 698, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("HoursCounterSpooler", 704, 4, typeof (uint)));
      AddEepromAddressInfo(new EepromAddressInfo("XAxisStepsPerMM", 726, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("YAxisStepsPerMM", 730, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("ZAxisStepsPerMM", 734, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("EAxisStepsPerMM", 738, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("SavedZState", 742, 2, typeof (ushort)));
      AddEepromAddressInfo(new EepromAddressInfo("ExtruderCurrent", 744, 2, typeof (ushort)));
      AddEepromAddressInfo(new EepromAddressInfo("HeaterResistance_M", 746, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("SerialNumber", 751, 4, typeof (uint)));
    }
  }
}
