// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.StayAwakeAndShutdown
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
      this.implementation = (IStayAwakeAndShutdown) new WinStayAwakeAndShutdown(hWnd);
    }

    public void Shutdown()
    {
      this.implementation.Shutdown();
    }

    public bool NeverSleep()
    {
      if (this.stay_awake_set)
        return true;
      this.stay_awake_set = true;
      return this.implementation.NeverSleep();
    }

    public void AllowSleep()
    {
      if (!this.stay_awake_set)
        return;
      this.implementation.AllowSleep();
      this.stay_awake_set = false;
    }

    public void CreateShutdownMessage(string msg)
    {
      this.implementation.CreateShutdownMessage(msg);
    }

    public void DestroyShutdownMessage()
    {
      this.implementation.DestroyShutdownMessage();
    }

    public bool InStayAwakeMode()
    {
      return this.stay_awake_set;
    }
  }
}
