// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.ConnectionManager.PrinterConnectionManager
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Common;
using M3D.Spooling.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace M3D.Spooling.ConnectionManager
{
  internal class PrinterConnectionManager
  {
    internal ConcurrentDictionary<string, PrinterConnection> printers = new ConcurrentDictionary<string, PrinterConnection>();
    private List<PrinterConnectionManager.PrinterTypeData> printer_profiles = new List<PrinterConnectionManager.PrinterTypeData>();
    private IUSBPrinterFinder printerFinder;
    private SharedShutdownThread runThread;

    internal event EventHandler<PrinterConnEventArgs> PrinterConnectedEventHandler;

    internal event EventHandler<PrinterConnEventArgs> PrinterDisconnectedEventHandler;

    internal event EventHandler<LogMessageEventArgs> LogEventHandler;

    public PrinterConnectionManager(params VID_PID[] printer_vid_pid)
    {
      if (printer_vid_pid == null || printer_vid_pid.Length < 1)
      {
        throw new ArgumentException("Constructor to PrinterConnectionManager must take at least one argument.");
      }

      foreach (VID_PID vid_pid in printer_vid_pid)
      {
        printer_profiles.Add(new PrinterConnectionManager.PrinterTypeData(vid_pid));
      }

      printerFinder = (IUSBPrinterFinder) new WinUSBPrinterFinder();
    }

    public void Start(ThreadSafeVariable<bool> shared_shutdown)
    {
      if (runThread != null)
      {
        return;
      }

      runThread = new SharedShutdownThread(new SharedShutdownThreadStart(Run), shared_shutdown)
      {
        DelayBetweenIterations = 600,
        Name = nameof(PrinterConnectionManager),
        IsBackground = false,
        OnThreadAborted = new EventHandler<ThreadSafeVariable<bool>>(OnPrinterConnectionManagerStopped)
      };
      runThread.Start();
    }

    private void OnPrinterConnectionManagerStopped(object sender, ThreadSafeVariable<bool> shared_shutdown)
    {
      foreach (KeyValuePair<string, PrinterConnection> printer in printers)
      {
        printer.Value.Shutdown();
      }
    }

    private bool Run()
    {
      try
      {
        var stringList1 = new List<string>();
        var printerConnEventArgsList = new List<PrinterConnEventArgs>();
        lock (printers)
        {
          foreach (KeyValuePair<string, PrinterConnection> printer in printers)
          {
            if (!stringList1.Contains(printer.Key))
            {
              stringList1.Add(printer.Key);
            }
          }
        }
        foreach (PrinterConnectionManager.PrinterTypeData printerProfile in printer_profiles)
        {
          foreach (var str in printerFinder.UsbAttached(printerProfile.vid_pid.VID, printerProfile.vid_pid.PID))
          {
            if (stringList1.Contains(str))
            {
              stringList1.Remove(str);
            }
          }
        }
        lock (printers)
        {
          foreach (KeyValuePair<string, PrinterConnection> printer in printers)
          {
            if (!printer.Value.IsAlive && !stringList1.Contains(printer.Key))
            {
              stringList1.Add(printer.Key);
            }
          }
        }
        foreach (var key in stringList1)
        {
          PrinterConnection printerToRemove;
          lock (printers)
          {
            if (!printers.TryRemove(key, out printerToRemove))
            {
              var message = "Already removed this printer :" + key;
              // ISSUE: reference to a compiler-generated field
              if (LogEventHandler != null)
              {
                // ISSUE: reference to a compiler-generated field
                LogEventHandler((object) this, new LogMessageEventArgs(message));
              }
            }
          }
          RemovePrinterHelper(printerToRemove);
          Thread.Sleep(10);
        }
        foreach (PrinterConnectionManager.PrinterTypeData printerProfile in printer_profiles)
        {
          List<string> stringList2 = printerFinder.UsbAttached(printerProfile.vid_pid.VID, printerProfile.vid_pid.PID);
          lock (printers)
          {
            foreach (var str in stringList2)
            {
              if (!string.IsNullOrEmpty(str) && !printers.ContainsKey(str))
              {
                printerConnEventArgsList.Add(new PrinterConnEventArgs(str, printerProfile.vid_pid));
              }
            }
          }
        }
        foreach (PrinterConnEventArgs e in printerConnEventArgsList)
        {
          lock (printers)
          {
            NewPrinterHelper(e);
          }

          Thread.Sleep(10);
        }
      }
      catch (Exception ex)
      {
        // ISSUE: reference to a compiler-generated field
        if (LogEventHandler != null)
        {
          // ISSUE: reference to a compiler-generated field
          LogEventHandler((object) this, new LogMessageEventArgs("Error: " + ex.Message));
        }
      }
      return true;
    }

    private void NewPrinterHelper(PrinterConnEventArgs e)
    {
      var comPort = e.com_port;
      var printer = new PrinterConnection(comPort);
      lock (printer.SerialPort.ThreadSync)
      {
        if (!printer.ConnectTo())
        {
          // ISSUE: reference to a compiler-generated field
          if (LogEventHandler == null)
          {
            return;
          }
          // ISSUE: reference to a compiler-generated field
          LogEventHandler((object) this, new LogMessageEventArgs("Could not connect to printer on port " + comPort));
        }
        else
        {
          // ISSUE: reference to a compiler-generated field
          if (LogEventHandler != null)
          {
            // ISSUE: reference to a compiler-generated field
            LogEventHandler((object) this, new LogMessageEventArgs("Successfully connected to printer on port " + comPort));
          }
          printers.TryAdd(comPort, printer);
          // ISSUE: reference to a compiler-generated field
          if (PrinterConnectedEventHandler == null)
          {
            return;
          }
          // ISSUE: reference to a compiler-generated field
          PrinterConnectedEventHandler((object) this, new PrinterConnEventArgs(printer, comPort, e.vid_pid));
        }
      }
    }

    private void RemovePrinterHelper(PrinterConnection printerToRemove)
    {
      try
      {
        if (printerToRemove.SerialPort != null)
        {
          // ISSUE: reference to a compiler-generated field
          if (LogEventHandler != null)
          {
            // ISSUE: reference to a compiler-generated field
            LogEventHandler((object) this, new LogMessageEventArgs("Disconnecting from printer " + printerToRemove.SerialNumber + " on port " + printerToRemove.ComPort + "..."));
          }
          printerToRemove.SerialPort.Dispose();
          // ISSUE: reference to a compiler-generated field
          if (LogEventHandler != null)
          {
            // ISSUE: reference to a compiler-generated field
            LogEventHandler((object) this, new LogMessageEventArgs("Disconnected from printer " + printerToRemove.SerialNumber + " on port " + printerToRemove.ComPort ?? ""));
          }
        }
      }
      catch (Exception ex)
      {
        // ISSUE: reference to a compiler-generated field
        if (LogEventHandler != null)
        {
          // ISSUE: reference to a compiler-generated field
          LogEventHandler((object) this, new LogMessageEventArgs("Error: " + ex.Message));
        }
      }
      // ISSUE: reference to a compiler-generated field
      if (PrinterDisconnectedEventHandler == null)
      {
        return;
      }
      // ISSUE: reference to a compiler-generated field
      PrinterDisconnectedEventHandler((object) this, new PrinterConnEventArgs(printerToRemove));
    }

    internal class PrinterTypeData
    {
      public readonly VID_PID vid_pid;

      public PrinterTypeData(VID_PID vid_pid)
      {
        this.vid_pid = vid_pid;
      }
    }
  }
}
