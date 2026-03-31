using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ProfilePopup : MonoBehaviour
{
    [SerializeField]
    Transform stageImageParent;

    [SerializeField]
    int cachedLastClearedStageId;

    [FormerlySerializedAs("fullStageImagePrefab")]
    [SerializeField]
    GameObject fullStageImageButtonPrefab;

    [SerializeField]
    StageDetailPopup stageDetailPopupForReplay;

    [SerializeField]
    ScrollRect scrollRect;

    [SerializeField]
    RectTransform viewport;

    [SerializeField]
    GridLayoutGroup gridLayoutGroup;

    [SerializeField]
    int loadConcurrency = 3;

    [SerializeField]
    int visibleBufferRows = 1;

    readonly List<StageCellEntry> stageCellEntryList = new List<StageCellEntry>();
    readonly HashSet<StageCellEntry> activeStageCellEntrySet = new HashSet<StageCellEntry>();
    readonly Vector3[] viewportCorners = new Vector3[4];
    readonly Vector3[] cellCorners = new Vector3[4];

    bool popupOpen;
    bool scrollRectSubscribed;
    int loadSessionVersion;
    
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
        CacheRuntimeReferences();
    }
#endif

    void Awake()
    {
        CacheRuntimeReferences();
        SubscribeToScrollRect();
    }

    void OnDestroy()
    {
        UnsubscribeFromScrollRect();
    }

    void CacheRuntimeReferences()
    {
        if (scrollRect == null)
        {
            scrollRect = GetComponentInChildren<ScrollRect>(true);
        }

        if (scrollRect != null)
        {
            if (viewport == null)
            {
                viewport = scrollRect.viewport != null
                    ? scrollRect.viewport
                    : scrollRect.GetComponent<RectTransform>();
            }

            if (stageImageParent == null)
            {
                stageImageParent = scrollRect.content;
            }
        }

        if (gridLayoutGroup == null && stageImageParent != null)
        {
            gridLayoutGroup = stageImageParent.GetComponent<GridLayoutGroup>();
        }
    }

    void SubscribeToScrollRect()
    {
        if (scrollRectSubscribed || scrollRect == null)
        {
            return;
        }

        scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        scrollRectSubscribed = true;
    }

    void UnsubscribeFromScrollRect()
    {
        if (scrollRectSubscribed == false || scrollRect == null)
        {
            return;
        }

        scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
        scrollRectSubscribed = false;
    }

    [UsedImplicitly]
    async void OpenPopup()
    {
        popupOpen = true;

        CacheRuntimeReferences();
        SubscribeToScrollRect();

        var lastClearedStageId = BlackContext.Instance.LastClearedStageId;
        if (cachedLastClearedStageId == lastClearedStageId && stageCellEntryList.Count == lastClearedStageId)
        {
            RefreshLoadPriorities();
            return;
        }

        loadSessionVersion++;
        activeStageCellEntrySet.Clear();
        stageCellEntryList.Clear();
        stageImageParent.DestroyImmediateAllChildren();

        for (var i = 0; i < lastClearedStageId; i++)
        {
            var fullStageImageButton = Instantiate(fullStageImageButtonPrefab, stageImageParent)
                .GetComponent<FullStageImageButton>();
            fullStageImageButton.InitializeShell(i, stageDetailPopupForReplay);
            stageCellEntryList.Add(new StageCellEntry(i, fullStageImageButton));
        }

        cachedLastClearedStageId = lastClearedStageId;

        ForceRefreshLayout();
        await Task.Yield();
        ForceRefreshLayout();
        RefreshLoadPriorities();
    }

    [UsedImplicitly]
    void ClosePopup()
    {
        popupOpen = false;
    }

    void ForceRefreshLayout()
    {
        if (stageImageParent == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();

        if (stageImageParent is RectTransform stageImageParentRectTransform)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(stageImageParentRectTransform);
        }

        Canvas.ForceUpdateCanvases();
    }

    void OnScrollValueChanged(Vector2 _)
    {
        if (popupOpen == false)
        {
            return;
        }

        RefreshLoadPriorities();
    }

    void RefreshLoadPriorities()
    {
        if (stageCellEntryList.Count == 0)
        {
            return;
        }

        var visibleMinRow = int.MaxValue;
        var visibleMaxRow = int.MinValue;

        foreach (var entry in stageCellEntryList)
        {
            entry.Visible = IsCellVisible(entry.RectTransform);
            if (entry.Visible == false)
            {
                continue;
            }

            var row = GetRow(entry.StageIndex);
            visibleMinRow = Mathf.Min(visibleMinRow, row);
            visibleMaxRow = Mathf.Max(visibleMaxRow, row);
        }

        if (visibleMinRow == int.MaxValue)
        {
            visibleMinRow = 0;
            visibleMaxRow = Mathf.Min(GetMaxRowIndex(), visibleBufferRows);
        }

        var bufferedMinRow = Mathf.Max(0, visibleMinRow - Mathf.Max(0, visibleBufferRows));
        var bufferedMaxRow = Mathf.Min(GetMaxRowIndex(), visibleMaxRow + Mathf.Max(0, visibleBufferRows));

        foreach (var entry in stageCellEntryList)
        {
            var row = GetRow(entry.StageIndex);
            entry.PriorityBand = entry.Visible ? 0 : row >= bufferedMinRow && row <= bufferedMaxRow ? 1 : 2;
            entry.RowDistance = GetRowDistance(row, bufferedMinRow, bufferedMaxRow);
        }

        TryStartMoreLoads();
    }

    void TryStartMoreLoads()
    {
        if (popupOpen == false)
        {
            return;
        }

        var maxConcurrentLoads = Mathf.Max(1, loadConcurrency);
        while (activeStageCellEntrySet.Count < maxConcurrentLoads)
        {
            var nextEntry = GetNextEntryToLoad();
            if (nextEntry == null)
            {
                break;
            }

            StartLoadingEntry(nextEntry, loadSessionVersion);
        }
    }

    StageCellEntry GetNextEntryToLoad()
    {
        StageCellEntry bestEntry = null;

        foreach (var entry in stageCellEntryList)
        {
            if (entry.LoadState != StageCellLoadState.Shell || activeStageCellEntrySet.Contains(entry))
            {
                continue;
            }

            if (bestEntry == null || IsHigherPriority(entry, bestEntry))
            {
                bestEntry = entry;
            }
        }

        return bestEntry;
    }

    bool IsHigherPriority(StageCellEntry candidate, StageCellEntry currentBest)
    {
        if (candidate.PriorityBand != currentBest.PriorityBand)
        {
            return candidate.PriorityBand < currentBest.PriorityBand;
        }

        if (candidate.RowDistance != currentBest.RowDistance)
        {
            return candidate.RowDistance < currentBest.RowDistance;
        }

        return candidate.StageIndex < currentBest.StageIndex;
    }

    async void StartLoadingEntry(StageCellEntry entry, int sessionVersion)
    {
        entry.LoadState = StageCellLoadState.Loading;
        activeStageCellEntrySet.Add(entry);

        try
        {
            var stageMetadata = await StageDetailPopup.LoadStageMetadataByZeroBasedIndexAsync(entry.StageIndex);
            if (sessionVersion != loadSessionVersion || entry.Button == null)
            {
                return;
            }

            if (stageMetadata == null)
            {
                entry.LoadState = StageCellLoadState.Failed;
                return;
            }

            entry.Button.BindStageMetadata(stageMetadata);
            entry.LoadState = StageCellLoadState.Loaded;
        }
        finally
        {
            activeStageCellEntrySet.Remove(entry);
            if (sessionVersion == loadSessionVersion && popupOpen)
            {
                TryStartMoreLoads();
            }
        }
    }

    bool IsCellVisible(RectTransform cellRectTransform)
    {
        if (cellRectTransform == null || viewport == null)
        {
            return true;
        }

        viewport.GetWorldCorners(viewportCorners);
        cellRectTransform.GetWorldCorners(cellCorners);

        return cellCorners[2].x > viewportCorners[0].x &&
               cellCorners[0].x < viewportCorners[2].x &&
               cellCorners[2].y > viewportCorners[0].y &&
               cellCorners[0].y < viewportCorners[2].y;
    }

    int GetRow(int stageIndex)
    {
        return stageIndex / GetColumnCount();
    }

    int GetColumnCount()
    {
        if (gridLayoutGroup != null &&
            gridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedColumnCount &&
            gridLayoutGroup.constraintCount > 0)
        {
            return gridLayoutGroup.constraintCount;
        }

        return 1;
    }

    int GetMaxRowIndex()
    {
        if (stageCellEntryList.Count == 0)
        {
            return 0;
        }

        return GetRow(stageCellEntryList.Count - 1);
    }

    static int GetRowDistance(int row, int minRow, int maxRow)
    {
        if (row < minRow)
        {
            return minRow - row;
        }

        if (row > maxRow)
        {
            return row - maxRow;
        }

        return 0;
    }

    enum StageCellLoadState
    {
        Shell,
        Loading,
        Loaded,
        Failed
    }

    sealed class StageCellEntry
    {
        public StageCellEntry(int stageIndex, FullStageImageButton button)
        {
            StageIndex = stageIndex;
            Button = button;
            RectTransform = button.GetComponent<RectTransform>();
        }

        public int StageIndex { get; }
        public FullStageImageButton Button { get; }
        public RectTransform RectTransform { get; }
        public bool Visible { get; set; }
        public int PriorityBand { get; set; }
        public int RowDistance { get; set; }
        public StageCellLoadState LoadState { get; set; } = StageCellLoadState.Shell;
    }
}
