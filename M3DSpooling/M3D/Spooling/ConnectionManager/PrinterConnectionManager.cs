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
        throw new ArgumentException("Constructor to PrinterConnectionManager must take at least one argument.");
      foreach (VID_PID vid_pid in printer_vid_pid)
        this.printer_profiles.Add(new PrinterConnectionManager.PrinterTypeData(vid_pid));
      this.printerFinder = (IUSBPrinterFinder) new WinUSBPrinterFinder();
    }

    public void Start(ThreadSafeVariable<bool> shared_shutdown)
    {
      if (this.runThread != null)
        return;
      this.runThread = new SharedShutdownThread(new SharedShutdownThreadStart(this.Run), shared_shutdown);
      this.runThread.DelayBetweenIterations = 600;
      this.runThread.Name = nameof (PrinterConnectionManager);
      this.runThread.IsBackground = false;
      this.runThread.OnThreadAborted = new EventHandler<ThreadSafeVariable<bool>>(this.OnPrinterConnectionManagerStopped);
      this.runThread.Start();
    }

    private void OnPrinterConnectionManagerStopped(object sender, ThreadSafeVariable<bool> shared_shutdown)
    {
      foreach (KeyValuePair<string, PrinterConnection> printer in this.printers)
        printer.Value.Shutdown();
    }

    private bool Run()
    {
      try
      {
        List<string> stringList1 = new List<string>();
        List<PrinterConnEventArgs> printerConnEventArgsList = new List<PrinterConnEventArgs>();
        lock (this.printers)
        {
          foreach (KeyValuePair<string, PrinterConnection> printer in this.printers)
          {
            if (!stringList1.Contains(printer.Key))
              stringList1.Add(printer.Key);
          }
        }
        foreach (PrinterConnectionManager.PrinterTypeData printerProfile in this.printer_profiles)
        {
          foreach (string str in this.printerFinder.UsbAttached(printerProfile.vid_pid.VID, printerProfile.vid_pid.PID))
          {
            if (stringList1.Contains(str))
              stringList1.Remove(str);
          }
        }
        lock (this.printers)
        {
          foreach (KeyValuePair<string, PrinterConnection> printer in this.printers)
          {
            if (!printer.Value.IsAlive && !stringList1.Contains(printer.Key))
              stringList1.Add(printer.Key);
          }
        }
        foreach (string key in stringList1)
        {
          PrinterConnection printerToRemove;
          lock (this.printers)
          {
            if (!this.printers.TryRemove(key, out printerToRemove))
            {
              string message = "Already removed this printer :" + key;
              // ISSUE: reference to a compiler-generated field
              if (this.LogEventHandler != null)
              {
                // ISSUE: reference to a compiler-generated field
                this.LogEventHandler((object) this, new LogMessageEventArgs(message));
              }
            }
          }
          this.RemovePrinterHelper(printerToRemove);
          Thread.Sleep(10);
        }
        foreach (PrinterConnectionManager.PrinterTypeData printerProfile in this.printer_profiles)
        {
          List<string> stringList2 = this.printerFinder.UsbAttached(printerProfile.vid_pid.VID, printerProfile.vid_pid.PID);
          lock (this.printers)
          {
            foreach (string str in stringList2)
            {
              if (!string.IsNullOrEmpty(str) && !this.printers.ContainsKey(str))
                printerConnEventArgsList.Add(new PrinterConnEventArgs(str, printerProfile.vid_pid));
            }
          }
        }
        foreach (PrinterConnEventArgs e in printerConnEventArgsList)
        {
          lock (this.printers)
            this.NewPrinterHelper(e);
          Thread.Sleep(10);
        }
      }
      catch (Exception ex)
      {
        // ISSUE: reference to a compiler-generated field
        if (this.LogEventHandler != null)
        {
          // ISSUE: reference to a compiler-generated field
          this.LogEventHandler((object) this, new LogMessageEventArgs("Error: " + ex.Message));
        }
      }
      return true;
    }

    private void NewPrinterHelper(PrinterConnEventArgs e)
    {
      string comPort = e.com_port;
      PrinterConnection printer = new PrinterConnection(comPort);
      lock (printer.SerialPort.ThreadSync)
      {
        if (!printer.ConnectTo())
        {
          // ISSUE: reference to a compiler-generated field
          if (this.LogEventHandler == null)
            return;
          // ISSUE: reference to a compiler-generated field
          this.LogEventHandler((object) this, new LogMessageEventArgs("Could not connect to printer on port " + comPort));
        }
        else
        {
          // ISSUE: reference to a compiler-generated field
          if (this.LogEventHandler != null)
          {
            // ISSUE: reference to a compiler-generated field
            this.LogEventHandler((object) this, new LogMessageEventArgs("Successfully connected to printer on port " + comPort));
          }
          this.printers.TryAdd(comPort, printer);
          // ISSUE: reference to a compiler-generated field
          if (this.PrinterConnectedEventHandler == null)
            return;
          // ISSUE: reference to a compiler-generated field
          this.PrinterConnectedEventHandler((object) this, new PrinterConnEventArgs(printer, comPort, e.vid_pid));
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
          if (this.LogEventHandler != null)
          {
            // ISSUE: reference to a compiler-generated field
            this.LogEventHandler((object) this, new LogMessageEventArgs("Disconnecting from printer " + printerToRemove.SerialNumber + " on port " + printerToRemove.ComPort + "..."));
          }
          printerToRemove.SerialPort.Dispose();
          // ISSUE: reference to a compiler-generated field
          if (this.LogEventHandler != null)
          {
            // ISSUE: reference to a compiler-generated field
            this.LogEventHandler((object) this, new LogMessageEventArgs("Disconnected from printer " + printerToRemove.SerialNumber + " on port " + printerToRemove.ComPort ?? ""));
          }
        }
      }
      catch (Exception ex)
      {
        // ISSUE: reference to a compiler-generated field
        if (this.LogEventHandler != null)
        {
          // ISSUE: reference to a compiler-generated field
          this.LogEventHandler((object) this, new LogMessageEventArgs("Error: " + ex.Message));
        }
      }
      // ISSUE: reference to a compiler-generated field
      if (this.PrinterDisconnectedEventHandler == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.PrinterDisconnectedEventHandler((object) this, new PrinterConnEventArgs(printerToRemove));
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
