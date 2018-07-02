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
