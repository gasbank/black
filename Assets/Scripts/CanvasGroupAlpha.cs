using System;
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
#endif

[RequireComponent(typeof(CanvasGroup))]
[DisallowMultipleComponent]
public class CanvasGroupAlpha : MonoBehaviour
{
    [SerializeField]
    bool alwaysInteractable;

    float blendRatio = 1.0f;

    [SerializeField]
    [AutoBindThis]
    CanvasGroup canvasGroup;

    [SerializeField]
    bool disableRaycasts;

    [SerializeField]
    Canvas[] immediateCanvasList;

    [SerializeField]
    float targetAlpha = 1.0f;

    [SerializeField]
    bool useUnscaledTime;

    public float TargetAlpha
    {
        get => targetAlpha;
        private set => SetTargetAlpha(value);
    }

    public bool TargetReached => Mathf.Approximately(TargetAlpha, canvasGroup.alpha);

    public Action OnTargetReached { get; internal set; }

    [field: SerializeField]
    public float AlphaSpeed { get; set; }

    public bool DisableRaycasts
    {
        get => disableRaycasts;
        set
        {
            disableRaycasts = value;
            canvasGroup.blocksRaycasts = !disableRaycasts;
        }
    }

    public IEnumerator WaitTargetReachCoro()
    {
        return new WaitWhile(() => TargetReached == false);
    }

    [ContextMenu(nameof(SetTargetAlphaOne))]
    public void SetTargetAlphaOne()
    {
        TargetAlpha = 1;
        if (Application.isEditor && Application.isPlaying == false || AlphaSpeed == 0) canvasGroup.alpha = 1;
#if UNITY_EDITOR
        UpdateChildrenSceneVisibilityEditor();
#endif
    }

    [ContextMenu(nameof(SetTargetAlphaZero))]
    public void SetTargetAlphaZero()
    {
        TargetAlpha = 0;
        if (Application.isEditor && Application.isPlaying == false || AlphaSpeed == 0) canvasGroup.alpha = 0;
#if UNITY_EDITOR
        UpdateChildrenSceneVisibilityEditor();
#endif
    }

    public void SetAlphaImmediately(float v)
    {
        TargetAlpha = v;
        canvasGroup.alpha = v;
    }

    void SetTargetAlpha(float value)
    {
        if (canvasGroup != null)
        {
            if (value > 0)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = !disableRaycasts;
            }
            else
            {
                canvasGroup.interactable = alwaysInteractable;
                canvasGroup.blocksRaycasts = false;
            }
        }

        targetAlpha = value;
        blendRatio = 0;
        enabled = true;

        // 소트 문제때문에 Canvas 붙인 것들이 있다. 이들도 여기에서 처리
        if (immediateCanvasList != null)
            foreach (var immediateCanvas in immediateCanvasList)
                if (immediateCanvas != null)
                    immediateCanvas.enabled = targetAlpha > 0;

#if UNITY_EDITOR
        UpdateSceneVisibilityEditor();
#endif
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
        UpdateChildrenSceneVisibilityEditor();
        immediateCanvasList = transform.GetChildren<Canvas>();
    }
#endif

    void Update()
    {
        if (Mathf.Approximately(canvasGroup.alpha, targetAlpha) == false)
        {
            canvasGroup.alpha = Mathf.SmoothStep(targetAlpha <= 0 ? 1 : 0, targetAlpha, blendRatio);
            //canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, blendRatio);
            var deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            blendRatio = Mathf.Clamp01(blendRatio + AlphaSpeed * deltaTime);
        }
        else
        {
            canvasGroup.alpha = targetAlpha;

            if (OnTargetReached != null)
            {
                OnTargetReached();
                OnTargetReached = null;
            }

            enabled = false;
        }
    }

    public void StartFade(float duration, float newTargetAlpha)
    {
        AlphaSpeed = duration > 0 ? 1.0f / duration : 0;
        if (duration > 0)
            TargetAlpha = newTargetAlpha;
        else
            SetAlphaImmediately(newTargetAlpha);
    }

#if UNITY_EDITOR
    void UpdateSceneVisibilityEditor()
    {
//        if (gameObject.IsBlocksRaycasts())
//        {
//            SceneVisibilityManager.instance.EnablePicking(gameObject, true);
//        }
//        else
//        {
//            SceneVisibilityManager.instance.DisablePicking(gameObject, true);
//        }
//
//        EditorUtility.SetDirty(SceneVisibilityManager.instance);
    }

    void UpdateChildrenSceneVisibilityEditor()
    {
        foreach (var canvasGroupAlpha in GetComponentsInChildren<CanvasGroupAlpha>())
            canvasGroupAlpha.UpdateSceneVisibilityEditor();
    }
#endif
}