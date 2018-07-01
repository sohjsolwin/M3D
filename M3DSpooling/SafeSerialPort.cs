// Decompiled with JetBrains decompiler
// Type: SafeSerialPort
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using SerialPortTester;
using System;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using System.Threading;

public class SafeSerialPort : ISerialPortIo, IDisposable
{
  public string logMessage = string.Empty;
  public object threadsync = new object();
  public object portThreadSync = new object();
  private bool _isAlive = true;
  private SerialPort _port;
  private Stream _internalSerialStream;

  public SafeSerialPort(object portThreadSync)
    : this("COM1", 9600, Parity.None, 8, StopBits.One, portThreadSync)
  {
  }

  public SafeSerialPort(string portName, object portThreadSync)
    : this(portName, 9600, Parity.None, 8, StopBits.One, portThreadSync)
  {
  }

  public SafeSerialPort(string portName, int baudRate, object portThreadSync)
    : this(portName, baudRate, Parity.None, 8, StopBits.One, portThreadSync)
  {
  }

  public SafeSerialPort(string portName, int baudRate, Parity parity, object portThreadSync)
    : this(portName, baudRate, parity, 8, StopBits.One, portThreadSync)
  {
  }

  public SafeSerialPort(string portName, int baudRate, Parity parity, int dataBits, object portThreadSync)
    : this(portName, baudRate, parity, dataBits, StopBits.One, portThreadSync)
  {
  }

