namespace M3D.Spooling.Common
{
  public class FilamentPreprocessorData
  {
    public InitialPrintPreProcessorData initialPrint;
    public BondingPreprocessorData bonding;

    public FilamentPreprocessorData()
    {
      initialPrint = new InitialPrintPreProcessorData();
      bonding = new BondingPreprocessorData();
    }

    public FilamentPreprocessorData(FilamentPreprocessorData other)
    {
      initialPrint = new InitialPrintPreProcessorData(other.initialPrint);
      bonding = new BondingPreprocessorData(other.bonding);
    }
  }
}
