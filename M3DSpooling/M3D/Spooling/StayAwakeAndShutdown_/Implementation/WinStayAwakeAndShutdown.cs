// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.StayAwakeAndShutdown_.Implementation.WinStayAwakeAndShutdown
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System;
using System.Runtime.InteropServices;

namespace M3D.Spooling.StayAwakeAndShutdown_.Implementation
{
  internal class WinStayAwakeAndShutdown : IStayAwakeAndShutdown
  {
    private bool in_stay_awake_mode;
    private IntPtr hWnd;
    private uint fPreviousExecutionState;

    public WinStayAwakeAndShutdown(IntPtr hWnd)
    {
      this.hWnd = hWnd;
    }

    public void Shutdown()
    {
    }

    public bool NeverSleep()
    {
      if (!in_stay_awake_mode)
      {
        fPreviousExecutionState = WinStayAwakeAndShutdown.NativeMethods.SetThreadExecutionState(2147483649U);
        if (fPreviousExecutionState == 0U)
        {
          return false;
        }

        in_stay_awake_mode = true;
      }
      return true;
    }

    public void AllowSleep()
    {
      if (!in_stay_awake_mode)
      {
        return;
      }

      var num = (int) WinStayAwakeAndShutdown.NativeMethods.SetThreadExecutionState(fPreviousExecutionState);
      in_stay_awake_mode = false;
    }

    public void CreateShutdownMessage(string msg)
    {
      WinStayAwakeAndShutdown.NativeMethods.ShutdownBlockReasonCreate(hWnd, msg);
    }

    public void DestroyShutdownMessage()
    {
      WinStayAwakeAndShutdown.NativeMethods.ShutdownBlockReasonDestroy(hWnd);
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
