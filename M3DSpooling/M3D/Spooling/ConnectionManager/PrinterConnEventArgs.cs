// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.ConnectionManager.PrinterConnEventArgs
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Core;
using System;

namespace M3D.Spooling.ConnectionManager
{
  internal class PrinterConnEventArgs : EventArgs
  {
    public PrinterConnection printer;
    public string com_port;
    public VID_PID vid_pid;

    public PrinterConnEventArgs(PrinterConnection printer)
    {
      this.printer = printer;
      if (printer == null || printer.SerialPort == null)
      {
        return;
      }

      com_port = printer.ComPort;
    }

    public PrinterConnEventArgs(string com_port, VID_PID vid_pid)
    {
      this.com_port = com_port;
      this.vid_pid = vid_pid;
    }

    public PrinterConnEventArgs(PrinterConnection printer, string com_port, VID_PID vid_pid)
    {
      this.printer = printer;
      this.com_port = com_port;
      this.vid_pid = vid_pid;
    }
  }
}
