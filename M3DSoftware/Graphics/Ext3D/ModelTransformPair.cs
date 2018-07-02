using M3D.Model;
using System;

namespace M3D.Graphics.Ext3D
{
  public class ModelTransformPair
  {
    public TransformationNode transformNode;
    public Model3DNode modelNode;
    public ModelSize modelSize;
    public ModelTransformPair.Data data;
    public float size;

    public ModelTransformPair()
      : this((TransformationNode) null, (Model3DNode) null)
    {
    }

    public ModelTransformPair(TransformationNode transformNode, Model3DNode modelNode)
    {
      this.transformNode = transformNode;
      this.modelNode = modelNode;
      data.name = "model";
      data.ID = 0U;
    }

    public void CalculateExtents()
    {
      if (modelNode == null)
      {
        return;
      }

      modelSize = modelNode.CalculateMinMax(transformNode.GetTransformationMatrix());
      OriginalModelSize = modelNode.CalculateMinMax();
      size = Math.Max(modelSize.Ext.x, modelSize.Ext.y);
      size = Math.Max(modelSize.Ext.z, size);
    }

    public ModelSize OriginalModelSize { get; private set; }

    public struct Data
    {
      public string name;
      public uint ID;

      public override string ToString()
      {
        return name;
      }
    }
  }
}
