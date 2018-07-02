using M3D.Spooling.Core;

namespace M3D.Spooling.Printer_Profiles
{
  internal class MicroPlusEEPROMConstants : EEPROMProfile
  {
    public MicroPlusEEPROMConstants()
      : base("Micro+", 78, 382, 362, 2)
    {
      AddEepromAddressInfo(new EepromAddressInfo("FirmwareVersion", 0, 4, typeof (uint)));
      AddEepromAddressInfo(new EepromAddressInfo("FirmwareCRC", 2, 4, typeof (uint)));
      AddEepromAddressInfo(new EepromAddressInfo("LastRecordedZValue", 4, 4, typeof (int)));
      AddEepromAddressInfo(new EepromAddressInfo("BacklashX", 6, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BacklashY", 8, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BedCompensationBackRight", 10, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BedCompensationBackLeft", 12, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BedCompensationFrontLeft", 14, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BedCompensationFrontRight", 16, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("FilamentColorID", 18, 4, typeof (uint)));
      AddEepromAddressInfo(new EepromAddressInfo("FilamentTypeID", 20, 2, typeof (ushort)));
      AddEepromAddressInfo(new EepromAddressInfo("FilamentTemperature", 21, 2, typeof (ushort)));
      AddEepromAddressInfo(new EepromAddressInfo("FilamentAmount", 22, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("ZCalibrationBLO", 36, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("ZCalibrationBRO", 38, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("ZCalibrationFRO", 40, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("ZCalibrationFLO", 42, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("ZCalibrationZO", 44, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BacklashSpeed", 48, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BedCompensationVersion", 50, 2, typeof (ushort)));
      AddEepromAddressInfo(new EepromAddressInfo("FilamentSize", 65, 2, typeof (ushort)));
      AddEepromAddressInfo(new EepromAddressInfo("FilamentUID", 66, 4, typeof (uint)));
      AddEepromAddressInfo(new EepromAddressInfo("EnabledFeatures", 68, 4, typeof (uint)));
      AddEepromAddressInfo(new EepromAddressInfo("PowerFailureProgressIndicator", 72, 4, typeof (uint)));
      AddEepromAddressInfo(new EepromAddressInfo("PowerFailurePrintingState", 74, 2, typeof (ushort)));
      AddEepromAddressInfo(new EepromAddressInfo("PowerFailureLastSavedX", 76, 4, typeof (uint)));
      AddEepromAddressInfo(new EepromAddressInfo("PowerFailureLastSavedY", 78, 4, typeof (uint)));
      AddEepromAddressInfo(new EepromAddressInfo("HoursCounterSpooler", 352, 4, typeof (uint)));
      AddEepromAddressInfo(new EepromAddressInfo("SerialNumber", 363, 4, typeof (uint)));
      AddEepromAddressInfo(new EepromAddressInfo("SavedZState", 371, 2, typeof (ushort)));
      AddEepromAddressInfo(new EepromAddressInfo("ExtruderCurrent", 372, 2, typeof (ushort)));
      AddEepromAddressInfo(new EepromAddressInfo("HeaterResistance_M", 373, 4, typeof (float)));
    }
  }
}
