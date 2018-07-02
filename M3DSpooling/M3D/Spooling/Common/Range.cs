namespace M3D.Spooling.Common
{
  public struct Range
  {
    public float min;
    public float max;

    public Range(Range other)
    {
      min = other.min;
      max = other.max;
    }

    public Range(float min, float max)
    {
      this.min = min;
      this.max = max;
    }

    public bool Intercepts(out float p, float p1, float p2)
    {
      if (p2 < (double)min)
      {
        p = p1 >= (double)p2 ? p1 >= (double)min ? min : p1 : p2;
        return true;
      }
      if (p2 > (double)max)
      {
        p = p1 <= (double)p2 ? p1 <= (double)max ? max : p1 : p2;
        return true;
      }
      p = p2;
      return false;
    }
  }
}
