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
