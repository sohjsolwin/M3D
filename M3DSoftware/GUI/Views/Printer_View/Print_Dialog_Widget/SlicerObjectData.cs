using M3D.Model;
using OpenTK;

namespace M3D.GUI.Views.Printer_View.Print_Dialog_Widget
{
  internal struct SlicerObjectData
  {
    public ModelData modelData;
    public Matrix4 transformation;

    public SlicerObjectData(ModelData modelData, Matrix4 transformation)
    {
      this.modelData = modelData;
      this.transformation = transformation;
    }
  }
}
