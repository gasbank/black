using UnityEngine;

public class EaselButton : MonoBehaviour
{
    [SerializeField]
    StageDetail stageDetail;
    
    public void OnClick()
    {
        stageDetail.OpenPopup();
    }
}