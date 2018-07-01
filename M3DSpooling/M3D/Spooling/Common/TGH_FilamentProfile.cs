// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.TGH_FilamentProfile
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Printer_Profiles;

namespace M3D.Spooling.Common
{
  internal class TGH_FilamentProfile : FilamentProfile
  {
    public TGH_FilamentProfile(FilamentSpool spool, PrinterProfile printer_profile)
      : base(spool)
    {
      this.preprocessor.initialPrint.StartingFanValue = (int) byte.MaxValue;
      this.preprocessor.initialPrint.StartingTempStabilizationDelay = 15;
      this.preprocessor.initialPrint.StartingTemp = printer_profile.TemperatureConstants.GetBoundedTemp(this.filament.filament_temperature - 5);
      this.preprocessor.bonding.FirstLayerTemp = this.preprocessor.initialPrint.StartingTemp;
      this.preprocessor.bonding.SecondLayerTemp = printer_profile.TemperatureConstants.GetBoundedTemp(this.filament.filament_temperature - 10);
      this.preprocessor.initialPrint.PrimeAmount = 9;
      this.preprocessor.initialPrint.FirstRaftLayerTemperature = printer_profile.TemperatureConstants.GetBoundedTemp(this.filament.filament_temperature - 5);
      this.preprocessor.initialPrint.SecondRaftResetTemp = true;
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
