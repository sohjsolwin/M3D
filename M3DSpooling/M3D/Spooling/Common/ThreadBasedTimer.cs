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
      thread_sync = new object();
      _interval = 100;
      _isRunning = false;
      Tick = (EventHandler) null;
      this.shared_shutdown = shared_shutdown;
    }

    public void Start()
    {
      Start(false);
    }

    public void Start(bool isbackground)
    {
      lock (thread_sync)
      {
        if (_isRunning)
        {
          return;
        }

        _isRunning = true;
        runThread = new Thread(new ThreadStart(RunThread))
        {
          Name = "thread based timer",
          IsBackground = isbackground
        };
        runThread.Start();
      }
    }

    public void Stop()
    {
      lock (thread_sync)
      {
        if (!_isRunning)
        {
          return;
        }

        _isRunning = false;
        runThread.Abort();
        runThread = (Thread) null;
      }
    }

    public int Interval
    {
      get
      {
        lock (thread_sync)
        {
          return _interval;
        }
      }
      set
      {
        lock (thread_sync)
        {
          _interval = value;
        }
      }
    }

    private void RunThread()
    {
      try
      {
        while (!shared_shutdown.Value)
        {
          Thread.Sleep(Interval);
          if (Tick != null)
          {
            Tick((object) null, (EventArgs) null);
          }
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
        lock (thread_sync)
        {
          return _isRunning;
        }
      }
    }
  }
}
