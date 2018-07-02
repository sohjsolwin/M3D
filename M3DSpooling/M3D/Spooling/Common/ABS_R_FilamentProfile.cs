using M3D.Spooling.Printer_Profiles;

namespace M3D.Spooling.Common
{
  public class ABS_R_FilamentProfile : ABS_FilamentProfile
  {
    public ABS_R_FilamentProfile(FilamentSpool spool, PrinterProfile printer_profile)
      : base(spool, printer_profile)
    {
      preprocessor.bonding.FirstLayerTemp = printer_profile.TemperatureConstants.GetBoundedTemp(filament.filament_temperature - 15);
    }

    public override string ShortName
    {
      get
      {
        return "ABR";
      }
    }
  }
}
