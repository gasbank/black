using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class Subcanvas : MonoBehaviour
{
    // 초기 구동 단계에서 Close()를 부르고 시작하는 경우가 있다. (OrderPlateRect)
    // 이 때 BackButtonHandler.Instance.PopAction()를 호출하면
    // 스택이 비어 있어서 오류가 난다. 이걸 처리하기 위한 플래그 변수.
    [SerializeField]
    bool backButtonHandlerPushed;

    [SerializeField]
    CanvasGroup canvasGroup;

    [SerializeField]
    CanvasScaler canvasScaler;

    // 아예 뒤로 가기와 무관한 경우에는 무시해야 한다.
    [SerializeField]
    bool disableBackButtonHandler;

    // 백 버튼 처리는 하되 닫지는 말아야 할 경우 true로 한다.
    [SerializeField]
    bool doNotCloseOnBackButton;

    //해당 호출값이 스택의 1이 아닌 최상부일경우 (아래에 추가 스택이 있을때)
    [SerializeField]
    bool forceBackButtonHandler;

    // 강제로 기다려야 하는 창인 경우 뒤로 가기 처리를 하면 안된다.
    // (ProgressMessage 등)
    [SerializeField]
    bool ignoreBackButtonHandler;

    public bool ForceBackButtonHandler
    {
        get => forceBackButtonHandler;
        set => forceBackButtonHandler = value;
    }

    public bool IsOpen => canvasGroup.alpha > 0;

#if UNITY_EDITOR
    void OnValidate()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasScaler = GetComponent<CanvasScaler>();
    }
#endif

    void SendPopupEventMessage()
    {
        SendMessage(IsOpen ? "OpenPopup" : "ClosePopup");
    }

    [UsedImplicitly]
    public void Toggle()
    {
        if (IsOpen)
        {
            CloseCanvasGroupInternal();
        }
        else
        {
            OpenCanvasGroupInternal();
        }

        SendPopupEventMessage();
    }

    void OpenCanvasGroupInternal()
    {
        canvasGroup.alpha = 1.0f;
        canvasGroup.blocksRaycasts = true;
    }

    void CloseCanvasGroupInternal()
    {
        canvasGroup.alpha = 0.0f;
        canvasGroup.blocksRaycasts = false;
    }

    public void Open()
    {
        if (IsOpen == false)
        {
            OpenCanvasGroupInternal();

            // Canvas가 숨겨진 상태에서 켜질 때 Canvas Scaler가 한번 작동해줘야하는데,
            // 그러지 못하는 것 같다. 글씨 크기가 조절되지 않는다.
            // 강제로 껐다 켜 준다.
            if (canvasScaler != null)
            {
                canvasScaler.enabled = false;
                // ReSharper disable once Unity.InefficientPropertyAccess
                canvasScaler.enabled = true;
            }

            SendPopupEventMessage();
            if (disableBackButtonHandler == false)
            {
                if (ForceBackButtonHandler)
                {
                    //현재 백 스택이 2 이상이며, 호출된 Subcanvas가 기존 뒤로 가기 처리방식으로는 해결이 불가능하지만 뒤로가기를 처리해야하는 경우 호출.
                    // 위험할 수 있는 명령이므로 주의해서 사용할것.
                    // = 아무것도 안함!
                }
                else if (ignoreBackButtonHandler)
                {
                    // 뒤로 가기를 처리할 수 없는 상태 (네트워크 확인 중, 클라우드 저장 중 등)
                    // 인 경우에는 그냥 바로 홈 화면으로 보내자.
                    BackButtonHandler.Instance.PushAction(BackButtonHandler.SuspendByMoveTaskToBackIfAndroid);
                }
                else
                {
                    BackButtonHandler.Instance.PushAction(CloseWithDefaultAction);
                }

                backButtonHandlerPushed = true;
            }
        }
    }

    public void OpenWithClickSound()
    {
        // 실제로 열렸는지와 상관 없이 유저 인터랙션이 있었으니 소리는 낸다.
        Sound.Instance.PlayButtonClick();

        Open();
    }

    public void Close()
    {
        if (IsOpen)
        {
            CloseCanvasGroupInternal();
            SendPopupEventMessage();
            if (backButtonHandlerPushed)
            {
                BackButtonHandler.Instance.PopAction();
                backButtonHandlerPushed = false;
            }
        }

        ForceBackButtonHandler = false;
    }

    public void CloseWithClickSound()
    {
        // 실제로 닫혔는지와 상관 없이 유저 인터랙션이 있었으니 소리는 낸다.
        Sound.Instance.PlayButtonClick();

        Close();
    }

    void CloseWithDefaultAction()
    {
        // ConfirmPopup인 경우 창이 닫히기 전에 처리해야 하는 기본 액션이 있다.
        // 그것을 처리하고 닫혀야 한다.
        SendMessage("ExecuteClosePopupDefaultAction", SendMessageOptions.DontRequireReceiver);
        if (doNotCloseOnBackButton == false) Close();
    }
}