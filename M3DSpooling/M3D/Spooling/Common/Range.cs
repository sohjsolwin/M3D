// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Range
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
      if ((double) p2 < (double)min)
      {
        p = (double) p1 >= (double) p2 ? ((double) p1 >= (double)min ? min : p1) : p2;
        return true;
      }
      if ((double) p2 > (double)max)
      {
        p = (double) p1 <= (double) p2 ? ((double) p1 <= (double)max ? max : p1) : p2;
        return true;
      }
      p = p2;
      return false;
    }
  }
}
