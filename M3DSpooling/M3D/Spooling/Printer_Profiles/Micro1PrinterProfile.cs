// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Printer_Profiles.Micro1PrinterProfile
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
      ProductConstants = new ProductProfile(9220U, 1003U, 32768U, (byte) 35, AVRChipData.ATxmega32C4, new Dictionary<char, FirmwareDetails>()
      {
        {
          'M',
          new FirmwareDetails(2016040401U, 562425535U, "M3D.Spooling.Embedded_Firmware.m3dfirmware-encrypt000001.hex")
        }
      });
      Scripts = (ScriptsProfile) new Micro1ScriptsProfile();
      PrinterSizeConstants = (PrinterSizeProfile) new Micro1PrinterSizeProfile();
      PreprocessorConstants = new PrinterPreprocessorProfile(new IPreprocessor[4]
      {
        (IPreprocessor) new GCodeInitializationPreprocessor(),
        (IPreprocessor) new BondingPreprocessor(),
        (IPreprocessor) new BedCompensationPreprocessor(),
        (IPreprocessor) new BackLashPreprocessor()
      });
      FirstRunConstants = new FirstRunProfile(new IFirstRunUpdater[6]
      {
        (IFirstRunUpdater) new FirstRunUpdateResetOffsets(),
        (IFirstRunUpdater) new FirstRunUpdateSetBatchto500mA(),
        (IFirstRunUpdater) new FirstRunUpdateDefaultFan(),
        (IFirstRunUpdater) new FirstRunUpdateFanValues(),
        (IFirstRunUpdater) new FirstRunUpdateSetBacklash1500(),
        (IFirstRunUpdater) new FirstRunUpdateSetSpeedLimits()
      });
      SpeedLimitConstants = new SpeedLimitProfile(4800f, 120f, 1500f, 4800f, 120f, 1500f, 60f, 30f, 60f, 600f, 60f, 102f, 720f, 60f, 360f, 4800f, 900f, 1500f);
      TemperatureConstants = new TemperatureProfile(150, 285);
      EEPROMConstants = (EEPROMProfile) new MicroEEPROMConstants();
      VirtualCodes = (VirtualCodeProfile) new MicroVirtualCodes();
      AccessoriesConstants = new AccessoriesProfile(false, 0, 0, false, false, 350, 300, 3000, 320, 2500);
      OptionsConstants = new SpoolerOptionsProfile();
    }
  }
}
