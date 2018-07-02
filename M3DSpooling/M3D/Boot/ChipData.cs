namespace M3D.Boot
{
  internal class ChipData
  {
    public int PageSize;
    public int TotalMemory;

    public ChipData(int PageSize, int NumberOfPagesTotal)
    {
      this.PageSize = PageSize;
      TotalMemory = NumberOfPagesTotal * PageSize * 2;
    }
  }
}
