using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class PriorityQueue<V>
{
    private SortedDictionary<int, V> mPriorityDict = new SortedDictionary<int, V>();
    private Dictionary<V, int> mPriorityDictInverse = new Dictionary<V, int>();

    private void Add(V value, int priority)
    {
        mPriorityDict.Add(priority, value);
        mPriorityDictInverse.Add(value, priority);
    }

    public int Count => mPriorityDict.Count;

    public void Remove(V value)
    {
        if (!mPriorityDictInverse.TryGetValue(value, out int priority))
        {
            return;
        }
        mPriorityDict.Remove(priority);
        mPriorityDictInverse.Remove(value);
    }

    public void Push(V value, int priority)
    {
        if (mPriorityDict.ContainsKey(priority))
        {
            throw new InvalidDataException("Duplicate priority: " + priority);
        }
        Remove(value);
        Add(value, priority);
    }

    /// <returns>Item with smallest priority</returns>
    public V Peek()
    {
        return mPriorityDict.First().Value;
    }

    public V Pop()
    {
        var value = mPriorityDict.First();
        Remove(value.Value);
        return value.Value;
    }
}
