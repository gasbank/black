using System;
using System.Collections;
using System.Collections.Generic;
using ConditionalDebug;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GridWorld : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    static bool Verbose => false;
    
    readonly HashSet<uint> coloredMinPoints = new HashSet<uint>();

    [SerializeField]
    GameObject animatedCoinPrefab;

    [SerializeField]
    int coin;

    [SerializeField]
    RectTransform coinIconRt;

    [SerializeField]
    TextMeshProUGUI coinText;

    public Dictionary<uint, int> coloredIslandCountByColor = new Dictionary<uint, int>();

    [SerializeField]
    PlayableDirector finaleDirector;

    [SerializeField]
    public Image flickerImage;

    [SerializeField]
    ScInt gold = 0;

    [SerializeField]
    IslandLabelSpawner islandLabelSpawner;

    [SerializeField]
    int maxIslandPixelArea;

    [SerializeField]
    PaletteButtonGroup paletteButtonGroup;

    Canvas rootCanvas;

    [SerializeField]
    RectTransform rt;

    StageData stageData;

    [SerializeField]
    StageSaveManager stageSaveManager;

    [SerializeField]
    Texture2D tex;

    [SerializeField]
    MainGame mainGame;

    public Texture2D Tex => tex;

    public int TexSize => tex.width;

    public string StageName { get; set; } = "TestStage";

    public int Coin
    {
        get => coin;
        set
        {
            coin = value;
            coinText.text = coin.ToString();
        }
    }

    public int Gold => gold;

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.dragging == false)
        {
            if (Verbose) ConDebug.Log($"World position 1 = {eventData.pointerCurrentRaycast.worldPosition}");
            transform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition);
            if (Verbose) ConDebug.Log($"World position 2 = {eventData.pointerPressRaycast.worldPosition}");

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, Camera.main,
                out var localPoint))
            {
                if (Fill(localPoint))
                {
                    // 특별 코인 획득 연출 - 아직 완성되지 않은 기능이므로 런칭스펙에서는 빼자.
                    //StartAnimateFillCoin(localPoint);

                    Sound.instance.PlayFillOkay();

                    // 이번에 칠한 칸이 마지막 칸인가? (모두 칠했는가?)
                    if (IsLabelByMinPointEmpty)
                    {
                        StartFinale();
                    }
                }
                else
                {
                    Sound.instance.PlayFillError();
                    
                    // 칠할 수 없는 경우에는 그에 대한 알림
                    flickerImage.enabled = true;
                    StartCoroutine(nameof(HideFlicker));
                }

                if (Verbose) ConDebug.Log($"Local position = {localPoint}");
            }
        }
    }

    bool IsLabelByMinPointEmpty => islandLabelSpawner.IsLabelByMinPointEmpty;

    void StartFinale()
    {
        mainGame.DeactivateTime();
        finaleDirector.Play(finaleDirector.playableAsset);
        UpdateLastClearedStageIdAndSave();
    }

    static void UpdateLastClearedStageIdAndSave()
    {
        if (!StageButton.CurrentStageMetadata)
        {
            if (Verbose) ConDebug.Log("Current stage metadata is not set. Last cleared stage ID will not be updated. (Did you start the play mode from Main scene?)");
            return;
        }
    
        var stageName = StageButton.CurrentStageMetadata.name;
        for (var i = 0; i < Data.dataSet.StageSequenceData.Count; i++)
        {
            if (Data.dataSet.StageSequenceData[i].stageName == stageName)
            {
                var old = BlackContext.instance.LastClearedStageId;
                
                BlackContext.instance.LastClearedStageId =
                    Mathf.Max(BlackContext.instance.LastClearedStageId, i + 1);

                // 스테이지 클리어에 진전이 있었다. 보상을 준다.
                if (BlackContext.instance.LastClearedStageId > old)
                {
                    BlackContext.instance.AddPendingGold(1);
                }
            }
        }

        SaveLoadManager.instance.Save(BlackContext.instance, ConfigPopup.instance, Sound.instance, Data.instance);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        rt = GetComponent<RectTransform>();
    }
