using M3D.Boot;
using M3D.Spooling.Embedded_Firmware;
using M3D.Spooling.FirstRunUpdates;
using M3D.Spooling.Preprocessors;
using M3D.Spooling.Preprocessors.Foundation;
using System.Collections.Generic;

namespace M3D.Spooling.Printer_Profiles
{
  internal class Micro1PrinterProfile : InternalPrinterProfile
  {
    public const uint FIRMWARE_VERSION = 2016040401;
    public const uint FIRMWARE_CRC = 562425535;

    public Micro1PrinterProfile()
    {
      ProfileName = "Micro";
      ProductConstants = new ProductProfile(9220U, 1003U, 32768U, 35, AVRChipData.ATxmega32C4, new Dictionary<char, FirmwareDetails>()
      {
        {
          'M',
          new FirmwareDetails(2016040401U, 562425535U, "M3D.Spooling.Embedded_Firmware.m3dfirmware-encrypt000001.hex")
        }
      });
      Scripts = new Micro1ScriptsProfile();
      PrinterSizeConstants = new Micro1PrinterSizeProfile();
      PreprocessorConstants = new PrinterPreprocessorProfile(new IPreprocessor[4]
      {
         new GCodeInitializationPreprocessor(),
         new BondingPreprocessor(),
         new BedCompensationPreprocessor(),
         new BackLashPreprocessor()
      });
      FirstRunConstants = new FirstRunProfile(new IFirstRunUpdater[6]
      {
         new FirstRunUpdateResetOffsets(),
         new FirstRunUpdateSetBatchto500mA(),
         new FirstRunUpdateDefaultFan(),
         new FirstRunUpdateFanValues(),
         new FirstRunUpdateSetBacklash1500(),
         new FirstRunUpdateSetSpeedLimits()
      });
      SpeedLimitConstants = new SpeedLimitProfile(4800f, 120f, 1500f, 4800f, 120f, 1500f, 60f, 30f, 60f, 600f, 60f, 102f, 720f, 60f, 360f, 4800f, 900f, 1500f);
      TemperatureConstants = new TemperatureProfile(150, 285);
      EEPROMConstants = new MicroEEPROMConstants();
      VirtualCodes = new MicroVirtualCodes();
      AccessoriesConstants = new AccessoriesProfile(false, 0, 0, false, false, 350, 300, 3000, 320, 2500);
      OptionsConstants = new SpoolerOptionsProfile();
    }
  }
}
