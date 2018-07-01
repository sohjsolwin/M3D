// Decompiled with JetBrains decompiler
// Type: Nito.Deque`1
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Nito
{
  [DebuggerDisplay("Count = {Count}, Capacity = {Capacity}")]
  [DebuggerTypeProxy(typeof (Deque<>.DebugView))]
  internal sealed class Deque<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection
  {
    private const int DefaultCapacity = 8;
    private T[] buffer;
    private int offset;

    public Deque(int capacity)
    {
      if (capacity < 1)
        throw new ArgumentOutOfRangeException(nameof (capacity), "Capacity must be greater than 0.");
      this.buffer = new T[capacity];
    }

    public Deque(IEnumerable<T> collection)
    {
      int collectionCount = collection.Count<T>();
      if (collectionCount > 0)
      {
        this.buffer = new T[collectionCount];
        this.DoInsertRange(0, collection, collectionCount);
      }
      else
        this.buffer = new T[8];
    }

    public Deque()
      : this(8)
    {
    }

    bool ICollection<T>.IsReadOnly
    {
      get
      {
        return false;
      }
    }

    public T this[int index]
    {
      get
      {
        Deque<T>.CheckExistingIndexArgument(this.Count, index);
        return this.DoGetItem(index);
      }
      set
      {
        Deque<T>.CheckExistingIndexArgument(this.Count, index);
        this.DoSetItem(index, value);
      }
    }

    public void Insert(int index, T item)
    {
      Deque<T>.CheckNewIndexArgument(this.Count, index);
      this.DoInsert(index, item);
    }

    public void RemoveAt(int index)
    {
      Deque<T>.CheckExistingIndexArgument(this.Count, index);
      this.DoRemoveAt(index);
    }

    public int IndexOf(T item)
    {
      EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
      int num = 0;
      foreach (T y in this)
      {
        if (equalityComparer.Equals(item, y))
          return num;
        ++num;
      }
      return -1;
    }

    void ICollection<T>.Add(T item)
    {
      this.DoInsert(this.Count, item);
    }

    bool ICollection<T>.Contains(T item)
    {
      return this.Contains<T>(item, (IEqualityComparer<T>) null);
    }

    void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    {
      if (array == null)
        throw new ArgumentNullException(nameof (array), "Array is null");
      int count = this.Count;
      Deque<T>.CheckRangeArguments(array.Length, arrayIndex, count);
      for (int index = 0; index != count; ++index)
        array[arrayIndex + index] = this[index];
    }

    public bool Remove(T item)
    {
      int index = this.IndexOf(item);
      if (index == -1)
        return false;
      this.DoRemoveAt(index);
      return true;
    }

    public IEnumerator<T> GetEnumerator()
    {
      int count = this.Count;
      for (int i = 0; i != count; ++i)
        yield return this.DoGetItem(i);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator) this.GetEnumerator();
    }

    private bool ObjectIsT(object item)
    {
      if (item is T)
        return true;
      if (item == null)
      {
        Type type = typeof (T);
        if (type.IsClass && !type.IsPointer || type.IsInterface || type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>))
          return true;
      }
      return false;
    }

    int IList.Add(object value)
    {
      if (!this.ObjectIsT(value))
        throw new ArgumentException("Item is not of the correct type.", nameof (value));
      this.AddToBack((T) value);
      return this.Count - 1;
    }

    bool IList.Contains(object value)
    {
      if (!this.ObjectIsT(value))
        throw new ArgumentException("Item is not of the correct type.", nameof (value));
      return this.Contains<T>((T) value);
    }

    int IList.IndexOf(object value)
    {
      if (!this.ObjectIsT(value))
        throw new ArgumentException("Item is not of the correct type.", nameof (value));
      return this.IndexOf((T) value);
    }

    void IList.Insert(int index, object value)
    {
      if (!this.ObjectIsT(value))
        throw new ArgumentException("Item is not of the correct type.", nameof (value));
      this.Insert(index, (T) value);
    }

    bool IList.IsFixedSize
    {
      get
      {
        return false;
      }
    }

    bool IList.IsReadOnly
    {
      get
      {
        return false;
      }
    }

    void IList.Remove(object value)
    {
      if (!this.ObjectIsT(value))
        throw new ArgumentException("Item is not of the correct type.", nameof (value));
      this.Remove((T) value);
    }

    object IList.this[int index]
    {
      get
      {
        return (object) this[index];
      }
      set
      {
        if (!this.ObjectIsT(value))
          throw new ArgumentException("Item is not of the correct type.", nameof (value));
        this[index] = (T) value;
      }
    }

    void ICollection.CopyTo(Array array, int index)
    {
      if (array == null)
        throw new ArgumentNullException(nameof (array), "Destination array cannot be null.");
      Deque<T>.CheckRangeArguments(array.Length, index, this.Count);
      for (int index1 = 0; index1 != this.Count; ++index1)
      {
        try
        {
          array.SetValue((object) this[index1], index + index1);
        }
        catch (InvalidCastException ex)
        {
          throw new ArgumentException("Destination array is of incorrect type.", (Exception) ex);
        }
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

    private static void CheckNewIndexArgument(int sourceLength, int index)
    {
      if (index < 0 || index > sourceLength)
        throw new ArgumentOutOfRangeException(nameof (index), "Invalid new index " + (object) index + " for source length " + (object) sourceLength);
    }

    private static void CheckExistingIndexArgument(int sourceLength, int index)
    {
      if (index < 0 || index >= sourceLength)
        throw new ArgumentOutOfRangeException(nameof (index), "Invalid existing index " + (object) index + " for source length " + (object) sourceLength);
    }

    private static void CheckRangeArguments(int sourceLength, int offset, int count)
    {
      if (offset < 0)
        throw new ArgumentOutOfRangeException(nameof (offset), "Invalid offset " + (object) offset);
      if (count < 0)
        throw new ArgumentOutOfRangeException(nameof (count), "Invalid count " + (object) count);
      if (sourceLength - offset < count)
        throw new ArgumentException("Invalid offset (" + (object) offset + ") or count + (" + (object) count + ") for source length " + (object) sourceLength);
    }

    private bool IsEmpty
    {
      get
      {
        return this.Count == 0;
      }
    }

    private bool IsFull
    {
      get
      {
        return this.Count == this.Capacity;
      }
    }

    private bool IsSplit
    {
      get
      {
        return this.offset > this.Capacity - this.Count;
      }
    }

    public int Capacity
    {
      get
      {
        return this.buffer.Length;
      }
      set
      {
        if (value < 1)
          throw new ArgumentOutOfRangeException(nameof (value), "Capacity must be greater than 0.");
        if (value < this.Count)
          throw new InvalidOperationException("Capacity cannot be set to a value less than Count");
        if (value == this.buffer.Length)
          return;
        T[] objArray = new T[value];
        if (this.IsSplit)
        {
          int num = this.Capacity - this.offset;
          Array.Copy((Array) this.buffer, this.offset, (Array) objArray, 0, num);
          Array.Copy((Array) this.buffer, 0, (Array) objArray, num, this.Count - num);
        }
        else
          Array.Copy((Array) this.buffer, this.offset, (Array) objArray, 0, this.Count);
        this.buffer = objArray;
        this.offset = 0;
      }
    }

    public int Count { get; private set; }

    private int DequeIndexToBufferIndex(int index)
    {
      return (index + this.offset) % this.Capacity;
    }

    private T DoGetItem(int index)
    {
      return this.buffer[this.DequeIndexToBufferIndex(index)];
    }

    private void DoSetItem(int index, T item)
    {
      this.buffer[this.DequeIndexToBufferIndex(index)] = item;
    }

    private void DoInsert(int index, T item)
    {
      this.EnsureCapacityForOneElement();
      if (index == 0)
        this.DoAddToFront(item);
      else if (index == this.Count)
        this.DoAddToBack(item);
      else
        this.DoInsertRange(index, (IEnumerable<T>) new T[1]
        {
          item
        }, 1);
    }

    private void DoRemoveAt(int index)
    {
      if (index == 0)
        this.DoRemoveFromFront();
      else if (index == this.Count - 1)
        this.DoRemoveFromBack();
      else
        this.DoRemoveRange(index, 1);
    }

    private int PostIncrement(int value)
    {
      int offset = this.offset;
      this.offset += value;
      this.offset %= this.Capacity;
      return offset;
    }

    private int PreDecrement(int value)
    {
      this.offset -= value;
      if (this.offset < 0)
        this.offset += this.Capacity;
      return this.offset;
    }

    private void DoAddToBack(T value)
    {
      this.buffer[this.DequeIndexToBufferIndex(this.Count)] = value;
      ++this.Count;
    }

    private void DoAddToFront(T value)
    {
      this.buffer[this.PreDecrement(1)] = value;
      ++this.Count;
    }

    private T DoRemoveFromBack()
    {
      T obj = this.buffer[this.DequeIndexToBufferIndex(this.Count - 1)];
      --this.Count;
      return obj;
    }

    private T DoRemoveFromFront()
    {
      --this.Count;
      return this.buffer[this.PostIncrement(1)];
    }

    private void DoInsertRange(int index, IEnumerable<T> collection, int collectionCount)
    {
      if (index < this.Count / 2)
      {
        int num1 = index;
        int num2 = this.Capacity - collectionCount;
        for (int index1 = 0; index1 != num1; ++index1)
          this.buffer[this.DequeIndexToBufferIndex(num2 + index1)] = this.buffer[this.DequeIndexToBufferIndex(index1)];
        this.PreDecrement(collectionCount);
      }
      else
      {
        int num1 = this.Count - index;
        int num2 = index + collectionCount;
        int num3 = 1;
        for (int index1 = num1 - num3; index1 != -1; --index1)
          this.buffer[this.DequeIndexToBufferIndex(num2 + index1)] = this.buffer[this.DequeIndexToBufferIndex(index + index1)];
      }
      int index2 = index;
      foreach (T obj in collection)
      {
        this.buffer[this.DequeIndexToBufferIndex(index2)] = obj;
        ++index2;
      }
      this.Count += collectionCount;
    }

    private void DoRemoveRange(int index, int collectionCount)
    {
      if (index == 0)
      {
        this.PostIncrement(collectionCount);
        this.Count -= collectionCount;
      }
      else if (index == this.Count - collectionCount)
      {
        this.Count -= collectionCount;
      }
      else
      {
        if (index + collectionCount / 2 < this.Count / 2)
        {
          int num1 = index;
          int num2 = collectionCount;
          int num3 = 1;
          for (int index1 = num1 - num3; index1 != -1; --index1)
            this.buffer[this.DequeIndexToBufferIndex(num2 + index1)] = this.buffer[this.DequeIndexToBufferIndex(index1)];
          this.PostIncrement(collectionCount);
        }
        else
        {
          int num1 = this.Count - collectionCount - index;
          int num2 = index + collectionCount;
          for (int index1 = 0; index1 != num1; ++index1)
            this.buffer[this.DequeIndexToBufferIndex(index + index1)] = this.buffer[this.DequeIndexToBufferIndex(num2 + index1)];
        }
        this.Count -= collectionCount;
      }
    }

    private void EnsureCapacityForOneElement()
    {
      if (!this.IsFull)
        return;
      this.Capacity *= 2;
    }

    public void AddToBack(T value)
    {
      this.EnsureCapacityForOneElement();
      this.DoAddToBack(value);
    }

    public void AddToFront(T value)
    {
      this.EnsureCapacityForOneElement();
      this.DoAddToFront(value);
    }

    public void InsertRange(int index, IEnumerable<T> collection)
    {
      int collectionCount = collection.Count<T>();
      Deque<T>.CheckNewIndexArgument(this.Count, index);
      if (collectionCount > this.Capacity - this.Count)
        this.Capacity = checked (this.Count + collectionCount);
      if (collectionCount == 0)
        return;
      this.DoInsertRange(index, collection, collectionCount);
    }

    public void RemoveRange(int offset, int count)
    {
      Deque<T>.CheckRangeArguments(this.Count, offset, count);
      if (count == 0)
        return;
      this.DoRemoveRange(offset, count);
    }

    public T RemoveFromBack()
    {
      if (this.IsEmpty)
        throw new InvalidOperationException("The deque is empty.");
      return this.DoRemoveFromBack();
    }

    public T RemoveFromFront()
    {
      if (this.IsEmpty)
        throw new InvalidOperationException("The deque is empty.");
      return this.DoRemoveFromFront();
    }

    public void Clear()
    {
      this.offset = 0;
      this.Count = 0;
    }

    [DebuggerNonUserCode]
    private sealed class DebugView
    {
      private readonly Deque<T> deque;

      public DebugView(Deque<T> deque)
      {
        this.deque = deque;
      }

      [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
      public T[] Items
      {
        get
        {
          T[] array = new T[this.deque.Count];
          ((ICollection<T>) this.deque).CopyTo(array, 0);
          return array;
        }
      }
    }
  }
}
