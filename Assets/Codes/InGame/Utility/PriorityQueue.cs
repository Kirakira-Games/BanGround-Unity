using System.Collections.Generic;
using System;

public class PriorityQueue<K, V> where K: IComparable
{
    private List<K> mKeys;
    private List<V> mValues;

    public PriorityQueue()
    {
        mKeys = new List<K>();
        mValues = new List<V>();
    }

    public V Top()
    {
        return mValues[0];
    }

    public bool Empty()
    {
        return mKeys.Count == 0;
    }

    public void Push(K key, V value)
    {
        int index = mKeys.Count - 1;
        while (index >= 0 && mKeys[index].CompareTo(key) > 0)
            index--;
        mKeys.Insert(index + 1, key);
        mValues.Insert(index + 1, value);
    }

    public void Pop()
    {
        mKeys.RemoveAt(0);
        mValues.RemoveAt(0);
    }
}