using System;
using System.Collections.Generic;
using ConditionalDebug;
using UnityEngine;

[DisallowMultipleComponent]
public class BackButtonHandler : MonoBehaviour
{
    static bool Verbose => false;
    
    public static BackButtonHandler instance;

    readonly Stack<Action> stack = new Stack<Action>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CallBackButtonAction();
        }
    }

    void CallBackButtonAction()
    {
        if (stack.Count == 0)
        {
            SuspendByQuitIfAndroid();
        }
        else
        {
            ExecuteTopAction();
        }
    }

    // 스택의 모든 작업이 텅 빌때까지 다 실행한다.
    // 다만, 앱이 백그라운드로 가는 수준은 처리하지 않는다.
    // (스택 구조이므로 해당 작업이 처리되지 않으면 그 아래에 있는 작업은 자연스럽게 처리되지 않음)
    public void FlushCallBackButtonActionExceptSuspendAction()
    {
        // 스택에 쌓인 뒤로 가기 작업 처리 중 stack 상황이 바뀔 것이다.
        // 따로 빼 놓는다.
        var count = stack.Count;
        // 스택에 쌓인 뒤로 가기 작업 처리 중 stack이 조기 소진될 수도 있으므로 추가 조건 넣는다.
        for (var i = 0; i < count && stack.Count > 0; i++)
        {
            ExecuteTopActionExceptSuspendAction();
        }
    }

    static void SuspendByQuitIfAndroid()
    {
#if UNITY_ANDROID
        ConfirmPopup.instance.OpenYesNoPopup("\\컬러뮤지엄를 종료하시겠습니까?".Localized(), () =>
        {
            // 실제로 앱 종료한다.
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }, ConfirmPopup.instance.Close);
#endif
    }

    // 백그라운드로만 보낸다. (실제로 종료하지 않음)
    public static void SuspendByMoveTaskToBackIfAndroid()
    {
#if UNITY_ANDROID
        if (Application.isEditor)
        {
            ConDebug.Log("Android home screen will be shown.");
        }
        else
        {
            var activity =
                new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            if (activity != null) activity.Call<bool>("moveTaskToBack", true);
        }
#endif
    }

    public void PushAction(Action action)
    {
        stack.Push(action);
        if (Verbose)
        {
            ConDebug.Log($"BackButtonHandler pushed: {stack.Count} actions stacked.");
        }
    }

    void ExecuteTopAction()
    {
        stack.Peek()();
        if (Verbose)
        {
            ConDebug.Log("BackButtonHandler execute top action.");
        }
    }

    // 안드로이드의 경우 홈 화면으로 가는 것 또는 게임이 종료되는 것까지 스택 작업에 들어가 있을 수 있다.
    // 이 작업은 처리하지 않는다.
    void ExecuteTopActionExceptSuspendAction()
    {
        if (stack.Peek() != SuspendByMoveTaskToBackIfAndroid && stack.Peek() != SuspendByQuitIfAndroid)
        {
            stack.Peek()();
        }
    }

    public void PopAction()
    {
        stack.Pop();
        if (Verbose)
        {
            ConDebug.Log($"BackButtonHandler popped: {stack.Count} actions stacked.");
        }
    }
}