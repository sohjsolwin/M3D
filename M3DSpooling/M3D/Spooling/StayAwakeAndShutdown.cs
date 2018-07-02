using M3D.Spooling.StayAwakeAndShutdown_.Implementation;
using System;

namespace M3D.Spooling
{
  public class StayAwakeAndShutdown
  {
    private IStayAwakeAndShutdown implementation;
    private bool stay_awake_set;

    public void StartUp(IntPtr hWnd)
    {
      implementation = (IStayAwakeAndShutdown) new WinStayAwakeAndShutdown(hWnd);
    }

    public void Shutdown()
    {
      implementation.Shutdown();
    }

    public bool NeverSleep()
    {
      if (stay_awake_set)
      {
        return true;
      }

      stay_awake_set = true;
      return implementation.NeverSleep();
    }

    public void AllowSleep()
    {
      if (!stay_awake_set)
      {
        return;
      }

      implementation.AllowSleep();
      stay_awake_set = false;
    }

    public void CreateShutdownMessage(string msg)
    {
      implementation.CreateShutdownMessage(msg);
    }

    public void DestroyShutdownMessage()
    {
      implementation.DestroyShutdownMessage();
    }

    public bool InStayAwakeMode()
    {
      return stay_awake_set;
    }
  }
}
