using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


public static class ExtMethods
{
    public static int Update<T1, T2>(this Dictionary<T1, T2> d, Func<T2, bool> where, Func<T2, T2> set)
    {
        var keys = d.Keys.ToArray();
        var counter = 0;

        for (int i = 0; i < keys.Length; i++)
        {
            var v = d[keys[i]];

            if (where(v))
            {
                d[keys[i]] = set(v);
                counter++;
            }
        }

        return counter;
    }

    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
    {
        key = kvp.Key;
        value = kvp.Value;
    }

    public static string[] ScanRegex(this string str, string format)
    {
        var results = Regex.Match(str, format).Groups;

        var objs = new List<string>();

        foreach (var result in results)
            objs.Add(result.ToString());

        return objs.ToArray();
    }
}
