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
