using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Character : MonoBehaviour
{
    [SerializeField]
    Character3D char3D;

    [SerializeField]
    RectTransform rt;

    [SerializeField]
    RectTransform parentRt;

    [SerializeField]
    Camera cam;

    [SerializeField]
    Image image;

    [SerializeField]
    Sprite sprite1;
    
    [SerializeField]
    Sprite sprite2;

    public Character3D Char3D
    {
        get => char3D;
        set => char3D = value;
    }

    public Camera Cam { get => cam; set => cam = value; }
    public Sprite Sprite1 { get => sprite1; set => sprite1 = value; }
    public Sprite Sprite2 { get => sprite2; set => sprite2 = value; }

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);

        UpdateRectTransformReferences();
    }
#endif

    void UpdateRectTransformReferences()
    {
        rt = GetComponent<RectTransform>();
        if (transform != null && transform.parent != null)
        {
            parentRt = transform.parent.GetComponent<RectTransform>();
        }
    }

    void Awake()
    {
        UpdateRectTransformReferences();
    }

    IEnumerator Start()
    {
        while (Application.isPlaying)
        {
            image.sprite = Sprite1;
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
            image.sprite = Sprite2;
            yield return new WaitForSeconds(Random.Range(0.5f, 1.0f));
        }
    }

    void Update()
    {
        FollowChar3D();
    }

    public void FollowChar3D()
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRt, char3D.ScreenPoint, cam,
            out var localPoint))
        {
            rt.anchoredPosition = localPoint;
        }

        var localScale = transform.localScale;
        localScale.x = char3D.VelocityOnScreenPoint.x > 0 ? -1 : 1;
        transform.localScale = localScale;
    }
}