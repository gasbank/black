using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.SceneManagement;
using System.IO;
using ConditionalDebug;

public class GridWorld : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    [SerializeField] Image image = null;
    [SerializeField] Texture2D tex = null;
    [SerializeField] PaletteButtonGroup paletteButtonGroup = null;
    [SerializeField] IslandLabelSpawner islandLabelSpawner = null;
    [SerializeField] StageSaveManager stageSaveManager = null;
    [SerializeField] ScInt gold = 0;
    HashSet<uint> coloredMinPoints = new HashSet<uint>();
    public Dictionary<uint, int> coloredIslandCountByColor = new Dictionary<uint, int>();
    StageData stageData;

    public Texture2D Tex => tex;

    public int texSize => tex.width;
    // inclusive
    Vector2Int minCursorInt => new Vector2Int(1, 1);
    // exclusive
    Vector2Int maxCursorInt => new Vector2Int(texSize - 1, texSize - 1);
    // inclusive
    Vector2 minCursor => new Vector2(minCursorInt.x, minCursorInt.y);
    // exclusive
    Vector2 maxCursor => new Vector2(maxCursorInt.x, maxCursorInt.y);

    public string StageName { get; set; } = "teststage";

    [SerializeField] int maxIslandPixelArea = 0;
    [SerializeField] RectTransform rt = null;
    [SerializeField] GameObject animatedCoinPrefab = null;
    [SerializeField] RectTransform coinIconRt = null;

    [SerializeField] int coin = 0;
    [SerializeField] TMPro.TextMeshProUGUI coinText = null;

    public int Coin {
        get => coin;
        set {
            coin = value;
            coinText.text = coin.ToString();
        }
    }

    Canvas rootCanvas;

    void OnValidate() {
        rt = GetComponent<RectTransform>();
    }

    void Awake() {
        rootCanvas = GetComponentInParent<Canvas>();
        Coin = 0;
    }

    public Texture2D LoadTextureWithInstantiate(Texture2D inputTexture, StageData stageData, int maxIslandPixelArea) {
        tex = Instantiate(inputTexture);
        image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        this.stageData = stageData;
        this.maxIslandPixelArea = maxIslandPixelArea;
        return tex;
    }

    public Texture2D LoadTexture(Texture2D inputTexture, StageData stageData, int maxIslandPixelArea) {
        tex = inputTexture;
        //image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        this.stageData = stageData;
        this.maxIslandPixelArea = maxIslandPixelArea;
        return tex;
    }

    public Texture2D LoadSprite(Sprite sprite, StageData stageData, int maxIslandPixelArea) {
        tex = sprite.texture;
        image.sprite = sprite;
        this.stageData = stageData;
        this.maxIslandPixelArea = maxIslandPixelArea;
        return tex;
    }

    // 팔레트 정보 채워지고 난 뒤에 진행 상황 불러와야
    // 제대로 된 색깔로 채울 수 있다.
    internal void ResumeGame() {
        try {
            var stageSaveData = stageSaveManager.Load(StageName);
            LoadBatchFill(StageName, stageSaveData.coloredMinPoints);
        } catch (Exception e) {
            Debug.LogException(e);
            DeleteSaveFileAndReloadScene();
        }
    }

    public void DeleteSaveFileAndReloadScene() {
        DeleteSaveFile();
        SceneManager.LoadScene("Main");
    }

    public void DeleteSaveFile() {
        stageSaveManager.DeleteSaveFile(StageName);
    }

    void SetPixelsByPattern(Vector2Int cursorInt, Vector2Int[] pattern, Color32 color) {
        foreach (var v in pattern) {
            var cv = cursorInt + v;
            if (cv.x >= minCursorInt.x && cv.x < maxCursorInt.x && cv.y >= minCursorInt.y && cv.y < maxCursorInt.y) {
                tex.SetPixels32(cv.x, cv.y, 1, 1, new Color32[] { color });
            }
        }
    }

    private static bool ColorMatch(Color32 a, Color32 b) {
        return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
    }

    Color32 GetPixel(Color32[] bitmap, int x, int y) {
        return bitmap[x + y * texSize];
    }

    void SetPixel(Color32[] bitmap, int x, int y, Color c) {
        bitmap[x + y * texSize] = c;
    }

    private void UpdateFillMinPoint(ref Vector2Int fillMinPoint, Vector2Int bitmapPoint) {
        // *** 주의 ***
        // min point 값은 플레이어가 입력한 좌표에서 Y축이 반전된 좌표계이다.
        var invertedBitmapPoint = BlackConvert.GetInvertedY(bitmapPoint, texSize);
        if (fillMinPoint.x > invertedBitmapPoint.x || (fillMinPoint.x == invertedBitmapPoint.x && fillMinPoint.y > invertedBitmapPoint.y)) {
            fillMinPoint.x = invertedBitmapPoint.x;
            fillMinPoint.y = invertedBitmapPoint.y;
        }
    }

    IEnumerator FloodFillCoro(Color32[] bitmap, Vector2Int bitmapPoint, Color32 targetColor, uint replacementColorUint, bool forceSolutionColor, List<int> result) {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(bitmapPoint);
        var fillMinPoint = new Vector2Int(texSize, texSize);
        ICollection<Vector2Int> pixelList = new List<Vector2Int>();
        var replacementColor = BlackConvert.GetColor(replacementColorUint);
        int iter = 0;
        while (q.Count > 0) {
            var n = q.Dequeue();
            if (!ColorMatch(GetPixel(bitmap, n.x, n.y), targetColor))
                continue;
            Vector2Int w = n, e = new Vector2Int(n.x + 1, n.y);
            while ((w.x >= 0) && ColorMatch(GetPixel(bitmap, w.x, w.y), targetColor)) {
                if (SetPixelAndUpdateMinPoint(bitmap, ref fillMinPoint, pixelList, replacementColor, w) == false) {
                    // ERROR
                    yield break;
                }
                if ((w.y > 0) && ColorMatch(GetPixel(bitmap, w.x, w.y - 1), targetColor))
                    q.Enqueue(new Vector2Int(w.x, w.y - 1));
                if ((w.y < texSize - 1) && ColorMatch(GetPixel(bitmap, w.x, w.y + 1), targetColor))
                    q.Enqueue(new Vector2Int(w.x, w.y + 1));
                w.x--;
            }
            while ((e.x <= texSize - 1) && ColorMatch(GetPixel(bitmap, e.x, e.y), targetColor)) {
                if (SetPixelAndUpdateMinPoint(bitmap, ref fillMinPoint, pixelList, replacementColor, e) == false) {
                    // ERROR
                    yield break;
                }
                if ((e.y > 0) && ColorMatch(GetPixel(bitmap, e.x, e.y - 1), targetColor))
                    q.Enqueue(new Vector2Int(e.x, e.y - 1));
                if ((e.y < texSize - 1) && ColorMatch(GetPixel(bitmap, e.x, e.y + 1), targetColor))
                    q.Enqueue(new Vector2Int(e.x, e.y + 1));
                e.x++;
            }
            
            yield return new WaitForEndOfFrame();
            tex.Apply();

            iter++;
            if (iter % 100 == 0) {
                yield return new WaitForEndOfFrame();
                tex.SetPixels32(bitmap);
                tex.Apply();
            }
        }
        yield return new WaitForEndOfFrame();
        tex.SetPixels32(bitmap);
        tex.Apply();

        ConDebug.Log($"FloodFill algorithm found {pixelList.Count} pixels to be flooded. Starting from {bitmapPoint} and found {fillMinPoint} as a min point.");
        if (pixelList.Count > 0) {
            // 이 지점부터 fillMinPoint는 유효한 값을 가진다.
            var fillMinPointUint = BlackConvert.GetP(fillMinPoint);

            // 디버그 출력
            if (pixelList.Count <= 128) {
                foreach (var pixel in pixelList) {
                    //Debug.Log($"Fill Pixel: {pixel.x}, {texSize - pixel.y - 1}");
                }
            }

            Debug.Log($"Fill Min Point: {fillMinPoint.x}, {fillMinPoint.y}");
            var solutionColorUint = stageData.islandDataByMinPoint[fillMinPointUint].rgba;
            Debug.Log($"Solution Color (uint): {solutionColorUint}");
            if (forceSolutionColor || solutionColorUint == replacementColorUint) {
                var solutionColor = BlackConvert.GetColor32(solutionColorUint);
                Debug.Log($"Solution Color RGB: {solutionColor.r},{solutionColor.g},{solutionColor.b}");
                foreach (var pixel in pixelList) {
                    SetPixel(bitmap, pixel.x, pixel.y, solutionColor);
                }

                UpdatePaletteBySolutionColor(fillMinPointUint, solutionColorUint);
                result.Add(1);
                yield break;
            } else {
                // 틀리면 다시 흰색으로 칠해야 한다.
                foreach (var pixel in pixelList) {
                    SetPixel(bitmap, pixel.x, pixel.y, Color.white);
                }
            }
        }
        yield return new WaitForEndOfFrame();
        tex.SetPixels32(bitmap);
        tex.Apply();
    }

    bool FloodFill(Color32[] bitmap, Vector2Int bitmapPoint, Color32 targetColor, uint replacementColorUint, bool forceSolutionColor) {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(bitmapPoint);
        var fillMinPoint = new Vector2Int(texSize, texSize);
        ICollection<Vector2Int> pixelList = new List<Vector2Int>();
        var replacementColor = BlackConvert.GetColor(replacementColorUint);
        while (q.Count > 0) {
            var n = q.Dequeue();
            if (!ColorMatch(GetPixel(bitmap, n.x, n.y), targetColor))
                continue;
            Vector2Int w = n, e = new Vector2Int(n.x + 1, n.y);
            while ((w.x >= 0) && ColorMatch(GetPixel(bitmap, w.x, w.y), targetColor)) {
                if (SetPixelAndUpdateMinPoint(bitmap, ref fillMinPoint, pixelList, replacementColor, w) == false) {
                    // ERROR
                    return false;
                }
                if ((w.y > 0) && ColorMatch(GetPixel(bitmap, w.x, w.y - 1), targetColor))
                    q.Enqueue(new Vector2Int(w.x, w.y - 1));
                if ((w.y < texSize - 1) && ColorMatch(GetPixel(bitmap, w.x, w.y + 1), targetColor))
                    q.Enqueue(new Vector2Int(w.x, w.y + 1));
                w.x--;
            }
            while ((e.x <= texSize - 1) && ColorMatch(GetPixel(bitmap, e.x, e.y), targetColor)) {
                if (SetPixelAndUpdateMinPoint(bitmap, ref fillMinPoint, pixelList, replacementColor, e) == false) {
                    // ERROR
                    return false;
                }
                if ((e.y > 0) && ColorMatch(GetPixel(bitmap, e.x, e.y - 1), targetColor))
                    q.Enqueue(new Vector2Int(e.x, e.y - 1));
                if ((e.y < texSize - 1) && ColorMatch(GetPixel(bitmap, e.x, e.y + 1), targetColor))
                    q.Enqueue(new Vector2Int(e.x, e.y + 1));
                e.x++;
            }
        }
        ConDebug.Log($"FloodFill algorithm found {pixelList.Count} pixels to be flooded. Starting from {bitmapPoint} and found {fillMinPoint} as a min point.");
        if (pixelList.Count > 0) {
            // 이 지점부터 fillMinPoint는 유효한 값을 가진다.
            var fillMinPointUint = BlackConvert.GetP(fillMinPoint);

            // 디버그 출력
            if (pixelList.Count <= 128) {
                foreach (var pixel in pixelList) {
                    //Debug.Log($"Fill Pixel: {pixel.x}, {texSize - pixel.y - 1}");
                }
            }

            Debug.Log($"Fill Min Point: {fillMinPoint.x}, {fillMinPoint.y}");
            var solutionColorUint = stageData.islandDataByMinPoint[fillMinPointUint].rgba;
            Debug.Log($"Solution Color (uint): {solutionColorUint} (0x{solutionColorUint:X8})");
            if (forceSolutionColor || solutionColorUint == replacementColorUint) {
                var solutionColor = BlackConvert.GetColor32(solutionColorUint);
                Debug.Log($"Solution Color RGB: {solutionColor.r},{solutionColor.g},{solutionColor.b}");
                foreach (var pixel in pixelList) {
                    SetPixel(bitmap, pixel.x, pixel.y, solutionColor);
                }

                UpdatePaletteBySolutionColor(fillMinPointUint, solutionColorUint);
                return true;
            } else {
                // 틀리면 다시 흰색으로 칠해야 한다.
                foreach (var pixel in pixelList) {
                    SetPixel(bitmap, pixel.x, pixel.y, Color.white);
                }
            }
        }
        return false;
    }

    private void UpdatePaletteBySolutionColor(uint fillMinPointUint, uint solutionColorUint) {
        islandLabelSpawner.DestroyLabelByMinPoint(fillMinPointUint);

        coloredMinPoints.Add(fillMinPointUint);

        if (coloredIslandCountByColor.TryGetValue(solutionColorUint, out var coloredIslandCount)) {
            coloredIslandCount++;
        } else {
            coloredIslandCount = 1;
        }
        coloredIslandCountByColor[solutionColorUint] = coloredIslandCount;
        paletteButtonGroup.UpdateColoredCount(solutionColorUint, coloredIslandCount);
    }

    private bool SetPixelAndUpdateMinPoint(Color32[] bitmap, ref Vector2Int fillMinPoint, ICollection<Vector2Int> pixelList, Color replacementColor, Vector2Int bitmapPoint) {
        // 일단 여기서 replacementColor로 변경 해 둬야 이 알고리즘이 작동한다.
        // 진짜 색깔로 칠하는 건 나중에 pixelList에 모아둔 값으로 제대로 한다.
        SetPixel(bitmap, bitmapPoint.x, bitmapPoint.y, replacementColor);
        pixelList.Add(bitmapPoint);
        //tex.SetPixel(bitmapPoint.x, bitmapPoint.y, replacementColor);
        if (pixelList.Count > maxIslandPixelArea) {
            Debug.LogError($"CRITICAL LOGIC ERROR: TOO BIG ISLAND. Allowed pixel area is {maxIslandPixelArea}!!! FloodFill() aborted.");
            return false;
        }
        UpdateFillMinPoint(ref fillMinPoint, bitmapPoint);
        return true;
    }

    public void OnPointerDown(PointerEventData eventData) {
    }

    bool Fill(Vector2 localPoint) {
        if (paletteButtonGroup.CurrentPaletteColor != Color.white) {

            var w = rt.rect.width;
            var h = rt.rect.height;

            //Debug.Log($"w={w} / h={h}");

            var ix = (int)((localPoint.x + w / 2) / w * tex.width);
            var iy = (int)((localPoint.y + h / 2) / h * tex.height);
            return FloodFillVec2IntAndApplyWithCurrentPaletteColor(new Vector2Int(ix, iy));
        }
        return false;
    }

    IEnumerator FillCoro(Vector2 localPoint) {
        if (paletteButtonGroup.CurrentPaletteColor != Color.white) {

            var w = rt.rect.width;
            var h = rt.rect.height;

            //Debug.Log($"w={w} / h={h}");

            var ix = (int)((localPoint.x + w / 2) / w * tex.width);
            var iy = (int)((localPoint.y + h / 2) / h * tex.height);
            yield return FloodFillVec2IntAndApplyWithCurrentPaletteColorCoro(new Vector2Int(ix, iy));
        }
    }

    public IEnumerator FloodFillVec2IntAndApplyWithCurrentPaletteColorCoro(Vector2Int bitmapPoint) {
        var bitmap = tex.GetPixels32();
        List<int> result = new List<int>();
        yield return FloodFillCoro(bitmap, bitmapPoint, Color.white, paletteButtonGroup.CurrentPaletteColorUint, false, result);
        if (result.Count > 0) {
            tex.SetPixels32(bitmap);
            tex.Apply();
        }
    }

    public bool FloodFillVec2IntAndApplyWithCurrentPaletteColor(Vector2Int bitmapPoint) {
        var bitmap = tex.GetPixels32();
        var result = FloodFill(bitmap, bitmapPoint, Color.white, paletteButtonGroup.CurrentPaletteColorUint, false);
        if (result) {
            tex.SetPixels32(bitmap);
            tex.Apply();
        }
        return result;
    }

    public void FloodFillVec2IntAndApplyWithSolution(Vector2Int bitmapPoint) {
        var bitmap = tex.GetPixels32();
        FloodFill(bitmap, bitmapPoint, Color.white, 0xff000000/*alpha=255 black*/, true);
        image.sprite.texture.SetPixels32(bitmap);
        image.sprite.texture.Apply();
    }

    public int[] CountWhiteAndBlackInBitmap() {
        var bitmap = tex.GetPixels32();
        var blackCount = 0;
        var whiteCount = 0;
        var otherCount = 0;
        foreach (var b in bitmap) {
            if (b == Color.white) {
                whiteCount++;
            } else if (b == Color.black) {
                blackCount++;
            } else {
                otherCount++;
            }
        }
        return new[] { blackCount, whiteCount, otherCount };
    }

    public void LoadBatchFill(string stageName, HashSet<uint> coloredMinPoints) {
        Debug.Log($"Starting batch fill of {coloredMinPoints.Count} points");
        // if (coloredMinPoints.Count > 0) {
        //     var bitmap = tex.GetPixels32();
        //     foreach (var minPoint in coloredMinPoints) {
        //         var minPointVec2 = BlackConvert.GetPInverse(minPoint);
        //         Debug.Log($"... minPoint: {minPointVec2}");
        //         FloodFill(bitmap, BlackConvert.GetInvertedY(minPointVec2, texSize), Color.white, 0xff000000/* alpha=1.0 black */, true);
        //     }
        //     tex.SetPixels32(bitmap);
        //     tex.Apply();
        // }
        StageSaveManager.LoadWipPng(stageName, tex);
        if (coloredMinPoints.Count > 0) {
            foreach (var minPoint in coloredMinPoints) {
                UpdatePaletteBySolutionColor(minPoint, stageData.islandDataByMinPoint[minPoint].rgba);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (eventData.dragging == false) {
            //Debug.Log($"World position 1 = {eventData.pointerCurrentRaycast.worldPosition}");
            var loc = transform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition);
            //Debug.Log($"World position 2 = {eventData.pointerPressRaycast.worldPosition}");

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, Camera.main, out Vector2 localPoint)) {
                //StartCoroutine(FillCoro(localPoint));
                if (Fill(localPoint)) {
                    StartAnimateFillCoin(localPoint);
                }
                //Debug.Log($"Local position = {localPoint}");
            }
        }
    }

    private void StartAnimateFillCoin(Vector2 localPoint) {
        var animatedCoin = Instantiate(animatedCoinPrefab, transform).GetComponent<AnimatedCoin>();
        animatedCoin.Rt.anchoredPosition = localPoint;
        animatedCoin.TargetRt = coinIconRt;
        animatedCoin.transform.SetParent(rootCanvas.transform, true);
        animatedCoin.transform.localScale = Vector3.one;
        animatedCoin.GridWorld = this;
        gold++;
    }

    void OnApplicationPause(bool pause) {
        if (pause) {
            WriteStageSaveData();
        }
    }

    public void WriteStageSaveData() {
        stageSaveManager.Save(StageName, coloredMinPoints, this);
    }

    void OnApplicationQuit() {
        WriteStageSaveData();
    }

    public int Gold => gold;
}