#endif

    void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        Coin = 0;
        flickerImage.enabled = false;
    }

    public void LoadTexture(Texture2D inputTexture, StageData inStageData, int inMaxIslandPixelArea)
    {
        tex = inputTexture;
        stageData = inStageData;
        maxIslandPixelArea = inMaxIslandPixelArea;
    }

    // 팔레트 정보 채워지고 난 뒤에 진행 상황 불러와야
    // 제대로 된 색깔로 채울 수 있다.
    internal void ResumeGame()
    {
        try
        {
            var stageSaveData = stageSaveManager.Load(StageName);
            LoadBatchFill(StageName, stageSaveData.coloredMinPoints);
            mainGame.SetRemainTime(stageSaveData.remainTime);
            if (IsLabelByMinPointEmpty)
            {
                StartFinale();
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            DeleteSaveFileAndReloadScene();
        }
    }

    public void DeleteSaveFileAndReloadScene()
    {
        DeleteSaveFile();
        SceneManager.LoadScene("Main");
    }

    public void DeleteSaveFile()
    {
        StageSaveManager.DeleteSaveFile(StageName);
    }

    static bool ColorMatch(Color32 a, Color32 b)
    {
        return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
    }

    Color32 GetPixel(Color32[] bitmap, int x, int y)
    {
        return bitmap[x + y * TexSize];
    }

    void SetPixel(Color32[] bitmap, int x, int y, Color c)
    {
        bitmap[x + y * TexSize] = c;
    }

    void UpdateFillMinPoint(ref Vector2Int fillMinPoint, Vector2Int bitmapPoint)
    {
        // *** 주의 ***
        // min point 값은 플레이어가 입력한 좌표에서 Y축이 반전된 좌표계이다.
        var invertedBitmapPoint = BlackConvert.GetInvertedY(bitmapPoint, TexSize);
        if (fillMinPoint.x > invertedBitmapPoint.x ||
            fillMinPoint.x == invertedBitmapPoint.x && fillMinPoint.y > invertedBitmapPoint.y)
        {
            fillMinPoint.x = invertedBitmapPoint.x;
            fillMinPoint.y = invertedBitmapPoint.y;
        }
    }

    bool FloodFill(Color32[] bitmap, Vector2Int bitmapPoint, Color32 targetColor, uint replacementColorUint,
        bool forceSolutionColor)
    {
        var q = new Queue<Vector2Int>();
        q.Enqueue(bitmapPoint);
        var fillMinPoint = new Vector2Int(TexSize, TexSize);
        ICollection<Vector2Int> pixelList = new List<Vector2Int>();
        var replacementColor = BlackConvert.GetColor(replacementColorUint);
        while (q.Count > 0)
        {
            var n = q.Dequeue();
            if (!ColorMatch(GetPixel(bitmap, n.x, n.y), targetColor))
                continue;
            Vector2Int w = n, e = new Vector2Int(n.x + 1, n.y);
            while (w.x >= 0 && ColorMatch(GetPixel(bitmap, w.x, w.y), targetColor))
            {
                if (SetPixelAndUpdateMinPoint(bitmap, ref fillMinPoint, pixelList, replacementColor, w) == false)
                    // ERROR
                    return false;

                if (w.y > 0 && ColorMatch(GetPixel(bitmap, w.x, w.y - 1), targetColor))
                    q.Enqueue(new Vector2Int(w.x, w.y - 1));
                if (w.y < TexSize - 1 && ColorMatch(GetPixel(bitmap, w.x, w.y + 1), targetColor))
                    q.Enqueue(new Vector2Int(w.x, w.y + 1));
                w.x--;
            }

            while (e.x <= TexSize - 1 && ColorMatch(GetPixel(bitmap, e.x, e.y), targetColor))
            {
                if (SetPixelAndUpdateMinPoint(bitmap, ref fillMinPoint, pixelList, replacementColor, e) == false)
                    // ERROR
                    return false;

                if (e.y > 0 && ColorMatch(GetPixel(bitmap, e.x, e.y - 1), targetColor))
                    q.Enqueue(new Vector2Int(e.x, e.y - 1));
                if (e.y < TexSize - 1 && ColorMatch(GetPixel(bitmap, e.x, e.y + 1), targetColor))
                    q.Enqueue(new Vector2Int(e.x, e.y + 1));
                e.x++;
            }
        }

        if (Verbose) ConDebug.Log($"FloodFill algorithm found {pixelList.Count} pixels to be flooded.");
        if (Verbose) ConDebug.Log($"Starting from {bitmapPoint} and found {fillMinPoint} as a min point.");

        if (pixelList.Count > 0)
        {
            // 이 지점부터 fillMinPoint는 유효한 값을 가진다.
            var fillMinPointUint = BlackConvert.GetP(fillMinPoint);

            // 디버그 출력
            if (Verbose)
                if (pixelList.Count <= 128)
                    foreach (var pixel in pixelList)
                        if (Verbose) ConDebug.Log($"Fill Pixel: {pixel.x}, {TexSize - pixel.y - 1}");

            if (Verbose) ConDebug.Log($"Fill Min Point: {fillMinPoint.x}, {fillMinPoint.y}");
            var solutionColorUint = stageData.islandDataByMinPoint[fillMinPointUint].rgba;
            if (Verbose) ConDebug.Log($"Solution Color (uint): {solutionColorUint} (0x{solutionColorUint:X8})");
            if (forceSolutionColor || solutionColorUint == replacementColorUint)
            {
                var solutionColor = BlackConvert.GetColor32(solutionColorUint);
                if (Verbose) ConDebug.Log($"Solution Color RGB: {solutionColor.r},{solutionColor.g},{solutionColor.b}");
                foreach (var pixel in pixelList) SetPixel(bitmap, pixel.x, pixel.y, solutionColor);

                UpdatePaletteBySolutionColor(fillMinPointUint, solutionColorUint);
                return true;
            }

            // 틀리면 다시 흰색으로 칠해야 한다.
            foreach (var pixel in pixelList) SetPixel(bitmap, pixel.x, pixel.y, Color.white);
        }

        return false;
    }

    void UpdatePaletteBySolutionColor(uint fillMinPointUint, uint solutionColorUint)
    {
        islandLabelSpawner.DestroyLabelByMinPoint(fillMinPointUint);

        coloredMinPoints.Add(fillMinPointUint);

        if (coloredIslandCountByColor.TryGetValue(solutionColorUint, out var coloredIslandCount))
            coloredIslandCount++;
        else
            coloredIslandCount = 1;

        coloredIslandCountByColor[solutionColorUint] = coloredIslandCount;
        paletteButtonGroup.UpdateColoredCount(solutionColorUint, coloredIslandCount);
    }

    bool SetPixelAndUpdateMinPoint(Color32[] bitmap, ref Vector2Int fillMinPoint,
        ICollection<Vector2Int> pixelList, Color replacementColor, Vector2Int bitmapPoint)
    {
        // 일단 여기서 replacementColor로 변경 해 둬야 이 알고리즘이 작동한다.
        // 진짜 색깔로 칠하는 건 나중에 pixelList에 모아둔 값으로 제대로 한다.
        SetPixel(bitmap, bitmapPoint.x, bitmapPoint.y, replacementColor);
        
//        if (pixelList.Contains(bitmapPoint))
//        {
//            Debug.LogError("pixelList duplicated item should not be inserted.");
//        }
        
        pixelList.Add(bitmapPoint);
        //tex.SetPixel(bitmapPoint.x, bitmapPoint.y, replacementColor);
        if (pixelList.Count > maxIslandPixelArea)
        {
            Debug.LogError(
                $"CRITICAL LOGIC ERROR: TOO BIG ISLAND. Allowed pixel area is {maxIslandPixelArea}, but this time {pixelList.Count}!!! Bug. FloodFill() aborted.");
            //return false;
        }

        UpdateFillMinPoint(ref fillMinPoint, bitmapPoint);
        return true;
    }

    bool Fill(Vector2 localPoint)
    {
        if (paletteButtonGroup.CurrentPaletteColor != Color.white)
        {
            var rect = rt.rect;
            var w = rect.width;
            var h = rect.height;

            if (Verbose) ConDebug.Log($"w={w} / h={h}");

            var ix = (int) ((localPoint.x + w / 2) / w * tex.width);
            var iy = (int) ((localPoint.y + h / 2) / h * tex.height);
            return FloodFillVec2IntAndApplyWithCurrentPaletteColor(new Vector2Int(ix, iy));
        }

        return false;
    }

    bool FloodFillVec2IntAndApplyWithCurrentPaletteColor(Vector2Int bitmapPoint)
    {
        var bitmap = tex.GetPixels32();
        var result = FloodFill(bitmap, bitmapPoint, Color.white, paletteButtonGroup.CurrentPaletteColorUint, false);
        if (result)
        {
            tex.SetPixels32(bitmap);
            tex.Apply();
        }

        return result;
    }

    public int[] CountWhiteAndBlackInBitmap()
    {
        var bitmap = tex.GetPixels32();
        var blackCount = 0;
        var whiteCount = 0;
        var otherCount = 0;
        foreach (var b in bitmap)
            if (b == Color.white)
                whiteCount++;
            else if (b == Color.black)
                blackCount++;
            else
                otherCount++;

        return new[] {blackCount, whiteCount, otherCount};
    }

    void LoadBatchFill(string stageName, HashSet<uint> inColoredMinPoints)
    {
        if (Verbose)
        {
            ConDebug.Log($"Starting batch fill of {inColoredMinPoints.Count} points");
        }

        StageSaveManager.LoadWipPng(stageName, tex);
        
        if (inColoredMinPoints.Count <= 0) return;
        
        foreach (var minPoint in inColoredMinPoints)
        {
            UpdatePaletteBySolutionColor(minPoint, stageData.islandDataByMinPoint[minPoint].rgba);
        }
    }

    IEnumerator HideFlicker()
    {
        yield return new WaitForSeconds(0.0f);
        flickerImage.enabled = false;
    }

    void StartAnimateFillCoin(Vector2 localPoint)
    {
        var animatedCoin = Instantiate(animatedCoinPrefab, transform).GetComponent<AnimatedCoin>();
        animatedCoin.Rt.anchoredPosition = localPoint;
        animatedCoin.TargetRt = coinIconRt;
        var animatedCoinTransform = animatedCoin.transform;
        animatedCoinTransform.SetParent(rootCanvas.transform, true);
        animatedCoinTransform.localScale = Vector3.one;
        animatedCoin.GridWorld = this;
        gold++;
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            WriteStageSaveData();
        }
    }

    public void WriteStageSaveData()
    {
        stageSaveManager.Save(StageName, coloredMinPoints, this, mainGame.GetRemainTime());
    }

    void OnApplicationQuit()
    {
        WriteStageSaveData();
    }
}