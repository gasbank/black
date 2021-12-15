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

        var lastClearedStageId = BlackContext.instance.LastClearedStageId;
        Debug.Log($"Last Cleared Stage ID: {lastClearedStageId}");
        //stageButton.SetStageMetadata();
    }

    [UsedImplicitly]
    void ClosePopup()
    {
    }
}