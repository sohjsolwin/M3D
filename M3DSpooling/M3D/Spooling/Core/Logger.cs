using M3D.Spooling.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace M3D.Spooling.Core
{
  public class Logger
  {
    private ThreadSafeVariable<bool> threadAborted = new ThreadSafeVariable<bool>(false);
    public Logger.OnLogDel _onLog;
    public bool _logAll;
    private StreamWriter _file;
    private Thread _writeThread;
    private string _logFile;
    private List<string> _buffer;
    private ThreadSafeVariable<string> _printerSerial;
    private ThreadSafeVariable<bool> shared_shutdown;
    private object _fileLock;
    private ThreadSafeVariable<bool> _logReads;
    private ThreadSafeVariable<bool> _logWrites;
    private ThreadSafeVariable<bool> _logWaits;
    private ThreadSafeVariable<bool> _logToFile;
    private ThreadSafeVariable<bool> _logFeedback;
    private ThreadSafeVariable<bool> _logToScreen;

    public bool LogReads
    {
      get
      {
        return _logReads.Value;
      }
      set
      {
        _logReads.Value = value;
      }
    }

    public bool LogWrites
    {
      get
      {
        return _logWrites.Value;
      }
      set
      {
        _logWrites.Value = value;
      }
    }

    public bool LogFeedback
    {
      get
      {
        return _logFeedback.Value;
      }
      set
      {
        _logFeedback.Value = value;
      }
    }

    public bool LogWaits
    {
      get
      {
        return _logWaits.Value;
      }
      set
      {
        _logWaits.Value = value;
      }
    }

    public bool LogToFile
    {
      get
      {
        return _logToFile.Value;
      }
      set
      {
        _logToFile.Value = value;
      }
    }

    public bool LogToScreen
    {
      get
      {
        return _logToScreen.Value;
      }
      set
      {
        _logToScreen.Value = value;
      }
    }

    public string PrinterSerial
    {
      get
      {
        return _printerSerial.Value;
      }
    }

    public Logger(ThreadSafeVariable<bool> shared_shutdown)
      : this(shared_shutdown, (string) null)
    {
    }

    public Logger(ThreadSafeVariable<bool> shared_shutdown, string printer_serial)
    {
      this.shared_shutdown = shared_shutdown;
      string path2;
      if (!string.IsNullOrEmpty(printer_serial))
      {
        _printerSerial = new ThreadSafeVariable<string>(printer_serial);
        path2 = printer_serial + "--" + DateTime.Now.ToString("yyyy-MM-dd_H-mm-ss") + ".txt";
      }
      else
      {
        _printerSerial = new ThreadSafeVariable<string>("Detecting");
        path2 = Guid.NewGuid().ToString() + ".txt";
      }
      _logFile = Path.Combine(Paths.LogPath, path2);
      _logReads = new ThreadSafeVariable<bool>(true);
      _logWrites = new ThreadSafeVariable<bool>(true);
      _logWaits = new ThreadSafeVariable<bool>(false);
      _logToFile = new ThreadSafeVariable<bool>(true);
      _logFeedback = new ThreadSafeVariable<bool>(false);
      _logToScreen = new ThreadSafeVariable<bool>(true);
      _buffer = new List<string>();
      _writeThread = new Thread(new ThreadStart(WriteThread))
      {
        IsBackground = true,
        Name = "Write Log"
      };
      _writeThread.IsBackground = true;
      _fileLock = new object();
      _file = (StreamWriter) null;
      _writeThread.Priority = ThreadPriority.Lowest;
      _writeThread.IsBackground = true;
      _writeThread.Start();
    }

    public void Shutdown()
    {
      threadAborted.Value = true;
      _writeThread = (Thread) null;
    }

    public void ResetWithSerialNumber(string printer_serial)
    {
      _printerSerial.Value = printer_serial;
      var destFileName = Path.Combine(Paths.LogPath, printer_serial + "--" + DateTime.Now.ToString("yyyy-MM-dd_H-mm-ss") + ".txt");
      lock (_fileLock)
      {
        if (_file != null)
        {
          _file.Close();
          try
          {
            File.Move(_logFile, destFileName);
          }
          catch (Exception ex)
          {
            ShowException(ex);
          }
          _logFile = destFileName;
          try
          {
            _file = new StreamWriter(_logFile, true);
          }
          catch (Exception ex)
          {
            ShowException(ex);
          }
        }
        else
        {
          _logFile = destFileName;
        }
      }
    }

    public void WriteLog(string text, Logger.TextType type)
    {
      if (type != Logger.TextType.Read && type != Logger.TextType.Wait && type != Logger.TextType.Feedback && ((type != Logger.TextType.Write || !LogWrites) && type != Logger.TextType.Error))
      {
        return;
      }

      if (LogToFile)
      {
        lock (_buffer)
        {
          _buffer.Add(text);
        }
      }
      if (!LogToScreen && type != Logger.TextType.Error || _onLog == null)
      {
        return;
      }

      _onLog(text, PrinterSerial);
    }

    private void ShowException(Exception e)
    {
      ErrorLogger.LogErrorMsg(_printerSerial.ToString() + ": Unable to create log file. File logging disabled: " + e.Message, "File Logging Error");
      threadAborted.Value = true;
      _writeThread = (Thread) null;
    }

    private void WriteThread()
    {
      do
      {
        Thread.Sleep(3000);
        writebufferToFile();
      }
      while (!threadAborted.Value && (shared_shutdown != null ? (!shared_shutdown.Value ? 1 : 0) : 1) != 0);
      try
      {
        if (_file == null)
        {
          return;
        }

        _file.Close();
      }
      catch (Exception ex)
      {
      }
    }

    private void writebufferToFile()
    {
      List<string> stringList;
      lock (_buffer)
      {
        stringList = new List<string>((IEnumerable<string>)_buffer);
        _buffer.Clear();
      }
      if (stringList.Count == 0)
      {
        return;
      }

      lock (_fileLock)
      {
        if (_file == null)
        {
          try
          {
            _file = new StreamWriter(_logFile);
          }
          catch (Exception ex)
          {
            if (ex is ThreadAbortException)
            {
              throw ex;
            }
          }
        }
        foreach (var str in stringList)
        {
          try
          {
            _file.WriteLine(str);
          }
          catch (Exception ex)
          {
            if (ex is ThreadAbortException)
            {
              throw ex;
            }
          }
        }
        try
        {
          _file.Flush();
        }
        catch (Exception ex)
        {
          if (ex is ThreadAbortException)
          {
            throw ex;
          }
        }
      }
    }

    public enum TextType
    {
      None,
      Write,
      Read,
      Wait,
      Feedback,
      Error,
    }

    public delegate void OnLogDel(string message, string printer_serial);
  }
}
