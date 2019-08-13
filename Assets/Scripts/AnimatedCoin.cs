using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedCoin : MonoBehaviour {
    [SerializeField] RectTransform targetRt = null;
    [SerializeField] RectTransform rt = null;
    [SerializeField] Vector3 currentVelocityPosition;
    [SerializeField] float smoothTimePosition = 0.1f;
    [SerializeField] Vector2 currentVelocitySize;
    [SerializeField] float smoothTimeSize = 0.1f;

    public RectTransform Rt => rt;
    public RectTransform TargetRt {
        get => targetRt;
        set => targetRt = value;
    }

    void OnValidate() {
        rt = GetComponent<RectTransform>();
    }

    void Awake() {
        if (targetRt == null) {
            targetRt = rt;
        }
    }

    void Update() {
        rt.position = Vector3.SmoothDamp(rt.position, targetRt.position, ref currentVelocityPosition, smoothTimePosition);
        rt.sizeDelta = Vector2.SmoothDamp(rt.sizeDelta, targetRt.rect.size, ref currentVelocitySize, smoothTimeSize);

        if (rt != targetRt && currentVelocityPosition.magnitude < 0.25f) {
            Destroy(gameObject);
        }
    }
}
