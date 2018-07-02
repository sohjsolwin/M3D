using System;

namespace M3D.Spooling.ConnectionManager
{
  public class LogMessageEventArgs : EventArgs
  {
    public string message;

    public LogMessageEventArgs(string message)
    {
      this.message = message;
    }
  }
}
