using System.Collections.Generic;
using System;

public class PriorityQueue<K, V> where K: IComparable
{
    private LinkedList<K> mKeys;
    private LinkedList<V> mValues;

    public int Count => mKeys.Count;
    public V Top => mValues.First.Value;
    public bool Empty => Count == 0;
    public LinkedListNode<V> FirstV => mValues.First;
    public LinkedListNode<V> LastV => mValues.Last;
    public LinkedListNode<K> FirstK => mKeys.First;
    public LinkedListNode<K> LastK => mKeys.Last;

    public PriorityQueue()
    {
        mKeys = new LinkedList<K>();
        mValues = new LinkedList<V>();
    }

    public void Push(K key, V value)
    {
        var keyPtr = mKeys.Last;
        var valPtr = mValues.Last;
        while (keyPtr != null && keyPtr.Value.CompareTo(key) > 0)
        {
            keyPtr = keyPtr.Previous;
            valPtr = valPtr.Previous;
        }
        if (keyPtr == null)
        {
            mKeys.AddFirst(key);
            mValues.AddFirst(value);
        }
        else
        {
            mKeys.AddAfter(keyPtr, key);
            mValues.AddAfter(valPtr, value);
        }
    }

    public void RemoveFirst()
    {
        mKeys.RemoveFirst();
        mValues.RemoveFirst();
    }

    public void RemoveLast()
    {
        mKeys.RemoveLast();
        mValues.RemoveLast();
    }

    public void Clear()
    {
        mKeys.Clear();
        mValues.Clear();
    }
}