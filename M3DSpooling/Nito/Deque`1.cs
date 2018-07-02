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
      {
        throw new ArgumentOutOfRangeException(nameof (capacity), "Capacity must be greater than 0.");
      }

      buffer = new T[capacity];
    }

    public Deque(IEnumerable<T> collection)
    {
      var collectionCount = collection.Count();
      if (collectionCount > 0)
      {
        buffer = new T[collectionCount];
        DoInsertRange(0, collection, collectionCount);
      }
      else
      {
        buffer = new T[8];
      }
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
        Deque<T>.CheckExistingIndexArgument(Count, index);
        return DoGetItem(index);
      }
      set
      {
        Deque<T>.CheckExistingIndexArgument(Count, index);
        DoSetItem(index, value);
      }
    }

    public void Insert(int index, T item)
    {
      Deque<T>.CheckNewIndexArgument(Count, index);
      DoInsert(index, item);
    }

    public void RemoveAt(int index)
    {
      Deque<T>.CheckExistingIndexArgument(Count, index);
      DoRemoveAt(index);
    }

    public int IndexOf(T item)
    {
      EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
      var num = 0;
      foreach (T y in this)
      {
        if (equalityComparer.Equals(item, y))
        {
          return num;
        }

        ++num;
      }
      return -1;
    }

    void ICollection<T>.Add(T item)
    {
      DoInsert(Count, item);
    }

    bool ICollection<T>.Contains(T item)
    {
      return this.Contains<T>(item, (IEqualityComparer<T>) null);
    }

    void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    {
      if (array == null)
      {
        throw new ArgumentNullException(nameof (array), "Array is null");
      }

      var count = Count;
      Deque<T>.CheckRangeArguments(array.Length, arrayIndex, count);
      for (var index = 0; index != count; ++index)
      {
        array[arrayIndex + index] = this[index];
      }
    }

    public bool Remove(T item)
    {
      var index = IndexOf(item);
      if (index == -1)
      {
        return false;
      }

      DoRemoveAt(index);
      return true;
    }

    public IEnumerator<T> GetEnumerator()
    {
      var count = Count;
      for (var i = 0; i != count; ++i)
      {
        yield return DoGetItem(i);
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator)GetEnumerator();
    }

    private bool ObjectIsT(object item)
    {
      if (item is T)
      {
        return true;
      }

      if (item == null)
      {
        Type type = typeof (T);
        if (type.IsClass && !type.IsPointer || type.IsInterface || type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>))
        {
          return true;
        }
      }
      return false;
    }

    int IList.Add(object value)
    {
      if (!ObjectIsT(value))
      {
        throw new ArgumentException("Item is not of the correct type.", nameof (value));
      }

      AddToBack((T) value);
      return Count - 1;
    }

    bool IList.Contains(object value)
    {
      if (!ObjectIsT(value))
      {
        throw new ArgumentException("Item is not of the correct type.", nameof (value));
      }

      return this.Contains<T>((T) value);
    }

    int IList.IndexOf(object value)
    {
      if (!ObjectIsT(value))
      {
        throw new ArgumentException("Item is not of the correct type.", nameof (value));
      }

      return IndexOf((T) value);
    }

    void IList.Insert(int index, object value)
    {
      if (!ObjectIsT(value))
      {
        throw new ArgumentException("Item is not of the correct type.", nameof (value));
      }

      Insert(index, (T) value);
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
      if (!ObjectIsT(value))
      {
        throw new ArgumentException("Item is not of the correct type.", nameof (value));
      }

      Remove((T) value);
    }

    object IList.this[int index]
    {
      get
      {
        return (object) this[index];
      }
      set
      {
        if (!ObjectIsT(value))
        {
          throw new ArgumentException("Item is not of the correct type.", nameof (value));
        }

        this[index] = (T) value;
      }
    }

    void ICollection.CopyTo(Array array, int index)
    {
      if (array == null)
      {
        throw new ArgumentNullException(nameof (array), "Destination array cannot be null.");
      }

      Deque<T>.CheckRangeArguments(array.Length, index, Count);
      for (var index1 = 0; index1 != Count; ++index1)
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
      {
        throw new ArgumentOutOfRangeException(nameof (index), "Invalid new index " + (object) index + " for source length " + (object) sourceLength);
      }
    }

    private static void CheckExistingIndexArgument(int sourceLength, int index)
    {
      if (index < 0 || index >= sourceLength)
      {
        throw new ArgumentOutOfRangeException(nameof (index), "Invalid existing index " + (object) index + " for source length " + (object) sourceLength);
      }
    }

    private static void CheckRangeArguments(int sourceLength, int offset, int count)
    {
      if (offset < 0)
      {
        throw new ArgumentOutOfRangeException(nameof (offset), "Invalid offset " + (object) offset);
      }

      if (count < 0)
      {
        throw new ArgumentOutOfRangeException(nameof (count), "Invalid count " + (object) count);
      }

      if (sourceLength - offset < count)
      {
        throw new ArgumentException("Invalid offset (" + (object) offset + ") or count + (" + (object) count + ") for source length " + (object) sourceLength);
      }
    }

    private bool IsEmpty
    {
      get
      {
        return Count == 0;
      }
    }

    private bool IsFull
    {
      get
      {
        return Count == Capacity;
      }
    }

    private bool IsSplit
    {
      get
      {
        return offset > Capacity - Count;
      }
    }

    public int Capacity
    {
      get
      {
        return buffer.Length;
      }
      set
      {
        if (value < 1)
        {
          throw new ArgumentOutOfRangeException(nameof (value), "Capacity must be greater than 0.");
        }

        if (value < Count)
        {
          throw new InvalidOperationException("Capacity cannot be set to a value less than Count");
        }

        if (value == buffer.Length)
        {
          return;
        }

        T[] objArray = new T[value];
        if (IsSplit)
        {
          var num = Capacity - offset;
          Array.Copy((Array)buffer, offset, (Array) objArray, 0, num);
          Array.Copy((Array)buffer, 0, (Array) objArray, num, Count - num);
        }
        else
        {
          Array.Copy((Array)buffer, offset, (Array) objArray, 0, Count);
        }

        buffer = objArray;
        offset = 0;
      }
    }

    public int Count { get; private set; }

    private int DequeIndexToBufferIndex(int index)
    {
      return (index + offset) % Capacity;
    }

    private T DoGetItem(int index)
    {
      return buffer[DequeIndexToBufferIndex(index)];
    }

    private void DoSetItem(int index, T item)
    {
      buffer[DequeIndexToBufferIndex(index)] = item;
    }

    private void DoInsert(int index, T item)
    {
      EnsureCapacityForOneElement();
      if (index == 0)
      {
        DoAddToFront(item);
      }
      else if (index == Count)
      {
        DoAddToBack(item);
      }
      else
      {
        DoInsertRange(index, (IEnumerable<T>) new T[1]
        {
          item
        }, 1);
      }
    }

    private void DoRemoveAt(int index)
    {
      if (index == 0)
      {
        DoRemoveFromFront();
      }
      else if (index == Count - 1)
      {
        DoRemoveFromBack();
      }
      else
      {
        DoRemoveRange(index, 1);
      }
    }

    private int PostIncrement(int value)
    {
      var offset = this.offset;
      this.offset += value;
      this.offset %= Capacity;
      return offset;
    }

    private int PreDecrement(int value)
    {
      offset -= value;
      if (offset < 0)
      {
        offset += Capacity;
      }

      return offset;
    }

    private void DoAddToBack(T value)
    {
      buffer[DequeIndexToBufferIndex(Count)] = value;
      ++Count;
    }

    private void DoAddToFront(T value)
    {
      buffer[PreDecrement(1)] = value;
      ++Count;
    }

    private T DoRemoveFromBack()
    {
      T obj = buffer[DequeIndexToBufferIndex(Count - 1)];
      --Count;
      return obj;
    }

    private T DoRemoveFromFront()
    {
      --Count;
      return buffer[PostIncrement(1)];
    }

    private void DoInsertRange(int index, IEnumerable<T> collection, int collectionCount)
    {
      if (index < Count / 2)
      {
        var num1 = index;
        var num2 = Capacity - collectionCount;
        for (var index1 = 0; index1 != num1; ++index1)
        {
          buffer[DequeIndexToBufferIndex(num2 + index1)] = buffer[DequeIndexToBufferIndex(index1)];
        }

        PreDecrement(collectionCount);
      }
      else
      {
        var num1 = Count - index;
        var num2 = index + collectionCount;
        var num3 = 1;
        for (var index1 = num1 - num3; index1 != -1; --index1)
        {
          buffer[DequeIndexToBufferIndex(num2 + index1)] = buffer[DequeIndexToBufferIndex(index + index1)];
        }
      }
      var index2 = index;
      foreach (T obj in collection)
      {
        buffer[DequeIndexToBufferIndex(index2)] = obj;
        ++index2;
      }
      Count += collectionCount;
    }

    private void DoRemoveRange(int index, int collectionCount)
    {
      if (index == 0)
      {
        PostIncrement(collectionCount);
        Count -= collectionCount;
      }
      else if (index == Count - collectionCount)
      {
        Count -= collectionCount;
      }
      else
      {
        if (index + collectionCount / 2 < Count / 2)
        {
          var num1 = index;
          var num2 = collectionCount;
          var num3 = 1;
          for (var index1 = num1 - num3; index1 != -1; --index1)
          {
            buffer[DequeIndexToBufferIndex(num2 + index1)] = buffer[DequeIndexToBufferIndex(index1)];
          }

          PostIncrement(collectionCount);
        }
        else
        {
          var num1 = Count - collectionCount - index;
          var num2 = index + collectionCount;
          for (var index1 = 0; index1 != num1; ++index1)
          {
            buffer[DequeIndexToBufferIndex(index + index1)] = buffer[DequeIndexToBufferIndex(num2 + index1)];
          }
        }
        Count -= collectionCount;
      }
    }

    private void EnsureCapacityForOneElement()
    {
      if (!IsFull)
      {
        return;
      }

      Capacity *= 2;
    }

    public void AddToBack(T value)
    {
      EnsureCapacityForOneElement();
      DoAddToBack(value);
    }

    public void AddToFront(T value)
    {
      EnsureCapacityForOneElement();
      DoAddToFront(value);
    }

    public void InsertRange(int index, IEnumerable<T> collection)
    {
      var collectionCount = collection.Count();
      Deque<T>.CheckNewIndexArgument(Count, index);
      if (collectionCount > Capacity - Count)
      {
        Capacity = checked (Count + collectionCount);
      }

      if (collectionCount == 0)
      {
        return;
      }

      DoInsertRange(index, collection, collectionCount);
    }

    public void RemoveRange(int offset, int count)
    {
      Deque<T>.CheckRangeArguments(Count, offset, count);
      if (count == 0)
      {
        return;
      }

      DoRemoveRange(offset, count);
    }

    public T RemoveFromBack()
    {
      if (IsEmpty)
      {
        throw new InvalidOperationException("The deque is empty.");
      }

      return DoRemoveFromBack();
    }

    public T RemoveFromFront()
    {
      if (IsEmpty)
      {
        throw new InvalidOperationException("The deque is empty.");
      }

      return DoRemoveFromFront();
    }

    public void Clear()
    {
      offset = 0;
      Count = 0;
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
          T[] array = new T[deque.Count];
          ((ICollection<T>)deque).CopyTo(array, 0);
          return array;
        }
      }
    }
  }
}
