// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.ABS_R_FilamentProfile
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Printer_Profiles;

namespace M3D.Spooling.Common
{
  public class ABS_R_FilamentProfile : ABS_FilamentProfile
  {
    public ABS_R_FilamentProfile(FilamentSpool spool, PrinterProfile printer_profile)
      : base(spool, printer_profile)
    {
      this.preprocessor.bonding.FirstLayerTemp = printer_profile.TemperatureConstants.GetBoundedTemp(this.filament.filament_temperature - 15);
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
