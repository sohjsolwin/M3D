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
      {
        return;
      }

      ErrorLogger.OnLogException(msg, e);
    }

    public static void LogErrorMsg(string msg)
    {
      ErrorLogger.LogErrorMsg(msg, "An problem was detected.");
    }

    public static void LogErrorMsg(string msg, string title)
    {
      if (ErrorLogger.OnLogErrorMsg == null)
      {
        return;
      }

      ErrorLogger.OnLogErrorMsg(msg, title);
    }

    public delegate void LogExceptionDel(string msg, Exception e);

    public delegate void LogErrorMsgDel(string msg, string title);
  }
}
