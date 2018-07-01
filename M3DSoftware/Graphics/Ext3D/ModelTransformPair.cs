// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Ext3D.ModelTransformPair
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      this.data.name = "model";
      this.data.ID = 0U;
    }

    public void CalculateExtents()
    {
      if (this.modelNode == null)
        return;
      this.modelSize = this.modelNode.CalculateMinMax(this.transformNode.GetTransformationMatrix());
      this.OriginalModelSize = this.modelNode.CalculateMinMax();
      this.size = Math.Max(this.modelSize.Ext.x, this.modelSize.Ext.y);
      this.size = Math.Max(this.modelSize.Ext.z, this.size);
    }

    public ModelSize OriginalModelSize { get; private set; }

    public struct Data
    {
      public string name;
      public uint ID;

      public override string ToString()
      {
        return this.name;
      }
    }
  }
}
