using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class RingEffect : MonoBehaviour {
    [SerializeField]
    MeshRenderer meshRenderer;

    void Awake() {
        Hide();
    }

    public void Show() {
        meshRenderer.enabled = true;
    }

    public void Hide() {
        meshRenderer.enabled = false;
    }
}
