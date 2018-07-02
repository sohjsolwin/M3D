namespace M3D.Spooling.Common
{
  public class BondingPreprocessorData
  {
    public int FirstLayerTemp;
    public int SecondLayerTemp;

    public BondingPreprocessorData()
    {
    }

    public BondingPreprocessorData(BondingPreprocessorData other)
    {
      FirstLayerTemp = other.FirstLayerTemp;
      SecondLayerTemp = other.SecondLayerTemp;
    }
  }
}
