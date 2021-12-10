using ConditionalDebug;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ProgressMessage : MonoBehaviour, IPlatformProgressMessage {
    public static IPlatformProgressMessage instance;
    public Text messageText;

    [SerializeField]
    GameObject closeButton;

    [SerializeField]
    Subcanvas subcanvas;

    [SerializeField]
    PlatformFacebookAdsInit platformFacebookAdsInit;

    public bool IsOpen => subcanvas.IsOpen;

    public string MessageText {
        get => messageText.text;
        set => messageText.text = value;
    }

    void Awake() {
        if (transform.parent.childCount - 1 != transform.GetSiblingIndex()) {
            Debug.LogError("Progress Message should be last sibling!");
        }

        closeButton.SetActive(false);
    }

#if UNITY_EDITOR
    void OnValidate() {
        subcanvas = GetComponent<Subcanvas>();
    }
#endif

    void OpenPopup() {
        // should receive message even if there is nothing to do
    }

    void ClosePopup() {
        // should receive message even if there is nothing to do
    }

    public void Open(string msg) {
        messageText.text = msg;
        //gameObject.SetActive(true);

        subcanvas.Open();
    }

    public void Close() {
        //gameObject.SetActive(false);

        subcanvas.Close();
    }

    //타 팝업에서도 필요시 호출해서 사용.
    public void PushCloseButton() {
        ConDebug.Log("ProgressMessage Force Close Push by user.");
        platformFacebookAdsInit.IsAdCurrentlyCalled = false;
        Close();
    }

    //네트워크 옵션 등 뒤로가기를 처리할수 없는 상태를 무시하기 위한 메소드.
    //주의깊게 사용할것.
    public void ForceBackButtonActive() {
        subcanvas.ForceBackButtonHandler = true;
    }

    public void CloseButtonPopup() {
        ConDebug.Log("ProgressMessage Close button popup");
        BackButtonHandler.instance.PushAction(instance.PushCloseButton);
        closeButton.SetActive(true);
    }

    public void DisableCloseButton() {
        ConDebug.Log("ProgressMessage Close button Disabled");
        closeButton.SetActive(false);
    }
}