using System.Linq;
using UnityEngine;

public class Miniroom : MonoBehaviour {
    void Awake() {
        foreach (var t in transform.GetComponentsInChildren<CanvasGroupAlpha>(true)
            .Where(e => e.transform.childCount == 0)) {
            t.SetTargetAlphaZero();
        }
    }
}