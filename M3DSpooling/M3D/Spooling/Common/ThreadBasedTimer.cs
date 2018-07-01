// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.ThreadBasedTimer
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Core;
using System;
using System.ComponentModel;
using System.Threading;

namespace M3D.Spooling.Common
{
  public class ThreadBasedTimer
  {
    private ThreadSafeVariable<bool> shared_shutdown;
    private Thread runThread;
    public EventHandler Tick;
    private int _interval;
    private bool _isRunning;
    private object thread_sync;

    public ThreadBasedTimer(IContainer not_used, ThreadSafeVariable<bool> shared_shutdown)
    {
      this.thread_sync = new object();
      this._interval = 100;
      this._isRunning = false;
      this.Tick = (EventHandler) null;
      this.shared_shutdown = shared_shutdown;
    }

    public void Start()
    {
      this.Start(false);
    }

    public void Start(bool isbackground)
    {
      lock (this.thread_sync)
      {
        if (this._isRunning)
          return;
        this._isRunning = true;
        this.runThread = new Thread(new ThreadStart(this.RunThread));
        this.runThread.Name = "thread based timer";
        this.runThread.IsBackground = isbackground;
        this.runThread.Start();
      }
    }

    public void Stop()
    {
      lock (this.thread_sync)
      {
        if (!this._isRunning)
          return;
        this._isRunning = false;
        this.runThread.Abort();
        this.runThread = (Thread) null;
      }
    }

    public int Interval
    {
      get
      {
        lock (this.thread_sync)
          return this._interval;
      }
      set
      {
        lock (this.thread_sync)
          this._interval = value;
      }
    }

    private void RunThread()
    {
      try
      {
        while (!this.shared_shutdown.Value)
        {
          Thread.Sleep(this.Interval);
          if (this.Tick != null)
            this.Tick((object) null, (EventArgs) null);
        }
      }
      catch (ThreadAbortException ex)
      {
        throw ex;
      }
      catch (Exception ex)
      {
        ErrorLogger.LogException(nameof (ThreadBasedTimer), ex);
      }
    }

    public bool IsRunning
    {
      get
      {
        lock (this.thread_sync)
          return this._isRunning;
      }
    }
  }
}
