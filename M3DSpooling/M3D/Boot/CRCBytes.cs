using System.Runtime.InteropServices;

namespace M3D.Boot
{
  [StructLayout(LayoutKind.Explicit)]
  internal struct CRCBytes
  {
    [FieldOffset(0)]
    public byte Byte1;
    [FieldOffset(1)]
    public byte Byte2;
    [FieldOffset(2)]
    public byte Byte3;
    [FieldOffset(3)]
    public byte Byte4;
    [FieldOffset(0)]
    public uint Int1;
  }
}
