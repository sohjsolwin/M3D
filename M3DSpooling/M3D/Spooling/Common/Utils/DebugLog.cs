using System;
using System.Diagnostics;
using System.IO;

namespace M3D.Spooling.Common.Utils
{
  public class DebugLog
  {
    private string functionName;
    private string description;
    private long memoryUsed;
    private long timeStamp;

    public DebugLog(string functionName, string description)
    {
      this.functionName = functionName;
      this.description = description;
      memoryUsed = Process.GetCurrentProcess().PrivateMemorySize64;
      timeStamp = DateTime.Now.Ticks;
    }

    public void Print(TextWriter tw)
    {
      var str = new DateTime(timeStamp).ToString("dd/MM/yyyy HH:mm:ss.ffff tt");
      tw.WriteLine(string.Format("{0}: {1} Current Memory Used: {2} bytes.  Date: {3}", functionName, description, memoryUsed, str));
    }
  }
}
