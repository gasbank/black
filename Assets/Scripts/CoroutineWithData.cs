// https://answers.unity.com/questions/24640/how-do-i-return-a-value-from-a-coroutine.html

using System.Collections;
using UnityEngine;

public class CoroutineWithData
{
    public object result;
    readonly IEnumerator target;

    public CoroutineWithData(MonoBehaviour owner, IEnumerator target)
    {
        this.target = target;
        coroutine = owner.StartCoroutine(Run());
    }

    public Coroutine coroutine { get; }

    IEnumerator Run()
    {
        while (target.MoveNext())
        {
            result = target.Current;
            yield return result;
        }
    }
}