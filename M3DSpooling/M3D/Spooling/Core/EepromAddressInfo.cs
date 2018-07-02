// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Core.EepromAddressInfo
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System;

namespace M3D.Spooling.Core
{
  public class EepromAddressInfo
  {
    public string Name { get; private set; }

    public ushort EepromAddr { get; private set; }

    public int Size { get; private set; }

    public Type Type { get; private set; }

    public EepromAddressInfo(string name, ushort eepromAddr, int size, Type type)
    {
      Name = name;
      EepromAddr = eepromAddr;
      Size = size;
      Type = type;
    }
  }
}
