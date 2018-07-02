namespace M3D.Spooling.Common
{
  public class InitialPrintPreProcessorData
  {
    public int StartingTemp;
    public int StartingTempStabilizationDelay;
    public int StartingFanValue;
    public int BedTemperature;
    public int PrimeAmount;
    public int FirstRaftLayerTemperature;
    public bool SecondRaftResetTemp;

    public InitialPrintPreProcessorData()
    {
    }

    public InitialPrintPreProcessorData(InitialPrintPreProcessorData other)
    {
      StartingTemp = other.StartingTemp;
      StartingTempStabilizationDelay = other.StartingTempStabilizationDelay;
      StartingFanValue = other.StartingFanValue;
      BedTemperature = other.BedTemperature;
      PrimeAmount = other.PrimeAmount;
      FirstRaftLayerTemperature = other.FirstRaftLayerTemperature;
      SecondRaftResetTemp = other.SecondRaftResetTemp;
    }
  }
}
