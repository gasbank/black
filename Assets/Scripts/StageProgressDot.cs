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
        // OnValidate()를 통해서도 호출되기 때문에 체크!
        if (Application.isPlaying)
        {
            animator.Play(active ? "Active" : "Inactive");
        }
    }

    public void SetColor(Color color)
    {
        image.color = color;
    }
}