// https://answers.unity.com/questions/24640/how-do-i-return-a-value-from-a-coroutine.html
using System.Collections;
using UnityEngine;

public class CoroutineWithData {
    public Coroutine coroutine { get; private set; }
    public object result;
    IEnumerator target;
    public CoroutineWithData(MonoBehaviour owner, IEnumerator target) {
        this.target = target;
        this.coroutine = owner.StartCoroutine(Run());
    }

    IEnumerator Run() {
        while (target.MoveNext()) {
            result = target.Current;
            yield return result;
        }
    }
}