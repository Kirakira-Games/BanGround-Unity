using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

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

    public static V2.PVector3 ToProto(this Vector3 vector3)
    {
        return new V2.PVector3
        {
            x = vector3.x,
            y = vector3.y,
            z = vector3.z
        };
    }

    public static V2.PVector2 ToProto(this Vector2 vector2)
    {
        return new V2.PVector2
        {
            x = vector2.x,
            y = vector2.y
        };
    }

    public static Vector3 ToUnity(this V2.PVector3 vector3)
    {
        return new Vector3
        {
            x = vector3.x,
            y = vector3.y,
            z = vector3.z
        };
    }

    public static Vector3 ToUnity(this V2.PVector2 vector2)
    {
        return new Vector2
        {
            x = vector2.x,
            y = vector2.y
        };
    }
}
