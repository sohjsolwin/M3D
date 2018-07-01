// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Utils.DebugLog
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
      this.memoryUsed = Process.GetCurrentProcess().PrivateMemorySize64;
      this.timeStamp = DateTime.Now.Ticks;
    }

    public void Print(TextWriter tw)
    {
      string str = new DateTime(this.timeStamp).ToString("dd/MM/yyyy HH:mm:ss.ffff tt");
      tw.WriteLine(string.Format("{0}: {1} Current Memory Used: {2} bytes.  Date: {3}", (object) this.functionName, (object) this.description, (object) this.memoryUsed, (object) str));
    }
  }
}
