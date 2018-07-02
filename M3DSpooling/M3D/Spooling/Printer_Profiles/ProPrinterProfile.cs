using M3D.Boot;
using M3D.Spooling.Embedded_Firmware;
using M3D.Spooling.FirstRunUpdates;
using M3D.Spooling.Preprocessors;
using M3D.Spooling.Preprocessors.Foundation;
using System.Collections.Generic;

namespace M3D.Spooling.Printer_Profiles
{
  internal class ProPrinterProfile : InternalPrinterProfile
  {
    public const uint M_FIRMWARE_VERSION = 2017121802;
    public const uint M_FIRMWARE_CRC = 243077141;
    public const uint S_FIRMWARE_VERSION = 2017120801;
    public const uint S_FIRMWARE_CRC = 751449189;

    public ProPrinterProfile()
    {
      ProfileName = "Pro";
      ProductConstants = new ProductProfile(41502U, 1155U, 98304U, 31, STMChipData.STM32F070CB, new Dictionary<char, FirmwareDetails>()
      {
        {
          'S',
          new FirmwareDetails(2017120801U, 751449189U, "M3D.Spooling.Embedded_Firmware.m3dfirmware-encrypt00S23x.hex")
        },
        {
          'M',
          new FirmwareDetails(2017121802U, 243077141U, "M3D.Spooling.Embedded_Firmware.m3dfirmware-encrypt00M24x.hex")
        }
      });
      Scripts = new ProScriptsProfile();
      PrinterSizeConstants = new ProPrinterSizeProfile();
      PreprocessorConstants = new PrinterPreprocessorProfile(new IPreprocessor[4]
      {
         new GCodeInitializationPreprocessor(),
         new BondingPreprocessor(),
         new BedCompensationPreprocessor(),
         new BackLashPreprocessor()
      });
      FirstRunConstants = new FirstRunProfile(new IFirstRunUpdater[0]);
      SpeedLimitConstants = new SpeedLimitProfile(12000f, 120f, 3000f, 12000f, 120f, 3000f, 240f, 30f, 120f, 600f, 60f, 144f, 720f, 60f, 360f, 12000f, 2520f, 7200f);
      TemperatureConstants = new TemperatureProfile(70, 295);
      EEPROMConstants = new ProEEPROMConstants();
      VirtualCodes = new ProVirtualCodes();
      SupportedFeaturesConstants = new SupportedFeaturesProfile(new Dictionary<string, int>()
      {
        {
          "Single Point Bed Height Calibration",
          0
        },
        {
          "Multi Point Automatic Bed Calibration",
          1
        },
        {
          "Power Outage Recovery",
          2
        },
        {
          "Untethered Printing",
          3
        },
        {
          "Heated Bed Control",
          4
        },
        {
          "Motion Recovery",
          5
        }
      });
      AccessoriesConstants = new AccessoriesProfile(true, 50, 100, true, true, 400, 300, 3000, 320, 2500);
      OptionsConstants = new SpoolerOptionsProfile
      {
        CheckGantryClips = false,
        VerifyGantryNonZeroValues = false,
        HomeAndSetTempOnCalibration = true,
        G92WorksOnAllAxes = true
      };
    }
  }
}
