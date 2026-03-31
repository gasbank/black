using UnityEngine;

[DisallowMultipleComponent]
public class MuseumDecoButton : MonoBehaviour
{
    const string LockedMessage = "미술관 개관 이후 쓸 수 있는 기능입니다.";

    [SerializeField]
    GameObject lockOverlay;

    [SerializeField]
    Subcanvas shopPopup;

    bool? lastLevel1State;

    void OnEnable()
    {
        Refresh();
    }

    void Start()
    {
        Refresh();
    }

    void Update()
    {
        var isLevel1 = MuseumLevelGroup.IsMuseumLevel1Unlocked();
        if (lastLevel1State != isLevel1)
        {
            Refresh(isLevel1);
        }
    }

    public void OnClick()
    {
        if (MuseumLevelGroup.IsMuseumLevel1Unlocked())
        {
            if (shopPopup == null)
            {
                Debug.LogError("MuseumDecoButton shopPopup is not assigned.", gameObject);
                return;
            }

            shopPopup.OpenWithClickSound();
            return;
        }

        Sound.Instance.PlayButtonClick();
        ConfirmPopup.Instance.OpenSimpleMessage(LockedMessage);
    }

    void Refresh()
    {
        Refresh(MuseumLevelGroup.IsMuseumLevel1Unlocked());
    }

    void Refresh(bool isLevel1)
    {
        lastLevel1State = isLevel1;

        if (lockOverlay != null)
        {
            lockOverlay.SetActive(isLevel1 == false);
        }
    }
}
