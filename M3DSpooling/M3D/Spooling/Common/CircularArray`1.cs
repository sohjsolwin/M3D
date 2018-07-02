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
      buffer = new T[capacity];
      front = 0;
      count = 0;
    }

    public int Count
    {
      get
      {
        lock (buffer)
        {
          return count;
        }
      }
    }

    public void Enqueue(T item)
    {
      lock (buffer)
      {
        var index = (front + count) % buffer.Length;
        ++count;
        if (count >= buffer.Length)
        {
          front = (front + 1) % buffer.Length;
          count = buffer.Length;
        }
        buffer[index] = item;
      }
    }

    public T Dequeue()
    {
      lock (buffer)
      {
        if (count == 0)
        {
          throw new InvalidOperationException("empty CircularArray<T>");
        }

        T obj = buffer[front];
        front = (front + 1) % buffer.Length;
        --count;
        return obj;
      }
    }

    public T Peek()
    {
      lock (buffer)
      {
        if (count == 0)
        {
          throw new InvalidOperationException("empty CircularArray<T>");
        }

        return buffer[front];
      }
    }

    public void Clear()
    {
      lock (buffer)
      {
        front = 0;
        count = 0;
      }
    }

    public T this[int index]
    {
      get
      {
        lock (buffer)
        {
          if (index < 0 || index >= buffer.Length)
          {
            throw new IndexOutOfRangeException();
          }

          return buffer[(front + index) % buffer.Length];
        }
      }
      set
      {
        lock (buffer)
        {
          if (index < 0 || index >= buffer.Length)
          {
            throw new IndexOutOfRangeException();
          }

          buffer[(front + index) % buffer.Length] = value;
        }
      }
    }

    public bool isFull
    {
      get
      {
        lock (buffer)
        {
          return count == buffer.Length;
        }
      }
    }

    public bool isEmpty
    {
      get
      {
        lock (buffer)
        {
          return count == 0;
        }
      }
    }

    public IEnumerator<T> GetEnumerator()
    {
      if (Count != 0 && buffer.Length != 0)
      {
        for (var i = 0; i < Count; ++i)
        {
          yield return this[i];
        }
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator)GetEnumerator();
    }
  }
}
