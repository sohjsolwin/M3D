// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Types.RectCoordinates
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

namespace M3D.Spooling.Common.Types
{
  public class RectCoordinates
  {
    public float front;
    public float back;
    public float left;
    public float right;

    public RectCoordinates()
    {
      front = 0.0f;
      back = 0.0f;
      left = 0.0f;
      right = 0.0f;
    }

    public RectCoordinates(float front, float back, float left, float right)
    {
      this.front = front;
      this.back = back;
      this.left = left;
      this.right = right;
    }

    public RectCoordinates(RectCoordinates other)
    {
      front = other.front;
      back = other.back;
      left = other.left;
      right = other.right;
    }
  }
}
