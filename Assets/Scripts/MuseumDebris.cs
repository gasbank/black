using UnityEngine;

[DisallowMultipleComponent]
public class MuseumDebris : MonoBehaviour
{
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    public void OnClick()
    {
        ConfirmPopup.instance.OpenYesNoPopup("잔해를 치울까요? 1골드가 필요합니다.", () =>
            {
                Destroy(gameObject);
                ConfirmPopup.instance.Close();
            },
            ConfirmPopup.instance.Close);
    }
}