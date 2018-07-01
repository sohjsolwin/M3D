// Decompiled with JetBrains decompiler
// Type: M3D.Model.ModelTransform
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
