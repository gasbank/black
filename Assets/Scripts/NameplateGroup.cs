using UnityEngine;
using UnityEngine.UI;

public class NameplateGroup : MonoBehaviour
{
    [SerializeField]
    [AutoBindThis]
    CanvasGroup canvasGroup;

    [SerializeField]
    Text artistText;
    
    [SerializeField]
    Text titleText;
    
    [SerializeField]
    Text descText;

    public string ArtistText
    {
        set => artistText.text = value;
    }
    
    public string TitleText
    {
        set => titleText.text = value;
    }
    
    public string DescText
    {
        set => descText.text = value;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    void Awake()
    {
        canvasGroup.alpha = 0;
    }
}