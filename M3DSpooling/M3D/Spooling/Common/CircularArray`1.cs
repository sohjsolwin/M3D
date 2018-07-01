// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.CircularArray`1
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System;
using System.Collections;
using System.Collections.Generic;

namespace M3D.Spooling.Common
{
  public class CircularArray<T> : IEnumerable<T>, IEnumerable
  {
    private T[] buffer;
    private int front;
    private int count;

    public CircularArray(int capacity)
    {
      this.buffer = new T[capacity];
      this.front = 0;
      this.count = 0;
    }

    public int Count
    {
      get
      {
        lock (this.buffer)
          return this.count;
      }
    }

    public void Enqueue(T item)
    {
      lock (this.buffer)
      {
        int index = (this.front + this.count) % this.buffer.Length;
        ++this.count;
        if (this.count >= this.buffer.Length)
        {
          this.front = (this.front + 1) % this.buffer.Length;
          this.count = this.buffer.Length;
        }
        this.buffer[index] = item;
      }
    }

    public T Dequeue()
    {
      lock (this.buffer)
      {
        if (this.count == 0)
          throw new InvalidOperationException("empty CircularArray<T>");
        T obj = this.buffer[this.front];
        this.front = (this.front + 1) % this.buffer.Length;
        --this.count;
        return obj;
      }
    }

    public T Peek()
    {
      lock (this.buffer)
      {
        if (this.count == 0)
          throw new InvalidOperationException("empty CircularArray<T>");
        return this.buffer[this.front];
      }
    }

    public void Clear()
    {
      lock (this.buffer)
      {
        this.front = 0;
        this.count = 0;
      }
    }

    public T this[int index]
    {
      get
      {
        lock (this.buffer)
        {
          if (index < 0 || index >= this.buffer.Length)
            throw new IndexOutOfRangeException();
          return this.buffer[(this.front + index) % this.buffer.Length];
        }
      }
      set
      {
        lock (this.buffer)
        {
          if (index < 0 || index >= this.buffer.Length)
            throw new IndexOutOfRangeException();
          this.buffer[(this.front + index) % this.buffer.Length] = value;
        }
      }
    }

    public bool isFull
    {
      get
      {
        lock (this.buffer)
          return this.count == this.buffer.Length;
      }
    }

    public bool isEmpty
    {
      get
      {
        lock (this.buffer)
          return this.count == 0;
      }
    }

    public IEnumerator<T> GetEnumerator()
    {
      if (this.Count != 0 && this.buffer.Length != 0)
      {
        for (int i = 0; i < this.Count; ++i)
          yield return this[i];
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator) this.GetEnumerator();
    }
  }
}
