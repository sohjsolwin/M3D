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
      index1 = v1Index;
      index2 = v2Index;
      index3 = v3Index;
    }
  }
}
