namespace M3D.Spooling.Common
{
  public class ThreadSafeVariable<T>
  {
    private T __value;
    private object thread_lock;

    public ThreadSafeVariable()
    {
      thread_lock = new object();
    }

    public ThreadSafeVariable(T default_val)
      : this()
    {
      __value = default_val;
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
          __value = value;
        }
      }
    }
  }
}
