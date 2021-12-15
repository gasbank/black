using UnityEngine;

public class AnimatedCoin : MonoBehaviour
{
    [SerializeField]
    Vector3 currentVelocityPosition;

    [SerializeField]
    Vector2 currentVelocitySize;

    [SerializeField]
    GridWorld gridWorld;

    [SerializeField]
    RectTransform rt;

    [SerializeField]
    float smoothTimePosition = 0.1f;

    [SerializeField]
    float smoothTimeSize = 0.1f;

    public RectTransform Rt => rt;

    [field: SerializeField]
    public RectTransform TargetRt { get; set; }

    public GridWorld GridWorld
    {
        get => gridWorld;
        set => gridWorld = value;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        rt = GetComponent<RectTransform>();
    }
#endif

    void Awake()
    {
        if (TargetRt == null) TargetRt = rt;
    }

    void Update()
    {
        rt.position =
            Vector3.SmoothDamp(rt.position, TargetRt.position, ref currentVelocityPosition, smoothTimePosition);
        rt.sizeDelta = Vector2.SmoothDamp(rt.sizeDelta, TargetRt.rect.size, ref currentVelocitySize, smoothTimeSize);

        if (rt != TargetRt && currentVelocityPosition.magnitude < 0.25f)
        {
            Destroy(gameObject);
            GridWorld.Coin++;
        }
    }
}