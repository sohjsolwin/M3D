// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Ext3D.ModelRendering.TriangleFace
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using System.Runtime.InteropServices;

namespace M3D.Graphics.Ext3D.ModelRendering
{
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct TriangleFace
  {
    public uint index1;
    public uint index2;
    public uint index3;

    public TriangleFace(uint v1Index, uint v2Index, uint v3Index)
    {
      this.index1 = v1Index;
      this.index2 = v2Index;
      this.index3 = v3Index;
    }
  }
}
