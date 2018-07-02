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
