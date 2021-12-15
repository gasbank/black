using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TransformExtension
{
    static Transform[] GetChildren(this Transform transform)
    {
        return Enumerable.Range(0, transform.childCount).Select(transform.GetChild).ToArray();
    }

    // where 절은 필수다. 없으면 null 체크 시 Unity 오브젝트 고려하지 않고 체크되기 때문
    public static T[] GetChildren<T>(this Transform transform) where T : Object
    {
        return Enumerable.Range(0, transform.childCount)
            .Select(transform.GetChild)
            .Select(e => e.GetComponent<T>())
            .Where(e => e != null)
            .ToArray();
    }

    public static void DestroyAllChildren(this Transform transform)
    {
        foreach (var c in GetChildren(transform)) Object.Destroy(c.gameObject);
    }

    public static void DestroyImmediateAllChildren(this Transform transform)
    {
        foreach (var c in GetChildren(transform)) Object.DestroyImmediate(c.gameObject);
    }

    public static void DestroyImmediateAllChildren<T>(this Transform transform) where T : Component
    {
        foreach (var c in GetChildren(transform))
            if (c.GetComponent<T>() != null)
                Object.DestroyImmediate(c.gameObject);
    }

    public static T[] GetComponentsInImmediateChildren<T>(this Transform transform)
    {
        var list = new List<T>();
        foreach (var c in GetChildren(transform))
        {
            var cc = c.GetComponent<T>();
            if (cc != null) list.Add(cc);
        }

        return list.ToArray();
    }

    public static T[] GetComponentsInChildrenWithName<T>(this Transform transform, string name) where T : Component
    {
        return transform.GetComponentsInChildren<T>().Where(e => e.name == name).ToArray();
    }

    public static T1[] GetComponentsInChildrenWithPruning<T1, T2>(this Transform transform)
    {
        var list = new List<T1>();
        GetComponentsInChildrenWithPruning<T1, T2>(transform, list);
        return list.ToArray();
    }

    static void GetComponentsInChildrenWithPruning<T1, T2>(Transform transform, List<T1> list)
    {
        foreach (var c in GetChildren(transform))
        {
            if (c.GetComponent<T2>() != null) continue;

            var cc = c.GetComponent<T1>();
            if (cc != null) list.Add(cc);

            GetComponentsInChildrenWithPruning<T1, T2>(c, list);
        }
    }
}