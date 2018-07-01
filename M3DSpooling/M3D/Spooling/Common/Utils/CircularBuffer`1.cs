// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Utils.CircularBuffer`1
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System;
using System.Collections;
using System.Collections.Generic;

namespace M3D.Spooling.Common.Utils
{
  public class CircularBuffer<T> : ICollection<T>, IEnumerable<T>, IEnumerable, ICollection
  {
    private int capacity;
    private Queue<T> buffer;

    public CircularBuffer(int capacity)
    {
      this.capacity = capacity;
      this.buffer = new Queue<T>(capacity);
    }

    public int Capacity
    {
      get
      {
        return this.capacity;
      }
    }

    public int Count
    {
      get
      {
        return this.buffer.Count;
      }
    }

    public T this[int index]
    {
      get
      {
        return this.buffer.ToArray()[index];
      }
    }

    public virtual T Add(T item)
    {
      T obj = default (T);
      if (this.buffer.Count == this.capacity)
        obj = this.buffer.Dequeue();
      this.buffer.Enqueue(item);
      return obj;
    }

    public virtual void Clear()
    {
      this.buffer.Clear();
    }

    int ICollection<T>.Count
    {
      get
      {
        return this.Count;
      }
    }

    bool ICollection<T>.IsReadOnly
    {
      get
      {
        return false;
      }
    }

    void ICollection<T>.Add(T item)
    {
      this.Add(item);
    }

    bool ICollection<T>.Remove(T item)
    {
      throw new NotImplementedException("Items can not be removed from circular buffer");
    }

    bool ICollection<T>.Contains(T item)
    {
      return this.buffer.Contains(item);
    }

    void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    {
      this.buffer.CopyTo(array, arrayIndex);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return (IEnumerator<T>) this.buffer.GetEnumerator();
    }

    int ICollection.Count
    {
      get
      {
        return this.Count;
      }
    }

    bool ICollection.IsSynchronized
    {
      get
      {
        return false;
      }
    }

    object ICollection.SyncRoot
    {
      get
      {
        return (object) this;
      }
    }

    void ICollection.CopyTo(Array array, int arrayIndex)
    {
      this.buffer.CopyTo((T[]) array, arrayIndex);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator) this.buffer.GetEnumerator();
    }
  }
}
