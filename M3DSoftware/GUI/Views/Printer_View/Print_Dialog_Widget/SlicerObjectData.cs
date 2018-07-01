// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Printer_View.Print_Dialog_Widget.SlicerObjectData
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
