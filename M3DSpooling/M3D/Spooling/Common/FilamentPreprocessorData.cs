// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.FilamentPreprocessorData
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
