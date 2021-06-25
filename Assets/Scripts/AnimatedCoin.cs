using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedCoin : MonoBehaviour {
    [SerializeField] RectTransform targetRt;
    [SerializeField] RectTransform rt;
    [SerializeField] Vector3 currentVelocityPosition;
    [SerializeField] float smoothTimePosition = 0.1f;
    [SerializeField] Vector2 currentVelocitySize;
    [SerializeField] float smoothTimeSize = 0.1f;
    [SerializeField] GridWorld gridWorld;

    public RectTransform Rt => rt;
    public RectTransform TargetRt {
        get => targetRt;
        set => targetRt = value;
    }
    public GridWorld GridWorld {
        get => gridWorld;
        set => gridWorld = value;
    }

#if UNITY_EDITOR
    void OnValidate() {
        rt = GetComponent<RectTransform>();
    }
#endif
    
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
            GridWorld.Coin++;
        }
    }
}
