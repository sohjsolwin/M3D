using OpenTK;

namespace M3D.Model
{
  public class ModelTransform
  {
    public ModelData data;
    public Matrix4 transformMatrix;

    public ModelTransform(ModelData data, Matrix4 transformMatrix)
    {
      this.data = data;
      this.transformMatrix = transformMatrix;
    }
  }
}
