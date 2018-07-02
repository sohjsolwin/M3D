namespace M3D.Spooling.Common
{
  public class SDCard
  {
    public bool IsPrintingFromSD;
    public long SDFilePosition;
    public long SDFileLength;

    public SDCard()
    {
      IsPrintingFromSD = false;
      SDFilePosition = 0L;
      SDFileLength = 0L;
    }

    public SDCard(SDCard other)
    {
      IsPrintingFromSD = other.IsPrintingFromSD;
      SDFilePosition = other.SDFilePosition;
      SDFileLength = other.SDFileLength;
    }
  }
}
