// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Core.ErrorLogger
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System;

namespace M3D.Spooling.Core
{
  public static class ErrorLogger
  {
    public static ErrorLogger.LogExceptionDel OnLogException;
    public static ErrorLogger.LogErrorMsgDel OnLogErrorMsg;

    public static void LogException(string msg, Exception e)
    {
      if (ErrorLogger.OnLogException == null)
        return;
      ErrorLogger.OnLogException(msg, e);
    }

    public static void LogErrorMsg(string msg)
    {
      ErrorLogger.LogErrorMsg(msg, "An problem was detected.");
    }

    public static void LogErrorMsg(string msg, string title)
    {
      if (ErrorLogger.OnLogErrorMsg == null)
        return;
      ErrorLogger.OnLogErrorMsg(msg, title);
    }

    public delegate void LogExceptionDel(string msg, Exception e);

    public delegate void LogErrorMsgDel(string msg, string title);
  }
}
