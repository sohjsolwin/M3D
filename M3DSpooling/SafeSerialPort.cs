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
      lock (threadsync)
      {
        if (portConfig == null)
        {
          throw new ArgumentNullException(nameof (portConfig));
        }

        _port = new SerialPort(portConfig.Name, portConfig.BaudRate, portConfig.Parity, portConfig.DataBits, portConfig.StopBits)
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
    lock (threadsync)
    {
      if (!_isAlive)
      {
        throw new Exception("The port has been disposed;");
      }

      return _port.ReadTo(Environment.NewLine);
    }
  }

  public void WriteLine(string text)
  {
    lock (portThreadSync)
    {
      lock (threadsync)
      {
        if (!_isAlive)
        {
          throw new Exception("The port has been disposed;");
        }

        _port.Write(text);
        _port.Write("\r");
      }
    }
  }

  public void Write(byte[] command, int offset, int count)
  {
    lock (portThreadSync)
    {
      lock (threadsync)
      {
        if (!_isAlive)
        {
          throw new Exception("The port has been disposed;");
        }

        _port.Write(command, offset, count);
      }
    }
  }

  public byte ReadByte()
  {
    lock (portThreadSync)
    {
      lock (threadsync)
      {
        if (!_isAlive)
        {
          throw new Exception("The port has been disposed;");
        }

        return (byte)_port.ReadByte();
      }
    }
  }

  public int Read(byte[] bytes, int offset, int count)
  {
    lock (portThreadSync)
    {
      lock (threadsync)
      {
        if (!_isAlive)
        {
          throw new Exception("The port has been disposed;");
        }

        return _port.Read(bytes, offset, count);
      }
    }
  }

  public string ReadExisting()
  {
    lock (portThreadSync)
    {
      lock (threadsync)
      {
        if (!_isAlive)
        {
          throw new Exception("The port has been disposed;");
        }

        return _port.ReadExisting();
      }
    }
  }

  public void Open()
  {
    lock (portThreadSync)
    {
      lock (threadsync)
      {
        if (!_isAlive)
        {
          throw new Exception("The port has been disposed;");
        }

        try
        {
          SerialPortFixer.Execute(_port.PortName);
        }
        catch (Exception ex)
        {
        }
        _port.Open();
        try
        {
          _internalSerialStream = _port.BaseStream;
          _port.DiscardInBuffer();
          _port.DiscardOutBuffer();
        }
        catch (Exception ex)
        {
          Stream internalSerialStream = _internalSerialStream;
          if (internalSerialStream == null)
          {
            FieldInfo field = typeof (SerialPort).GetField("internalSerialStream", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
              throw;
            }
            else
            {
              internalSerialStream = (Stream) field.GetValue(_port);
            }
          }
          logMessage = logMessage + "\nAn error occurred while constructing the serial port adaptor:" + ex.ToString();
          SafeDisconnect(_port, internalSerialStream);
          throw;
        }
      }
    }
  }

  public void Dispose()
  {
    lock (portThreadSync)
    {
      lock (threadsync)
      {
        if (!_isAlive)
        {
          return;
        }

        SafeDisconnect(_port, _internalSerialStream);
        GC.SuppressFinalize(this);
        _isAlive = false;
      }
    }
  }

  private void SafeDisconnect()
  {
    SafeDisconnect(_port, _internalSerialStream);
  }

  private void SafeDisconnect(SerialPort port, Stream internalSerialStream)
  {
    lock (portThreadSync)
    {
      lock (threadsync)
      {
        if (!_isAlive)
        {
          return;
        }

        if (port != null)
        {
          GC.SuppressFinalize(port);
        }

        if (internalSerialStream != null)
        {
          GC.SuppressFinalize(internalSerialStream);
          ShutdownEventLoopHandler(internalSerialStream);
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
    lock (portThreadSync)
    {
      lock (threadsync)
      {
        try
        {
          FieldInfo field1 = internalSerialStream.GetType().GetField("eventRunner", BindingFlags.Instance | BindingFlags.NonPublic);
          if (field1 == null)
          {
            return;
          }

          var obj = field1.GetValue(internalSerialStream);
          Type type = obj.GetType();
          FieldInfo field2 = type.GetField("endEventLoop", BindingFlags.Instance | BindingFlags.NonPublic);
          FieldInfo field3 = type.GetField("eventLoopEndedSignal", BindingFlags.Instance | BindingFlags.NonPublic);
          FieldInfo field4 = type.GetField("waitCommEventWaitHandle", BindingFlags.Instance | BindingFlags.NonPublic);
          if (field2 == null || field3 == null || field4 == null)
          {
            return;
          }

          var waitHandle = (WaitHandle) field3.GetValue(obj);
          var manualResetEvent = (ManualResetEvent) field4.GetValue(obj);
          field2.SetValue(obj, true);
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
      return _port.PortName;
    }
    set
    {
      _port.PortName = value;
    }
  }

  public int BytesToRead
  {
    get
    {
      lock (portThreadSync)
      {
        lock (threadsync)
        {
          if (!_isAlive)
          {
            throw new Exception("The port has been disposed;");
          }

          return _port.BytesToRead;
        }
      }
    }
  }

  public int BytesToWrite
  {
    get
    {
      lock (portThreadSync)
      {
        lock (threadsync)
        {
          if (!_isAlive)
          {
            throw new Exception("The port has been disposed;");
          }

          return _port.BytesToWrite;
        }
      }
    }
  }

  public int WriteBufferSize
  {
    get
    {
      lock (portThreadSync)
      {
        lock (threadsync)
        {
          if (!_isAlive)
          {
            throw new Exception("The port has been disposed;");
          }

          return _port.WriteBufferSize;
        }
      }
    }
  }

  public int BaudRate
  {
    get
    {
      lock (portThreadSync)
      {
        lock (threadsync)
        {
          if (!_isAlive)
          {
            throw new Exception("The port has been disposed;");
          }

          return _port.BaudRate;
        }
      }
    }
    set
    {
      lock (portThreadSync)
      {
        lock (threadsync)
        {
          if (!_isAlive)
          {
            throw new Exception("The port has been disposed;");
          }

          _port.BaudRate = value;
        }
      }
    }
  }

  public Parity Parity
  {
    get
    {
      lock (portThreadSync)
      {
        lock (threadsync)
        {
          if (!_isAlive)
          {
            throw new Exception("The port has been disposed;");
          }

          return _port.Parity;
        }
      }
    }
    set
    {
      lock (portThreadSync)
      {
        lock (threadsync)
        {
          if (!_isAlive)
          {
            throw new Exception("The port has been disposed;");
          }

          _port.Parity = value;
        }
      }
    }
  }

  public StopBits StopBits
  {
    get
    {
      lock (portThreadSync)
      {
        lock (threadsync)
        {
          if (!_isAlive)
          {
            throw new Exception("The port has been disposed;");
          }

          return _port.StopBits;
        }
      }
    }
    set
    {
      lock (portThreadSync)
      {
        lock (threadsync)
        {
          if (!_isAlive)
          {
            throw new Exception("The port has been disposed;");
          }

          _port.StopBits = value;
        }
      }
    }
  }

  public Handshake Handshake
  {
    get
    {
      lock (portThreadSync)
      {
        lock (threadsync)
        {
          if (!_isAlive)
          {
            throw new Exception("The port has been disposed;");
          }

          return _port.Handshake;
        }
      }
    }
    set
    {
      lock (portThreadSync)
      {
        lock (threadsync)
        {
          if (!_isAlive)
          {
            throw new Exception("The port has been disposed;");
          }

          _port.Handshake = value;
        }
      }
    }
  }

  public bool IsOpen
  {
    get
    {
      lock (portThreadSync)
      {
        lock (threadsync)
        {
          if (!_isAlive)
          {
            return false;
          }

          return _port.IsOpen;
        }
      }
    }
  }

  public bool DtrEnable
  {
    get
    {
      lock (portThreadSync)
      {
        lock (threadsync)
        {
          if (!_isAlive)
          {
            throw new Exception("The port has been disposed;");
          }

          return _port.DtrEnable;
        }
      }
    }
    set
    {
      lock (portThreadSync)
      {
        lock (threadsync)
        {
          if (!_isAlive)
          {
            throw new Exception("The port has been disposed;");
          }

          _port.DtrEnable = value;
        }
      }
    }
  }

  public int WriteTimeout
  {
    get
    {
      lock (portThreadSync)
      {
        lock (threadsync)
        {
          if (!_isAlive)
          {
            throw new Exception("The port has been disposed;");
          }

          return _port.WriteTimeout;
        }
      }
    }
    set
    {
      lock (portThreadSync)
      {
        lock (threadsync)
        {
          if (!_isAlive)
          {
            throw new Exception("The port has been disposed;");
          }

          _port.WriteTimeout = value;
        }
      }
    }
  }

  public int ReadTimeout
  {
    get
    {
      lock (portThreadSync)
      {
        lock (threadsync)
        {
          if (!_isAlive)
          {
            throw new Exception("The port has been disposed;");
          }

          return _port.ReadTimeout;
        }
      }
    }
    set
    {
      lock (portThreadSync)
      {
        lock (threadsync)
        {
          if (!_isAlive)
          {
            throw new Exception("The port has been disposed;");
          }

          _port.ReadTimeout = value;
        }
      }
    }
  }

  public bool IsAlive
  {
    get
    {
      return _isAlive;
    }
  }

  public object ThreadSync
  {
    get
    {
      return threadsync;
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
