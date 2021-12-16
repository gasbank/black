using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class StageProgressDot : MonoBehaviour
{
    [SerializeField]
    Animator animator;

    [SerializeField]
    Image image;
    
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    public void SetAnimActive(bool active)
    {
        animator.Play(active ? "Active" : "Inactive");
    }

    public void SetColor(Color color)
    {
        image.color = color;
    }
}