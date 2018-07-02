using M3D.Model.Utils;

namespace M3D.Model
{
  public class ModelSize
  {
    public ModelSize(Vector3 Min, Vector3 Max)
    {
      this.Min = Min;
      this.Max = Max;
    }

    public Vector3 Max { get; private set; }

    public Vector3 Min { get; private set; }

    public Vector3 Ext
    {
      get
      {
        return Max - Min;
      }
    }
  }
}