  public SafeSerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, object portThreadSync)
    : this(new SerialPortConfig(portName, baudRate, dataBits, stopBits, parity, false, false), portThreadSync)
  {
  }

  public SafeSerialPort(SerialPortConfig portConfig, object portThreadSync)
  {
    this.portThreadSync = portThreadSync;
    lock (portThreadSync)
    {
      lock (this.threadsync)
      {
        if (portConfig == null)
          throw new ArgumentNullException(nameof (portConfig));
        this._port = new SerialPort(portConfig.Name, portConfig.BaudRate, portConfig.Parity, portConfig.DataBits, portConfig.StopBits)
        {
          RtsEnable = portConfig.RtsEnable,
          DtrEnable = portConfig.DtrEnable,
          ReadTimeout = 5000,
          WriteTimeout = 5000
        };
      }
    }
  }

  public string ReadLine()
  {
    lock (this.threadsync)
    {
      if (!this._isAlive)
        throw new Exception("The port has been disposed;");
      return this._port.ReadTo(Environment.NewLine);
    }
  }

  public void WriteLine(string text)
  {
    lock (this.portThreadSync)
    {
      lock (this.threadsync)
      {
        if (!this._isAlive)
          throw new Exception("The port has been disposed;");
        this._port.Write(text);
        this._port.Write("\r");
      }
    }
  }

  public void Write(byte[] command, int offset, int count)
  {
    lock (this.portThreadSync)
    {
      lock (this.threadsync)
      {
        if (!this._isAlive)
          throw new Exception("The port has been disposed;");
        this._port.Write(command, offset, count);
      }
    }
  }

  public byte ReadByte()
  {
    lock (this.portThreadSync)
    {
      lock (this.threadsync)
      {
        if (!this._isAlive)
          throw new Exception("The port has been disposed;");
        return (byte) this._port.ReadByte();
      }
    }
  }

  public int Read(byte[] bytes, int offset, int count)
  {
    lock (this.portThreadSync)
    {
      lock (this.threadsync)
      {
        if (!this._isAlive)
          throw new Exception("The port has been disposed;");
        return this._port.Read(bytes, offset, count);
      }
    }
  }

  public string ReadExisting()
  {
    lock (this.portThreadSync)
    {
      lock (this.threadsync)
      {
        if (!this._isAlive)
          throw new Exception("The port has been disposed;");
        return this._port.ReadExisting();
      }
    }
  }

  public void Open()
  {
    lock (this.portThreadSync)
    {
      lock (this.threadsync)
      {
        if (!this._isAlive)
          throw new Exception("The port has been disposed;");
        try
        {
          SerialPortFixer.Execute(this._port.PortName);
        }
        catch (Exception ex)
        {
        }
        this._port.Open();
        try
        {
          this._internalSerialStream = this._port.BaseStream;
          this._port.DiscardInBuffer();
          this._port.DiscardOutBuffer();
        }
        catch (Exception ex)
        {
          Stream internalSerialStream = this._internalSerialStream;
          if (internalSerialStream == null)
          {
            FieldInfo field = typeof (SerialPort).GetField("internalSerialStream", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == (FieldInfo) null)
              throw;
            else
              internalSerialStream = (Stream) field.GetValue((object) this._port);
          }
          this.logMessage = this.logMessage + "\nAn error occurred while constructing the serial port adaptor:" + ex.ToString();
          this.SafeDisconnect(this._port, internalSerialStream);
          throw;
        }
      }
    }
  }

  public void Dispose()
  {
    lock (this.portThreadSync)
    {
      lock (this.threadsync)
      {
        if (!this._isAlive)
          return;
        this.SafeDisconnect(this._port, this._internalSerialStream);
        GC.SuppressFinalize((object) this);
        this._isAlive = false;
      }
    }
  }

  private void SafeDisconnect()
  {
    this.SafeDisconnect(this._port, this._internalSerialStream);
  }

  private void SafeDisconnect(SerialPort port, Stream internalSerialStream)
  {
    lock (this.portThreadSync)
    {
      lock (this.threadsync)
      {
        if (!this._isAlive)
          return;
        if (port != null)
          GC.SuppressFinalize((object) port);
        if (internalSerialStream != null)
        {
          GC.SuppressFinalize((object) internalSerialStream);
          this.ShutdownEventLoopHandler(internalSerialStream);
        }
        try
        {
          internalSerialStream?.Close();
        }
        catch (Exception ex)
        {
        }
        try
        {
          port?.Close();
        }
        catch (Exception ex)
        {
        }
      }
    }
  }

  private void ShutdownEventLoopHandler(Stream internalSerialStream)
  {
    lock (this.portThreadSync)
    {
      lock (this.threadsync)
      {
        try
        {
          FieldInfo field1 = internalSerialStream.GetType().GetField("eventRunner", BindingFlags.Instance | BindingFlags.NonPublic);
          if (field1 == (FieldInfo) null)
            return;
          object obj = field1.GetValue((object) internalSerialStream);
          Type type = obj.GetType();
          FieldInfo field2 = type.GetField("endEventLoop", BindingFlags.Instance | BindingFlags.NonPublic);
          FieldInfo field3 = type.GetField("eventLoopEndedSignal", BindingFlags.Instance | BindingFlags.NonPublic);
          FieldInfo field4 = type.GetField("waitCommEventWaitHandle", BindingFlags.Instance | BindingFlags.NonPublic);
          if (field2 == (FieldInfo) null || field3 == (FieldInfo) null || field4 == (FieldInfo) null)
            return;
          WaitHandle waitHandle = (WaitHandle) field3.GetValue(obj);
          ManualResetEvent manualResetEvent = (ManualResetEvent) field4.GetValue(obj);
          field2.SetValue(obj, (object) true);
          do
          {
            manualResetEvent.Set();
          }
          while (!waitHandle.WaitOne(2000));
        }
        catch (Exception ex)
        {
        }
      }
    }
  }

  public string PortName
  {
    get
    {
      return this._port.PortName;
    }
    set
    {
      this._port.PortName = value;
    }
  }

  public int BytesToRead
  {
    get
    {
      lock (this.portThreadSync)
      {
        lock (this.threadsync)
        {
          if (!this._isAlive)
            throw new Exception("The port has been disposed;");
          return this._port.BytesToRead;
        }
      }
    }
  }

  public int BytesToWrite
  {
    get
    {
      lock (this.portThreadSync)
      {
        lock (this.threadsync)
        {
          if (!this._isAlive)
            throw new Exception("The port has been disposed;");
          return this._port.BytesToWrite;
        }
      }
    }
  }

  public int WriteBufferSize
  {
    get
    {
      lock (this.portThreadSync)
      {
        lock (this.threadsync)
        {
          if (!this._isAlive)
            throw new Exception("The port has been disposed;");
          return this._port.WriteBufferSize;
        }
      }
    }
  }

  public int BaudRate
  {
    get
    {
      lock (this.portThreadSync)
      {
        lock (this.threadsync)
        {
          if (!this._isAlive)
            throw new Exception("The port has been disposed;");
          return this._port.BaudRate;
        }
      }
    }
    set
    {
      lock (this.portThreadSync)
      {
        lock (this.threadsync)
        {
          if (!this._isAlive)
            throw new Exception("The port has been disposed;");
          this._port.BaudRate = value;
        }
      }
    }
  }

  public Parity Parity
  {
    get
    {
      lock (this.portThreadSync)
      {
        lock (this.threadsync)
        {
          if (!this._isAlive)
            throw new Exception("The port has been disposed;");
          return this._port.Parity;
        }
      }
    }
    set
    {
      lock (this.portThreadSync)
      {
        lock (this.threadsync)
        {
          if (!this._isAlive)
            throw new Exception("The port has been disposed;");
          this._port.Parity = value;
        }
      }
    }
  }

  public StopBits StopBits
  {
    get
    {
      lock (this.portThreadSync)
      {
        lock (this.threadsync)
        {
          if (!this._isAlive)
            throw new Exception("The port has been disposed;");
          return this._port.StopBits;
        }
      }
    }
    set
    {
      lock (this.portThreadSync)
      {
        lock (this.threadsync)
        {
          if (!this._isAlive)
            throw new Exception("The port has been disposed;");
          this._port.StopBits = value;
        }
      }
    }
  }

  public Handshake Handshake
  {
    get
    {
      lock (this.portThreadSync)
      {
        lock (this.threadsync)
        {
          if (!this._isAlive)
            throw new Exception("The port has been disposed;");
          return this._port.Handshake;
        }
      }
    }
    set
    {
      lock (this.portThreadSync)
      {
        lock (this.threadsync)
        {
          if (!this._isAlive)
            throw new Exception("The port has been disposed;");
          this._port.Handshake = value;
        }
      }
    }
  }

  public bool IsOpen
  {
    get
    {
      lock (this.portThreadSync)
      {
        lock (this.threadsync)
        {
          if (!this._isAlive)
            return false;
          return this._port.IsOpen;
        }
      }
    }
  }

  public bool DtrEnable
  {
    get
    {
      lock (this.portThreadSync)
      {
        lock (this.threadsync)
        {
          if (!this._isAlive)
            throw new Exception("The port has been disposed;");
          return this._port.DtrEnable;
        }
      }
    }
    set
    {
      lock (this.portThreadSync)
      {
        lock (this.threadsync)
        {
          if (!this._isAlive)
            throw new Exception("The port has been disposed;");
          this._port.DtrEnable = value;
        }
      }
    }
  }

  public int WriteTimeout
  {
    get
    {
      lock (this.portThreadSync)
      {
        lock (this.threadsync)
        {
          if (!this._isAlive)
            throw new Exception("The port has been disposed;");
          return this._port.WriteTimeout;
        }
      }
    }
    set
    {
      lock (this.portThreadSync)
      {
        lock (this.threadsync)
        {
          if (!this._isAlive)
            throw new Exception("The port has been disposed;");
          this._port.WriteTimeout = value;
        }
      }
    }
  }

  public int ReadTimeout
  {
    get
    {
      lock (this.portThreadSync)
      {
        lock (this.threadsync)
        {
          if (!this._isAlive)
            throw new Exception("The port has been disposed;");
          return this._port.ReadTimeout;
        }
      }
    }
    set
    {
      lock (this.portThreadSync)
      {
        lock (this.threadsync)
        {
          if (!this._isAlive)
            throw new Exception("The port has been disposed;");
          this._port.ReadTimeout = value;
        }
      }
    }
  }

  public bool IsAlive
  {
    get
    {
      return this._isAlive;
    }
  }

  public object ThreadSync
  {
    get
    {
      return this.threadsync;
    }
  }

  public class Consts
  {
    public const string InitPortName = "COM1";
    public const int InitBaudRate = 9600;
    public const Parity InitParity = Parity.None;
    public const int InitDataBits = 8;
    public const StopBits InitStopBits = StopBits.One;
    public const bool InitDiscardNull = false;
    public const bool InitDtrEnable = false;
    public const Handshake InitHandshake = Handshake.None;
    public const byte InitParityReplace = 0;
    public const int InitReadBufferSize = 512;
    public const int InitReadTimeout = 0;
    public const int InitReceivedBytesThreshold = 1;
    public const bool InitRtsEnable = false;
    public const int InitWriteBufferSize = 1024;
    public const int InitWriteTimeout = 0;
  }
}
