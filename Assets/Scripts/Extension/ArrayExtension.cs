using System.Collections.Generic;
using UnityEngine;

public static class ArrayExtensions
{
    public static T RandomElement<T>(this T[] items)
    {
        // Return a random item.
        return items[Random.Range(0, items.Length)];
    }

    public static T RandomElement<T>(this List<T> items)
    {
        return items[Random.Range(0, items.Count)];
    }
}