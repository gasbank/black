using JetBrains.Annotations;
using UnityEngine;

public class StageDetail : MonoBehaviour
{
    [SerializeField]
    Subcanvas subcanvas;
    
    public void OpenPopup()
    {
        subcanvas.Open();
    }

    [UsedImplicitly]
    void ClosePopup()
    {
    }
}