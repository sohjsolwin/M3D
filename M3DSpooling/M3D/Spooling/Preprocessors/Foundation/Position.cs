namespace M3D.Spooling.Preprocessors.Foundation
{
  public class Position
  {
    public float absoluteX;
    public float absoluteY;
    public float absoluteZ;
    public float absoluteE;
    public float relativeX;
    public float relativeY;
    public float relativeZ;
    public float relativeE;
    public float F;

    public enum Direction
    {
      Positive,
      Negative,
      Neither,
    }
  }
}
