using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class Miniroom : MonoBehaviour
{
    struct LeafTarget
    {
        public string RelativePath;
        public Transform Transform;
        public CanvasGroup CanvasGroup;
        public CanvasGroupAlpha CanvasGroupAlpha;
    }

    readonly List<Transform> startVisibleRoots = new();
    readonly List<Transform> fadeRoots = new();
    readonly List<Transform> secondFloorRoots = new();
    readonly List<LeafTarget> leafTargets = new();
    readonly Dictionary<string, LeafTarget> leafTargetsByPath = new(StringComparer.Ordinal);
    readonly HashSet<string> transitionVisibleLeafPathSet = new(StringComparer.Ordinal)
    {
        "Room (Base)",
        "Room (Top)"
    };

    [SerializeField]
    Transform roomBase;

    [SerializeField]
    Transform roomTop;

    [SerializeField]
    Transform roomSecondFloor;

#if UNITY_EDITOR
    void OnValidate()
    {
        ResolveRoots();
    }
#endif

    void Awake()
    {
        RefreshRootTargets();
        RefreshLeafTargets();

        foreach (var leafTarget in leafTargets)
        {
            if (leafTarget.CanvasGroupAlpha != null)
            {
                leafTarget.CanvasGroupAlpha.SetTargetAlphaZero();
            }
        }
    }

    public List<string> GetLeafPathList()
    {
        RefreshLeafTargets();
        return leafTargets.Select(e => e.RelativePath).ToList();
    }

    public bool IsLeafVisible(string relativePath)
    {
        RefreshLeafTargets();
        return leafTargetsByPath.TryGetValue(relativePath, out var leafTarget) &&
               GetTargetAlpha(leafTarget) > 0.0f;
    }

    public bool TrySetLeafVisibility(string relativePath, float alpha, bool canRaycast = false)
    {
        RefreshLeafTargets();

        if (leafTargetsByPath.TryGetValue(relativePath, out var leafTarget) == false)
        {
            return false;
        }

        SetLeafAlpha(leafTarget, alpha, canRaycast);
        return true;
    }

    public void SetLeafVisibility(IEnumerable<string> relativePathList, float alpha, bool canRaycast = false)
    {
        if (relativePathList == null)
        {
            return;
        }

        RefreshLeafTargets();
        foreach (var relativePath in relativePathList)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                continue;
            }

            if (leafTargetsByPath.TryGetValue(relativePath, out var leafTarget))
            {
                SetLeafAlpha(leafTarget, alpha, canRaycast);
            }
        }
    }

    public void SetAllLeafVisibility(float alpha, bool canRaycast = false)
    {
        RefreshLeafTargets();
        foreach (var leafTarget in leafTargets)
        {
            SetLeafAlpha(leafTarget, alpha, canRaycast);
        }
    }

    public void ResetRootVisibility()
    {
        RefreshRootTargets();

        foreach (Transform rootChild in transform)
        {
            if (rootChild == null)
            {
                continue;
            }

            SetTargetAlpha(rootChild, 1.0f, false);
        }
    }

    public void SetTransitionLeafBlend(float blend)
    {
        RefreshLeafTargets();

        foreach (var leafTarget in leafTargets)
        {
            var alpha = transitionVisibleLeafPathSet.Contains(leafTarget.RelativePath)
                ? blend
                : 0.0f;
            SetLeafAlpha(leafTarget, alpha, false);
        }
    }

    public void SetStartVisibleAlpha(float alpha)
    {
        RefreshRootTargets();
        SetRootsAlpha(startVisibleRoots, alpha, alpha > 0.0f ? 1.0f : 0.0f, false);
    }

    public void SetFadeAlpha(float alpha, bool canRaycast)
    {
        RefreshRootTargets();
        SetRootsAlpha(fadeRoots, alpha, alpha > 0.0f ? 1.0f : 0.0f, canRaycast);
    }

    public void SetSecondFloorAlpha(float alpha)
    {
        RefreshRootTargets();
        SetRootsAlpha(secondFloorRoots, alpha, alpha > 0.0f ? 1.0f : 0.0f, false);
    }

    public void PrepareFadeRootsForCrossfade()
    {
        RefreshRootTargets();

        foreach (var root in fadeRoots)
        {
            if (root == null)
            {
                continue;
            }

            SetTargetAlpha(root, 0.0f, false);

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

    public void SetFadeBlend(float blend, bool canRaycast)
    {
        RefreshRootTargets();

        foreach (var root in fadeRoots)
        {
            if (root == null)
            {
                continue;
            }

            SetTargetAlpha(root, blend, canRaycast);

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

    void ResolveRoots()
    {
        if (roomBase == null)
        {
            roomBase = transform.Find("Room (Base)");
        }

        if (roomTop == null)
        {
            roomTop = transform.Find("Room (Top)");
        }

        if (roomSecondFloor == null)
        {
            roomSecondFloor = transform.Find("Room (Second Floor)");
        }
    }

    void RefreshRootTargets()
    {
        ResolveRoots();

        startVisibleRoots.Clear();
        fadeRoots.Clear();
        secondFloorRoots.Clear();

        foreach (Transform rootChild in transform)
        {
            if (rootChild == null)
            {
                continue;
            }

            if (rootChild == roomBase || rootChild == roomTop)
            {
                startVisibleRoots.Add(rootChild);
                continue;
            }

            if (IsSecondFloorRoot(rootChild))
            {
                secondFloorRoots.Add(rootChild);
                continue;
            }

            fadeRoots.Add(rootChild);
        }
    }

    void RefreshLeafTargets()
    {
        leafTargets.Clear();
        leafTargetsByPath.Clear();

        foreach (var leafTransform in EnumerateLeafTransforms(transform))
        {
            if (leafTransform == null)
            {
                continue;
            }

            var relativePath = GetRelativePath(leafTransform);
            if (string.IsNullOrEmpty(relativePath))
            {
                continue;
            }

            var leafTarget = new LeafTarget
            {
                RelativePath = relativePath,
                Transform = leafTransform,
                CanvasGroupAlpha = leafTransform.GetComponent<CanvasGroupAlpha>()
            };

            leafTarget.CanvasGroup = leafTarget.CanvasGroupAlpha == null
                ? EnsureCanvasGroup(leafTransform)
                : null;

            leafTargets.Add(leafTarget);
            leafTargetsByPath[relativePath] = leafTarget;
        }
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

    string GetRelativePath(Transform target)
    {
        if (target == null)
        {
            return null;
        }

        var pathPartList = new List<string>();
        var current = target;
        while (current != null && current != transform)
        {
            pathPartList.Add(current.name);
            current = current.parent;
        }

        if (current != transform)
        {
            return null;
        }

        pathPartList.Reverse();
        return string.Join("/", pathPartList);
    }

    bool IsSecondFloorRoot(Transform rootChild)
    {
        return rootChild != null &&
               (rootChild == roomSecondFloor ||
                rootChild.name.IndexOf("Second Floor", StringComparison.Ordinal) >= 0);
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

    static void SetLeafAlpha(LeafTarget leafTarget, float alpha, bool canRaycast)
    {
        if (leafTarget.CanvasGroupAlpha != null)
        {
            SetCanvasGroupAlpha(leafTarget.CanvasGroupAlpha, alpha, canRaycast);
            return;
        }

        SetCanvasGroupAlpha(leafTarget.CanvasGroup, alpha, canRaycast);
    }

    void SetTargetAlpha(Transform target, float alpha, bool canRaycast)
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

    void SetRootsAlpha(IEnumerable<Transform> roots, float rootAlpha, float childAlpha, bool canRaycast)
    {
        foreach (var root in roots)
        {
            if (root == null)
            {
                continue;
            }

            SetTargetAlpha(root, rootAlpha, canRaycast);

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

    static float GetTargetAlpha(LeafTarget leafTarget)
    {
        if (leafTarget.CanvasGroupAlpha != null)
        {
            return leafTarget.CanvasGroupAlpha.TargetAlpha;
        }

        return leafTarget.CanvasGroup != null ? leafTarget.CanvasGroup.alpha : 0.0f;
    }
}
