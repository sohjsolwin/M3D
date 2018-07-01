// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Core.Logger
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
        return this._logReads.Value;
      }
      set
      {
        this._logReads.Value = value;
      }
    }

    public bool LogWrites
    {
      get
      {
        return this._logWrites.Value;
      }
      set
      {
        this._logWrites.Value = value;
      }
    }

    public bool LogFeedback
    {
      get
      {
        return this._logFeedback.Value;
      }
      set
      {
        this._logFeedback.Value = value;
      }
    }

    public bool LogWaits
    {
      get
      {
        return this._logWaits.Value;
      }
      set
      {
        this._logWaits.Value = value;
      }
    }

    public bool LogToFile
    {
      get
      {
        return this._logToFile.Value;
      }
      set
      {
        this._logToFile.Value = value;
      }
    }

    public bool LogToScreen
    {
      get
      {
        return this._logToScreen.Value;
      }
      set
      {
        this._logToScreen.Value = value;
      }
    }

    public string PrinterSerial
    {
      get
      {
        return this._printerSerial.Value;
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
        this._printerSerial = new ThreadSafeVariable<string>(printer_serial);
        path2 = printer_serial + "--" + DateTime.Now.ToString("yyyy-MM-dd_H-mm-ss") + ".txt";
      }
      else
      {
        this._printerSerial = new ThreadSafeVariable<string>("Detecting");
        path2 = Guid.NewGuid().ToString() + ".txt";
      }
      this._logFile = Path.Combine(Paths.LogPath, path2);
      this._logReads = new ThreadSafeVariable<bool>(true);
      this._logWrites = new ThreadSafeVariable<bool>(true);
      this._logWaits = new ThreadSafeVariable<bool>(false);
      this._logToFile = new ThreadSafeVariable<bool>(true);
      this._logFeedback = new ThreadSafeVariable<bool>(false);
      this._logToScreen = new ThreadSafeVariable<bool>(true);
      this._buffer = new List<string>();
      this._writeThread = new Thread(new ThreadStart(this.WriteThread));
      this._writeThread.IsBackground = true;
      this._writeThread.Name = "Write Log";
      this._writeThread.IsBackground = true;
      this._fileLock = new object();
      this._file = (StreamWriter) null;
      this._writeThread.Priority = ThreadPriority.Lowest;
      this._writeThread.IsBackground = true;
      this._writeThread.Start();
    }

    public void Shutdown()
    {
      this.threadAborted.Value = true;
      this._writeThread = (Thread) null;
    }

    public void ResetWithSerialNumber(string printer_serial)
    {
      this._printerSerial.Value = printer_serial;
      string destFileName = Path.Combine(Paths.LogPath, printer_serial + "--" + DateTime.Now.ToString("yyyy-MM-dd_H-mm-ss") + ".txt");
      lock (this._fileLock)
      {
        if (this._file != null)
        {
          this._file.Close();
          try
          {
            File.Move(this._logFile, destFileName);
          }
          catch (Exception ex)
          {
            this.ShowException(ex);
          }
          this._logFile = destFileName;
          try
          {
            this._file = new StreamWriter(this._logFile, true);
          }
          catch (Exception ex)
          {
            this.ShowException(ex);
          }
        }
        else
          this._logFile = destFileName;
      }
    }

    public void WriteLog(string text, Logger.TextType type)
    {
      if (type != Logger.TextType.Read && type != Logger.TextType.Wait && type != Logger.TextType.Feedback && ((type != Logger.TextType.Write || !this.LogWrites) && type != Logger.TextType.Error))
        return;
      if (this.LogToFile)
      {
        lock (this._buffer)
          this._buffer.Add(text);
      }
      if (!this.LogToScreen && type != Logger.TextType.Error || this._onLog == null)
        return;
      this._onLog(text, this.PrinterSerial);
    }

    private void ShowException(Exception e)
    {
      ErrorLogger.LogErrorMsg(this._printerSerial.ToString() + ": Unable to create log file. File logging disabled: " + e.Message, "File Logging Error");
      this.threadAborted.Value = true;
      this._writeThread = (Thread) null;
    }

    private void WriteThread()
    {
      do
      {
        Thread.Sleep(3000);
        this.writebufferToFile();
      }
      while (!this.threadAborted.Value && (this.shared_shutdown != null ? (!this.shared_shutdown.Value ? 1 : 0) : 1) != 0);
      try
      {
        if (this._file == null)
          return;
        this._file.Close();
      }
      catch (Exception ex)
      {
      }
    }

    private void writebufferToFile()
    {
      List<string> stringList;
      lock (this._buffer)
      {
        stringList = new List<string>((IEnumerable<string>) this._buffer);
        this._buffer.Clear();
      }
      if (stringList.Count == 0)
        return;
      lock (this._fileLock)
      {
        if (this._file == null)
        {
          try
          {
            this._file = new StreamWriter(this._logFile);
          }
          catch (Exception ex)
          {
            if (ex is ThreadAbortException)
              throw ex;
          }
        }
        foreach (string str in stringList)
        {
          try
          {
            this._file.WriteLine(str);
          }
          catch (Exception ex)
          {
            if (ex is ThreadAbortException)
              throw ex;
          }
        }
        try
        {
          this._file.Flush();
        }
        catch (Exception ex)
        {
          if (ex is ThreadAbortException)
            throw ex;
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
