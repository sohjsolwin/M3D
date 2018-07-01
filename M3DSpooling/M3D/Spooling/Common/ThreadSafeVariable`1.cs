// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.ThreadSafeVariable`1
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

namespace M3D.Spooling.Common
{
  public class ThreadSafeVariable<T>
  {
    private T __value;
    private object thread_lock;

    public ThreadSafeVariable()
    {
      this.thread_lock = new object();
    }

    public ThreadSafeVariable(T default_val)
      : this()
    {
      this.__value = default_val;
    }

    public T Value
    {
      get
      {
        lock (this.thread_lock)
          return this.__value;
      }
      set
      {
        lock (this.thread_lock)
          this.__value = value;
      }
    }
  }
}
