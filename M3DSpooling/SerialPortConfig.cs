// Decompiled with JetBrains decompiler
// Type: SerialPortConfig
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System;
using System.IO.Ports;

public class SerialPortConfig
{
  public string Name { get; private set; }

  public int BaudRate { get; private set; }

  public int DataBits { get; private set; }

  public StopBits StopBits { get; private set; }

  public Parity Parity { get; private set; }

  public bool DtrEnable { get; private set; }

  public bool RtsEnable { get; private set; }

  public SerialPortConfig(string name, int baudRate, int dataBits, StopBits stopBits, Parity parity, bool dtrEnable, bool rtsEnable)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentNullException(nameof (name));
    this.RtsEnable = rtsEnable;
    this.BaudRate = baudRate;
    this.DataBits = dataBits;
    this.StopBits = stopBits;
    this.Parity = parity;
    this.DtrEnable = dtrEnable;
    this.Name = name;
  }

  public override string ToString()
  {
    return string.Format("{0} (Baud: {1}/DataBits: {2}/Parity: {3}/StopBits: {4}/{5})", (object) this.Name, (object) this.BaudRate, (object) this.DataBits, (object) this.Parity, (object) this.StopBits, this.RtsEnable ? (object) "RTS" : (object) "No RTS");
  }
}
