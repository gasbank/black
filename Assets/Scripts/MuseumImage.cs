using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MuseumImage : MonoBehaviour
{
    [SerializeField]
    MuseumDebris[] debrisList;

    [SerializeField]
    Transform museumLevel0;

    [SerializeField]
    Transform museumLevel1;

    [SerializeField]
    Transform museumLevel1RoomBase;

    [SerializeField]
    Transform museumLevel1RoomTop;

    [SerializeField]
    Transform museumLevel1RoomSecondFloor;

    [SerializeField]
    float screenFadeDuration = 0.7f;

    [SerializeField]
    float levelTransitionDuration = 2.0f;

    [SerializeField]
    Graphic transitionBlackoutOverlay;

    [SerializeField]
    Graphic transitionInputBlocker;

    struct CanvasGroupState
    {
        public CanvasGroup CanvasGroup;
        public float Alpha;
        public bool Interactable;
        public bool BlocksRaycasts;
    }

    struct MuseumLevel1LeafTarget
    {
        public string RelativePath;
        public Transform Transform;
        public CanvasGroup CanvasGroup;
        public CanvasGroupAlpha CanvasGroupAlpha;
    }

    Transform museumStageRoot;
    RectTransform rootCanvasRectTransform;
    CanvasGroup museumLevel0CanvasGroup;
    CanvasGroup museumLevel1CanvasGroup;
    CanvasGroupAlpha museumLevel1CanvasGroupAlpha;
    readonly List<Transform> museumLevel1StartVisibleRoots = new();
    readonly List<Transform> museumLevel1FadeRoots = new();
    readonly List<Transform> museumLevel1SecondFloorRoots = new();
    readonly List<MuseumLevel1LeafTarget> museumLevel1LeafTargets = new();
    readonly Dictionary<string, MuseumLevel1LeafTarget> museumLevel1LeafTargetsByPath = new(StringComparer.Ordinal);
    readonly HashSet<string> museumLevel1TransitionVisibleLeafPathSet = new(StringComparer.Ordinal)
    {
        "Room (Base)",
        "Room (Top)"
    };
    readonly List<CanvasGroupState> hiddenUiRootStates = new();
    int transitionBlackoutOriginalSiblingIndex = -1;
    bool initialized;
    bool transitionPlaying;

    public bool IsAnyExclamationMarkShown => debrisList.Where(e => e != null).Any(e => e.IsExclamationMarkShown); 
    public bool CanInteract => transitionPlaying == false;
    public bool IsAllDebrisCleared
    {
        get
        {
            var hasDebris = false;

            foreach (var debris in debrisList)
            {
                if (debris == null)
                {
                    continue;
                }

                hasDebris = true;
                if (debris.IsOpen)
                {
                    return false;
                }
            }

            return hasDebris;
        }
    }

    void OnEnable()
    {
        if (BlackContext.Instance != null)
        {
            BlackContext.Instance.OnGoldChanged += OnGoldChanged;
        }

        foreach (var debris in debrisList)
        {
            if (debris != null)
            {
                debris.Cleared += OnDebrisCleared;
            }
        }
    }

    void OnDisable()
    {
        if (BlackContext.Instance != null)
        {
            BlackContext.Instance.OnGoldChanged -= OnGoldChanged;
        }

        foreach (var debris in debrisList)
        {
            if (debris != null)
            {
                debris.Cleared -= OnDebrisCleared;
            }
        }
    }

    IEnumerator Start()
    {
        ResolveSceneReferences();
        EnsureMuseumLevelCanvasGroups();
        EnsureTransitionInputBlocker();
        InitializeTransitionBlackoutOverlay();
        PrewarmMuseumLevel1();

        yield return new WaitUntil(() =>
            BlackContext.Instance != null &&
            BlackContext.Instance.LoadedAtLeastOnce &&
            ConfirmPopup.Instance != null &&
            Data.Instance != null);

        initialized = true;
        ApplyMuseumLevelStateImmediately();
        UpdateExclamationMark();
        TryStartMuseumLevelTransition();
    }

    void OnGoldChanged()
    {
        UpdateExclamationMark();
    }

    void UpdateExclamationMark()
    {
        foreach (var t in debrisList)
        {
            if (t == null)
            {
                continue;
            }

            t.UpdateExclamationMark();
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
        ResolveSceneReferences();
    }
#endif

    public void SetDebrisState(List<int> clearedDebrisIndexList)
    {
        for (var i = 0; i < debrisList.Length; i++)
        {
            if (debrisList[i] == null)
            {
                continue;
            }

            if (clearedDebrisIndexList.Contains(i))
            {
                debrisList[i].Close();
            }
            else
            {
                debrisList[i].Open();
            }
        }

        if (initialized)
        {
            ApplyMuseumLevelStateImmediately();
            TryStartMuseumLevelTransition();
        }
    }

    public List<int> GetDebrisState()
    {
        var clearedDebrisIndexList = new List<int>();
        for (var i = 0; i < debrisList.Length; i++)
        {
            if (debrisList[i].IsOpen == false)
            {
                clearedDebrisIndexList.Add(i);
            }
        }

        return clearedDebrisIndexList;
    }

    public List<string> GetMuseumLevel1LeafPathList()
    {
        RefreshMuseumLevel1LeafTargets();
        return museumLevel1LeafTargets.Select(e => e.RelativePath).ToList();
    }

    public bool TrySetMuseumLevel1LeafVisibility(string relativePath, float alpha, bool canRaycast = false)
    {
        RefreshMuseumLevel1LeafTargets();

        if (museumLevel1LeafTargetsByPath.TryGetValue(relativePath, out var leafTarget) == false)
        {
            return false;
        }

        SetMuseumLevel1LeafAlpha(leafTarget, alpha, canRaycast);
        return true;
    }

    public void SetMuseumLevel1LeafVisibility(IEnumerable<string> relativePathList, float alpha, bool canRaycast = false)
    {
        if (relativePathList == null)
        {
            return;
        }

        RefreshMuseumLevel1LeafTargets();
        foreach (var relativePath in relativePathList)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                continue;
            }

            if (museumLevel1LeafTargetsByPath.TryGetValue(relativePath, out var leafTarget))
            {
                SetMuseumLevel1LeafAlpha(leafTarget, alpha, canRaycast);
            }
        }
    }

    public void SetAllMuseumLevel1LeafVisibility(float alpha, bool canRaycast = false)
    {
        RefreshMuseumLevel1LeafTargets();
        foreach (var leafTarget in museumLevel1LeafTargets)
        {
            SetMuseumLevel1LeafAlpha(leafTarget, alpha, canRaycast);
        }
    }

    public void ClearAllLevel0DebrisForAdmin()
    {
        for (var i = 0; i < debrisList.Length; i++)
        {
            if (debrisList[i] != null)
            {
                debrisList[i].ClearForAdmin();
            }
        }
    }

    void OnDebrisCleared()
    {
        if (initialized == false)
        {
            return;
        }

        TryStartMuseumLevelTransition();
    }

    void ResolveMuseumLevelRoots()
    {
        if (museumLevel0 == null)
        {
            museumLevel0 = transform.Find("Museum (Level 0)");
        }

        if (museumLevel1 == null)
        {
            museumLevel1 = transform.Find("Museum (Level 1)");
        }

        if (museumLevel1RoomBase == null && museumLevel1 != null)
        {
            museumLevel1RoomBase = museumLevel1.Find("Room (Base)");
        }

        if (museumLevel1RoomTop == null && museumLevel1 != null)
        {
            museumLevel1RoomTop = museumLevel1.Find("Room (Top)");
        }

        if (museumLevel1RoomSecondFloor == null && museumLevel1 != null)
        {
            museumLevel1RoomSecondFloor = museumLevel1.Find("Room (Second Floor)");
        }
    }

    void ResolveSceneReferences()
    {
        ResolveMuseumLevelRoots();

        if (museumStageRoot == null)
        {
            museumStageRoot = transform.parent;
        }

        if (rootCanvasRectTransform == null)
        {
            rootCanvasRectTransform = GetComponentInParent<Canvas>()?.GetComponent<RectTransform>();
        }

        if (transitionBlackoutOverlay == null && rootCanvasRectTransform != null)
        {
            var blackoutTransform = rootCanvasRectTransform.Find("Museum Transition Blackout");
            if (blackoutTransform != null)
            {
                transitionBlackoutOverlay = blackoutTransform.GetComponent<Graphic>();
            }
        }
    }

    void EnsureMuseumLevelCanvasGroups()
    {
        museumLevel0CanvasGroup = EnsureCanvasGroup(museumLevel0);
        museumLevel1CanvasGroup = EnsureCanvasGroup(museumLevel1);
        museumLevel1CanvasGroupAlpha = museumLevel1 != null ? museumLevel1.GetComponent<CanvasGroupAlpha>() : null;
        EnsureCanvasGroup(museumLevel1RoomBase);
        EnsureCanvasGroup(museumLevel1RoomTop);
        EnsureCanvasGroup(museumLevel1RoomSecondFloor);
        RefreshMuseumLevel1FadeTargets();
        RefreshMuseumLevel1LeafTargets();
    }

    static CanvasGroup EnsureCanvasGroup(Transform target)
    {
        if (target == null)
        {
            return null;
        }

        var canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = target.gameObject.AddComponent<CanvasGroup>();
        }

        return canvasGroup;
    }

    void PrewarmMuseumLevel1()
    {
        if (museumLevel1 == null || museumLevel1.gameObject.activeSelf)
        {
            return;
        }

        museumLevel1.gameObject.SetActive(true);
    }

    void InitializeTransitionBlackoutOverlay()
    {
        if (transitionBlackoutOverlay == null)
        {
            return;
        }

        var blackoutColor = transitionBlackoutOverlay.color;
        blackoutColor.r = 0.0f;
        blackoutColor.g = 0.0f;
        blackoutColor.b = 0.0f;
        blackoutColor.a = 0.0f;
        transitionBlackoutOverlay.color = blackoutColor;
        transitionBlackoutOverlay.raycastTarget = false;
        transitionBlackoutOverlay.gameObject.SetActive(true);
        transitionBlackoutOriginalSiblingIndex = transitionBlackoutOverlay.rectTransform.GetSiblingIndex();
        RestoreTransitionBlackoutSibling();
    }

    void RefreshMuseumLevel1FadeTargets()
    {
        museumLevel1StartVisibleRoots.Clear();
        museumLevel1FadeRoots.Clear();
        museumLevel1SecondFloorRoots.Clear();

        if (museumLevel1 == null)
        {
            return;
        }

        foreach (Transform rootChild in museumLevel1)
        {
            if (rootChild == null)
            {
                continue;
            }

            EnsureCanvasGroup(rootChild);
            if (rootChild == museumLevel1RoomBase || rootChild == museumLevel1RoomTop)
            {
                museumLevel1StartVisibleRoots.Add(rootChild);
                continue;
            }

            if (IsMuseumLevel1SecondFloorRoot(rootChild))
            {
                museumLevel1SecondFloorRoots.Add(rootChild);
                continue;
            }

            museumLevel1FadeRoots.Add(rootChild);
        }
    }

    void RefreshMuseumLevel1LeafTargets()
    {
        museumLevel1LeafTargets.Clear();
        museumLevel1LeafTargetsByPath.Clear();

        if (museumLevel1 == null)
        {
            return;
        }

        foreach (var leafTransform in EnumerateLeafTransforms(museumLevel1))
        {
            if (leafTransform == null)
            {
                continue;
            }

            var relativePath = GetMuseumLevel1RelativePath(leafTransform);
            if (string.IsNullOrEmpty(relativePath))
            {
                continue;
            }

            var leafTarget = new MuseumLevel1LeafTarget
            {
                RelativePath = relativePath,
                Transform = leafTransform,
                CanvasGroupAlpha = leafTransform.GetComponent<CanvasGroupAlpha>()
            };

            leafTarget.CanvasGroup = leafTarget.CanvasGroupAlpha == null
                ? EnsureCanvasGroup(leafTransform)
                : null;

            museumLevel1LeafTargets.Add(leafTarget);
            museumLevel1LeafTargetsByPath[relativePath] = leafTarget;
        }
    }

    static IEnumerable<Transform> EnumerateLeafTransforms(Transform root)
    {
        if (root == null)
        {
            yield break;
        }

        foreach (Transform child in root)
        {
            if (child == null)
            {
                continue;
            }

            if (child.childCount == 0)
            {
                yield return child;
                continue;
            }

            foreach (var descendantLeaf in EnumerateLeafTransforms(child))
            {
                yield return descendantLeaf;
            }
        }
    }

    string GetMuseumLevel1RelativePath(Transform target)
    {
        if (museumLevel1 == null || target == null)
        {
            return null;
        }

        var pathPartList = new List<string>();
        var current = target;
        while (current != null && current != museumLevel1)
        {
            pathPartList.Add(current.name);
            current = current.parent;
        }

        if (current != museumLevel1)
        {
            return null;
        }

        pathPartList.Reverse();
        return string.Join("/", pathPartList);
    }

    bool IsMuseumLevel1SecondFloorRoot(Transform rootChild)
    {
        return rootChild != null &&
               (rootChild == museumLevel1RoomSecondFloor ||
                rootChild.name.IndexOf("Second Floor", StringComparison.Ordinal) >= 0);
    }

    void EnsureTransitionInputBlocker()
    {
        if (transitionInputBlocker != null)
        {
            transitionInputBlocker.gameObject.SetActive(false);
            return;
        }

        var blockerTransform = transform.Find("Museum Transition Blocker");
        if (blockerTransform != null)
        {
            transitionInputBlocker = blockerTransform.GetComponent<Graphic>();
            if (transitionInputBlocker != null)
            {
                transitionInputBlocker.gameObject.SetActive(false);
                return;
            }
        }

        var blockerObject = new GameObject("Museum Transition Blocker", typeof(RectTransform), typeof(Image));
        var blockerRectTransform = blockerObject.GetComponent<RectTransform>();
        blockerRectTransform.SetParent(transform, false);
        blockerRectTransform.anchorMin = Vector2.zero;
        blockerRectTransform.anchorMax = Vector2.one;
        blockerRectTransform.offsetMin = Vector2.zero;
        blockerRectTransform.offsetMax = Vector2.zero;
        blockerRectTransform.SetAsLastSibling();

        var blockerImage = blockerObject.GetComponent<Image>();
        blockerImage.color = new Color(0, 0, 0, 0);
        blockerImage.raycastTarget = true;
        blockerObject.SetActive(false);

        transitionInputBlocker = blockerImage;
    }

    void ApplyMuseumLevelStateImmediately()
    {
        if (transitionPlaying)
        {
            return;
        }

        SetMuseumLevelVisibility(BlackContext.Instance != null && BlackContext.Instance.HasPlayedMuseumLevel1Transition);
    }

    void SetMuseumLevelVisibility(bool showLevel1)
    {
        if (museumLevel0 != null)
        {
            museumLevel0.gameObject.SetActive(true);
        }

        if (museumLevel0CanvasGroup != null)
        {
            museumLevel0CanvasGroup.alpha = showLevel1 ? 0.0f : 1.0f;
            museumLevel0CanvasGroup.interactable = showLevel1 == false;
            museumLevel0CanvasGroup.blocksRaycasts = showLevel1 == false;
        }

        if (museumLevel1 != null)
        {
            museumLevel1.gameObject.SetActive(true);
        }

        if (museumLevel1CanvasGroup != null)
        {
            SetMuseumLevel1RootAlpha(1.0f, showLevel1);
        }

        ResetMuseumLevel1RootVisibility();
        SetMuseumLevel1TransitionLeafBlend(showLevel1 ? 1.0f : 0.0f);

        if (showLevel1 && museumLevel0 != null)
        {
            museumLevel0.gameObject.SetActive(false);
        }
    }

    void SetMuseumLevel1StartVisibleAlpha(float alpha)
    {
        SetMuseumLevel1RootsAlpha(museumLevel1StartVisibleRoots, alpha, alpha > 0.0f ? 1.0f : 0.0f, false);
    }

    void SetMuseumLevel1FadeAlpha(float alpha, bool canRaycast)
    {
        SetMuseumLevel1RootsAlpha(museumLevel1FadeRoots, alpha, alpha > 0.0f ? 1.0f : 0.0f, canRaycast);
    }

    void SetMuseumLevel1SecondFloorAlpha(float alpha)
    {
        SetMuseumLevel1RootsAlpha(museumLevel1SecondFloorRoots, alpha, alpha > 0.0f ? 1.0f : 0.0f, false);
    }

    static void SetCanvasGroupAlpha(CanvasGroup canvasGroup, float alpha, bool canRaycast)
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = alpha;
        canvasGroup.interactable = canRaycast && alpha > 0.0f;
        canvasGroup.blocksRaycasts = canRaycast && alpha > 0.0f;
    }

    static void SetCanvasGroupAlpha(CanvasGroupAlpha canvasGroupAlpha, float alpha, bool canRaycast)
    {
        if (canvasGroupAlpha == null)
        {
            return;
        }

        canvasGroupAlpha.DisableRaycasts = canRaycast == false || alpha <= 0.0f;
        canvasGroupAlpha.SetAlphaImmediately(alpha);
    }

    static void SetMuseumLevel1LeafAlpha(MuseumLevel1LeafTarget leafTarget, float alpha, bool canRaycast)
    {
        if (leafTarget.CanvasGroupAlpha != null)
        {
            SetCanvasGroupAlpha(leafTarget.CanvasGroupAlpha, alpha, canRaycast);
            return;
        }

        SetCanvasGroupAlpha(leafTarget.CanvasGroup, alpha, canRaycast);
    }

    void SetMuseumLevel1TargetAlpha(Transform target, float alpha, bool canRaycast)
    {
        if (target == null)
        {
            return;
        }

        var canvasGroupAlpha = target.GetComponent<CanvasGroupAlpha>();
        if (canvasGroupAlpha != null)
        {
            SetCanvasGroupAlpha(canvasGroupAlpha, alpha, canRaycast);
            return;
        }

        SetCanvasGroupAlpha(EnsureCanvasGroup(target), alpha, canRaycast);
    }

    void SetMuseumLevel1RootAlpha(float alpha, bool canRaycast)
    {
        if (museumLevel1CanvasGroupAlpha != null)
        {
            SetCanvasGroupAlpha(museumLevel1CanvasGroupAlpha, alpha, canRaycast);
            return;
        }

        SetCanvasGroupAlpha(museumLevel1CanvasGroup, alpha, canRaycast);
    }

    void ResetMuseumLevel1RootVisibility()
    {
        if (museumLevel1 == null)
        {
            return;
        }

        foreach (Transform rootChild in museumLevel1)
        {
            if (rootChild == null)
            {
                continue;
            }

            SetMuseumLevel1TargetAlpha(rootChild, 1.0f, false);
        }
    }

    void SetMuseumLevel1TransitionLeafBlend(float blend)
    {
        RefreshMuseumLevel1LeafTargets();

        foreach (var leafTarget in museumLevel1LeafTargets)
        {
            var alpha = museumLevel1TransitionVisibleLeafPathSet.Contains(leafTarget.RelativePath)
                ? blend
                : 0.0f;
            SetMuseumLevel1LeafAlpha(leafTarget, alpha, false);
        }
    }

    void SetMuseumLevel1RootsAlpha(IEnumerable<Transform> roots, float rootAlpha, float childAlpha, bool canRaycast)
    {
        foreach (var root in roots)
        {
            if (root == null)
            {
                continue;
            }

            SetMuseumLevel1TargetAlpha(root, rootAlpha, canRaycast);

            foreach (var canvasGroupAlpha in root.GetComponentsInChildren<CanvasGroupAlpha>(true))
            {
                if (canvasGroupAlpha.transform == root)
                {
                    continue;
                }

                SetCanvasGroupAlpha(canvasGroupAlpha, childAlpha, canRaycast && rootAlpha > 0.0f);
            }
        }
    }

    void PrepareMuseumLevel1FadeRootsForCrossfade()
    {
        foreach (var root in museumLevel1FadeRoots)
        {
            if (root == null)
            {
                continue;
            }

            SetMuseumLevel1TargetAlpha(root, 0.0f, false);

            foreach (var canvasGroupAlpha in root.GetComponentsInChildren<CanvasGroupAlpha>(true))
            {
                if (canvasGroupAlpha.transform == root)
                {
                    continue;
                }

                SetCanvasGroupAlpha(canvasGroupAlpha, 1.0f, false);
            }
        }
    }

    void SetMuseumLevel1FadeBlend(float blend, bool canRaycast)
    {
        foreach (var root in museumLevel1FadeRoots)
        {
            if (root == null)
            {
                continue;
            }

            SetMuseumLevel1TargetAlpha(root, blend, canRaycast);

            foreach (var canvasGroupAlpha in root.GetComponentsInChildren<CanvasGroupAlpha>(true))
            {
                if (canvasGroupAlpha.transform == root)
                {
                    continue;
                }

                SetCanvasGroupAlpha(canvasGroupAlpha, 1.0f, canRaycast && blend > 0.0f);
            }
        }
    }

    void SetTransitionInputBlocked(bool blocked)
    {
        if (transitionInputBlocker != null)
        {
            transitionInputBlocker.gameObject.SetActive(blocked);
            transitionInputBlocker.raycastTarget = blocked;
            if (blocked && transitionInputBlocker.rectTransform.parent == transform)
            {
                transitionInputBlocker.rectTransform.SetAsLastSibling();
            }
        }
    }

    void SetTransitionBlackout(float alpha, bool blockRaycasts)
    {
        if (transitionBlackoutOverlay == null)
        {
            return;
        }

        var blackoutColor = transitionBlackoutOverlay.color;
        blackoutColor.a = alpha;
        transitionBlackoutOverlay.color = blackoutColor;
        transitionBlackoutOverlay.raycastTarget = blockRaycasts && alpha > 0.0f;

        if (blockRaycasts)
        {
            transitionBlackoutOverlay.rectTransform.SetAsLastSibling();
            return;
        }

        RestoreTransitionBlackoutSibling();
    }

    void RestoreTransitionBlackoutSibling()
    {
        if (transitionBlackoutOverlay == null || transitionBlackoutOriginalSiblingIndex < 0)
        {
            return;
        }

        var parent = transitionBlackoutOverlay.rectTransform.parent;
        if (parent == null)
        {
            return;
        }

        var siblingIndex = Mathf.Clamp(transitionBlackoutOriginalSiblingIndex, 0, parent.childCount - 1);
        transitionBlackoutOverlay.rectTransform.SetSiblingIndex(siblingIndex);
    }

    IEnumerator FadeTransitionBlackout(float startAlpha, float endAlpha)
    {
        var duration = Mathf.Max(0.01f, screenFadeDuration);
        var elapsed = 0.0f;

        SetTransitionBlackout(startAlpha, true);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            var blend = Mathf.Clamp01(elapsed / duration);
            SetTransitionBlackout(Mathf.Lerp(startAlpha, endAlpha, blend), true);
            yield return null;
        }

        SetTransitionBlackout(endAlpha, true);
    }

    void HideNonMuseumUiRoots()
    {
        hiddenUiRootStates.Clear();

        if (rootCanvasRectTransform == null)
        {
            return;
        }

        var confirmPopupRoot = (ConfirmPopup.Instance as Component)?.transform;

        for (var i = 0; i < rootCanvasRectTransform.childCount; i++)
        {
            var uiRoot = rootCanvasRectTransform.GetChild(i);
            if (uiRoot == museumStageRoot ||
                uiRoot == confirmPopupRoot ||
                uiRoot == transitionBlackoutOverlay?.transform)
            {
                continue;
            }

            var canvasGroup = EnsureCanvasGroup(uiRoot);
            if (canvasGroup == null)
            {
                continue;
            }

            hiddenUiRootStates.Add(new CanvasGroupState
            {
                CanvasGroup = canvasGroup,
                Alpha = canvasGroup.alpha,
                Interactable = canvasGroup.interactable,
                BlocksRaycasts = canvasGroup.blocksRaycasts
            });

            canvasGroup.alpha = 0.0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    void RestoreHiddenUiRoots()
    {
        foreach (var hiddenUiRootState in hiddenUiRootStates)
        {
            if (hiddenUiRootState.CanvasGroup == null)
            {
                continue;
            }

            hiddenUiRootState.CanvasGroup.alpha = hiddenUiRootState.Alpha;
            hiddenUiRootState.CanvasGroup.interactable = hiddenUiRootState.Interactable;
            hiddenUiRootState.CanvasGroup.blocksRaycasts = hiddenUiRootState.BlocksRaycasts;
        }

        hiddenUiRootStates.Clear();
    }

    void PrepareMuseumLevelCrossfadeState()
    {
        museumLevel0.gameObject.SetActive(true);
        museumLevel1.gameObject.SetActive(true);
        museumLevel0CanvasGroup.alpha = 1.0f;
        museumLevel0CanvasGroup.interactable = false;
        museumLevel0CanvasGroup.blocksRaycasts = false;
        SetMuseumLevel1RootAlpha(1.0f, false);
        ResetMuseumLevel1RootVisibility();
        SetMuseumLevel1TransitionLeafBlend(0.0f);
    }

    IEnumerator CrossFadeMuseumLevels()
    {
        var duration = Mathf.Max(0.01f, levelTransitionDuration);
        var elapsed = 0.0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            var blend = Mathf.Clamp01(elapsed / duration);
            museumLevel0CanvasGroup.alpha = 1.0f - blend;
            SetMuseumLevel1TransitionLeafBlend(blend);
            yield return null;
        }

        museumLevel0CanvasGroup.alpha = 0.0f;
        museumLevel0.gameObject.SetActive(false);
        museumLevel1.gameObject.SetActive(true);
        museumLevel0CanvasGroup.interactable = false;
        museumLevel0CanvasGroup.blocksRaycasts = false;
        SetMuseumLevel1RootAlpha(1.0f, true);
        ResetMuseumLevel1RootVisibility();
        SetMuseumLevel1TransitionLeafBlend(1.0f);
    }

    void TryStartMuseumLevelTransition()
    {
        if (initialized == false || transitionPlaying)
        {
            return;
        }

        if (BlackContext.Instance == null)
        {
            return;
        }

        if (BlackContext.Instance.HasPlayedMuseumLevel1Transition)
        {
            SetMuseumLevelVisibility(true);
            return;
        }

        if (IsAllDebrisCleared == false)
        {
            SetMuseumLevelVisibility(false);
            return;
        }

        StartCoroutine(PlayMuseumLevelTransitionCoro());
    }

    IEnumerator PlayMuseumLevelTransitionCoro()
    {
        transitionPlaying = true;
        SetTransitionInputBlocked(true);

        BlackContext.Instance.HasPlayedMuseumLevel1Transition = true;
        SaveLoadManager.Save(BlackContext.Instance, ConfigPopup.Instance, Sound.Instance, Data.Instance, null);

        if (museumLevel0 == null || museumLevel1 == null || museumLevel0CanvasGroup == null || museumLevel1CanvasGroup == null)
        {
            Debug.LogError("Museum level transition targets are not configured.");
            transitionPlaying = false;
            SetTransitionInputBlocked(false);
            SetMuseumLevelVisibility(true);
            yield break;
        }

        SetTransitionBlackout(0.0f, true);
        yield return FadeTransitionBlackout(0.0f, 1.0f);

        HideNonMuseumUiRoots();

        yield return FadeTransitionBlackout(1.0f, 0.0f);

        PrepareMuseumLevelCrossfadeState();
        yield return CrossFadeMuseumLevels();

        SetTransitionBlackout(0.0f, false);

        ConfirmPopup.Instance.Open(@"\이제 본격적으로 미술관을 열 수 있게 됐습니다!".Localized(),
            OnMuseumLevelTransitionPopupClosed);
    }

    void OnMuseumLevelTransitionPopupClosed()
    {
        ConfirmPopup.Instance.Close();
        RestoreHiddenUiRoots();
        SetTransitionBlackout(0.0f, false);
        transitionPlaying = false;
        SetTransitionInputBlocked(false);
    }
}
