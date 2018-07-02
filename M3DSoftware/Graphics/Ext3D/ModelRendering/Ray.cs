using M3D.Model.Utils;

namespace M3D.Graphics.Ext3D.ModelRendering
{
  public class Ray
  {
    private Vector3 position;
    private Vector3 direction;

    public Ray(Vector3 position, Vector3 direction)
    {
      this.position = new Vector3(position);
      this.direction = new Vector3(direction);
    }

    public Vector3 Position
    {
      get
      {
        return position;
      }
    }

    public Vector3 Direction
    {
      get
      {
        return direction;
      }
    }
  }
}
