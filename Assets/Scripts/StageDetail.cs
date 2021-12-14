using JetBrains.Annotations;
using UnityEngine;

public class StageDetail : MonoBehaviour
{
    [SerializeField]
    Subcanvas subcanvas;

    [SerializeField]
    StageButton stageButton;
    
    public void OpenPopup()
    {
        subcanvas.Open();
        
        //stageButton.SetStageMetadata();
    }

    [UsedImplicitly]
    void ClosePopup()
    {
    }
}