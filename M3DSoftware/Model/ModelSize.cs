// Decompiled with JetBrains decompiler
// Type: M3D.Model.ModelSize
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
