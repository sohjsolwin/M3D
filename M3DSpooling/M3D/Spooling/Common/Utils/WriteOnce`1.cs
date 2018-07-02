// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Utils.WriteOnce`1
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System;

namespace M3D.Spooling.Common.Utils
{
  public class WriteOnce<T>
  {
    private T __value;
    private bool hasBeenSet;
    private object thread_lock;

    public WriteOnce(T default_val)
      : this()
    {
      __value = default_val;
    }

    public WriteOnce()
    {
      hasBeenSet = false;
      thread_lock = new object();
    }

    public T Value
    {
      get
      {
        lock (thread_lock)
        {
          return __value;
        }
      }
      set
      {
        lock (thread_lock)
        {
          if (hasBeenSet)
          {
            throw new InvalidOperationException("Value can only be set once.");
          }

          __value = value;
          hasBeenSet = true;
        }
      }
    }
  }
}
