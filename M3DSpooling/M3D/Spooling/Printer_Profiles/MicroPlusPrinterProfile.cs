// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Printer_Profiles.MicroPlusPrinterProfile
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
  internal class MicroPlusPrinterProfile : InternalPrinterProfile
  {
    public const uint M_FIRMWARE_VERSION = 2017121801;
    public const uint M_FIRMWARE_CRC = 3451098045;

    public MicroPlusPrinterProfile()
    {
      this.ProfileName = "Micro+";
      this.ProductConstants = new ProductProfile(41503U, 1155U, 104448U, (byte) 139, STMChipData.STM32F070CB, new Dictionary<char, FirmwareDetails>()
      {
        {
          'M',
          new FirmwareDetails(2017121801U, 3451098045U, "M3D.Spooling.Embedded_Firmware.m3dfirmware-encrypt00M30x.hex")
        }
      });
      this.Scripts = (ScriptsProfile) new MicroPlusScriptsProfile();
      this.PrinterSizeConstants = (PrinterSizeProfile) new Micro1PrinterSizeProfile();
      this.PreprocessorConstants = new PrinterPreprocessorProfile(new IPreprocessor[3]
      {
        (IPreprocessor) new GCodeInitializationPreprocessor(),
        (IPreprocessor) new BondingPreprocessor(),
        (IPreprocessor) new BedCompensationPreprocessor()
      });
      this.FirstRunConstants = new FirstRunProfile(new IFirstRunUpdater[0]);
      this.SpeedLimitConstants = new SpeedLimitProfile(4800f, 120f, 1500f, 4800f, 120f, 1500f, 120f, 30f, 120f, 600f, 60f, 102f, 720f, 60f, 360f, 4800f, 1800f, 7200f);
      this.TemperatureConstants = new TemperatureProfile(150, 285);
      this.EEPROMConstants = (EEPROMProfile) new MicroPlusEEPROMConstants();
      this.VirtualCodes = (VirtualCodeProfile) new MicroPlusVirtualCodes();
      this.SupportedFeaturesConstants = new SupportedFeaturesProfile(new Dictionary<string, int>()
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
        }
      });
      this.AccessoriesConstants = new AccessoriesProfile(false, 0, 0, true, false, 350, 300, 3000, 320, 2500);
      this.OptionsConstants = new SpoolerOptionsProfile();
      this.OptionsConstants.CheckGantryClips = true;
      this.OptionsConstants.VerifyGantryNonZeroValues = false;
      this.OptionsConstants.HomeAndSetTempOnCalibration = true;
      this.OptionsConstants.G92WorksOnAllAxes = true;
    }
  }
}
