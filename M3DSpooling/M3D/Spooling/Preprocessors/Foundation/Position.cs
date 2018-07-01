// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Preprocessors.Foundation.Position
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
