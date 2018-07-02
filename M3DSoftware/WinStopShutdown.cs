using M3D.GUI.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace M3D
{
  internal class WinStopShutdown : IStopShutdown
  {
    private IntPtr hWnd;

    public WinStopShutdown(IntPtr hWnd)
    {
      this.hWnd = hWnd;
    }

    public void CreateShutdownMessage(string msg)
    {
      WinStopShutdown.NativeMethods.ShutdownBlockReasonCreate(hWnd, msg);
    }

    public void DestroyShutdownMessage()
    {
      WinStopShutdown.NativeMethods.ShutdownBlockReasonDestroy(hWnd);
    }

    internal static class NativeMethods
    {
      public const uint ES_CONTINUOUS = 2147483648;
      public const uint ES_SYSTEM_REQUIRED = 1;

      [DllImport("kernel32.dll")]
      public static extern uint SetThreadExecutionState(uint esFlags);

      [DllImport("user32.dll")]
      public static extern bool ShutdownBlockReasonCreate(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] string pwszReason);

      [DllImport("user32.dll")]
      public static extern bool ShutdownBlockReasonDestroy(IntPtr hWnd);
    }
  }
}
