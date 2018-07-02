// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.PLA_FilamentProfile
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Printer_Profiles;

namespace M3D.Spooling.Common
{
  internal class PLA_FilamentProfile : FilamentProfile
  {
    public PLA_FilamentProfile(FilamentSpool spool, PrinterProfile printer_profile)
      : base(spool)
    {
      preprocessor.initialPrint.StartingTemp = filament.filament_temperature;
      preprocessor.initialPrint.StartingFanValue = (int) byte.MaxValue;
      preprocessor.initialPrint.StartingTempStabilizationDelay = 15;
      preprocessor.bonding.FirstLayerTemp = printer_profile.TemperatureConstants.GetBoundedTemp(filament.filament_temperature + 10);
      preprocessor.bonding.SecondLayerTemp = printer_profile.TemperatureConstants.GetBoundedTemp(filament.filament_temperature + 5);
      preprocessor.initialPrint.PrimeAmount = 19;
      preprocessor.initialPrint.FirstRaftLayerTemperature = printer_profile.TemperatureConstants.GetBoundedTemp(filament.filament_temperature + 10);
      preprocessor.initialPrint.SecondRaftResetTemp = false;
    }

    public override bool TestSizeWarning(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
    {
      return false;
    }

    public override string ShortName
    {
      get
      {
        return "PLA";
      }
    }
  }
}
