// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Printer_Profiles.MicroPlusEEPROMConstants
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Core;

namespace M3D.Spooling.Printer_Profiles
{
  internal class MicroPlusEEPROMConstants : EEPROMProfile
  {
    public MicroPlusEEPROMConstants()
      : base("Micro+", 78, (ushort) 382, (ushort) 362, 2)
    {
      AddEepromAddressInfo(new EepromAddressInfo("FirmwareVersion", (ushort) 0, 4, typeof (uint)));
      AddEepromAddressInfo(new EepromAddressInfo("FirmwareCRC", (ushort) 2, 4, typeof (uint)));
      AddEepromAddressInfo(new EepromAddressInfo("LastRecordedZValue", (ushort) 4, 4, typeof (int)));
      AddEepromAddressInfo(new EepromAddressInfo("BacklashX", (ushort) 6, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BacklashY", (ushort) 8, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BedCompensationBackRight", (ushort) 10, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BedCompensationBackLeft", (ushort) 12, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BedCompensationFrontLeft", (ushort) 14, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BedCompensationFrontRight", (ushort) 16, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("FilamentColorID", (ushort) 18, 4, typeof (uint)));
      AddEepromAddressInfo(new EepromAddressInfo("FilamentTypeID", (ushort) 20, 2, typeof (ushort)));
      AddEepromAddressInfo(new EepromAddressInfo("FilamentTemperature", (ushort) 21, 2, typeof (ushort)));
      AddEepromAddressInfo(new EepromAddressInfo("FilamentAmount", (ushort) 22, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("ZCalibrationBLO", (ushort) 36, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("ZCalibrationBRO", (ushort) 38, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("ZCalibrationFRO", (ushort) 40, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("ZCalibrationFLO", (ushort) 42, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("ZCalibrationZO", (ushort) 44, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BacklashSpeed", (ushort) 48, 4, typeof (float)));
      AddEepromAddressInfo(new EepromAddressInfo("BedCompensationVersion", (ushort) 50, 2, typeof (ushort)));
      AddEepromAddressInfo(new EepromAddressInfo("FilamentSize", (ushort) 65, 2, typeof (ushort)));
      AddEepromAddressInfo(new EepromAddressInfo("FilamentUID", (ushort) 66, 4, typeof (uint)));
      AddEepromAddressInfo(new EepromAddressInfo("EnabledFeatures", (ushort) 68, 4, typeof (uint)));
      AddEepromAddressInfo(new EepromAddressInfo("PowerFailureProgressIndicator", (ushort) 72, 4, typeof (uint)));
      AddEepromAddressInfo(new EepromAddressInfo("PowerFailurePrintingState", (ushort) 74, 2, typeof (ushort)));
      AddEepromAddressInfo(new EepromAddressInfo("PowerFailureLastSavedX", (ushort) 76, 4, typeof (uint)));
      AddEepromAddressInfo(new EepromAddressInfo("PowerFailureLastSavedY", (ushort) 78, 4, typeof (uint)));
      AddEepromAddressInfo(new EepromAddressInfo("HoursCounterSpooler", (ushort) 352, 4, typeof (uint)));
      AddEepromAddressInfo(new EepromAddressInfo("SerialNumber", (ushort) 363, 4, typeof (uint)));
      AddEepromAddressInfo(new EepromAddressInfo("SavedZState", (ushort) 371, 2, typeof (ushort)));
      AddEepromAddressInfo(new EepromAddressInfo("ExtruderCurrent", (ushort) 372, 2, typeof (ushort)));
      AddEepromAddressInfo(new EepromAddressInfo("HeaterResistance_M", (ushort) 373, 4, typeof (float)));
    }
  }
}
