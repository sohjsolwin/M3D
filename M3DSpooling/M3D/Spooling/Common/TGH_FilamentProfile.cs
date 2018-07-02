using M3D.Spooling.Printer_Profiles;

namespace M3D.Spooling.Common
{
  internal class TGH_FilamentProfile : FilamentProfile
  {
    public TGH_FilamentProfile(FilamentSpool spool, PrinterProfile printer_profile)
      : base(spool)
    {
      preprocessor.initialPrint.StartingFanValue = (int) byte.MaxValue;
      preprocessor.initialPrint.StartingTempStabilizationDelay = 15;
      preprocessor.initialPrint.StartingTemp = printer_profile.TemperatureConstants.GetBoundedTemp(filament.filament_temperature - 5);
      preprocessor.bonding.FirstLayerTemp = preprocessor.initialPrint.StartingTemp;
      preprocessor.bonding.SecondLayerTemp = printer_profile.TemperatureConstants.GetBoundedTemp(filament.filament_temperature - 10);
      preprocessor.initialPrint.PrimeAmount = 9;
      preprocessor.initialPrint.FirstRaftLayerTemperature = printer_profile.TemperatureConstants.GetBoundedTemp(filament.filament_temperature - 5);
      preprocessor.initialPrint.SecondRaftResetTemp = true;
    }

    public override bool TestSizeWarning(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
    {
      return false;
    }

    public override string ShortName
    {
      get
      {
        return "TGH";
      }
    }
  }
}
