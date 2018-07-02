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
