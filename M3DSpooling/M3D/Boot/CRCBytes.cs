// Decompiled with JetBrains decompiler
// Type: M3D.Boot.CRCBytes
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
