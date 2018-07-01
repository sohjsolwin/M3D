// Decompiled with JetBrains decompiler
// Type: ISerialPortIo
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System;

public interface ISerialPortIo : IDisposable
{
  string ReadLine();

  void WriteLine(string text);

  void Write(byte[] command, int offset, int count);

  byte ReadByte();

  int Read(byte[] bytes, int offset, int count);

  string ReadExisting();

  void Open();

  object ThreadSync { get; }

  string PortName { get; set; }

  int WriteTimeout { get; set; }

  int ReadTimeout { get; set; }

  int BytesToRead { get; }

  bool IsOpen { get; }
}
