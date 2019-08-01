using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class GridWorld : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    [SerializeField] Image image = null;
    [SerializeField] Texture2D tex = null;
    [SerializeField] PaletteButtonGroup paletteButtonGroup = null;
    [SerializeField] IslandLabelSpawner islandLabelSpawner = null;
    [SerializeField] StageSaveManager stageSaveManager = null;
    HashSet<uint> coloredMinPoints = new HashSet<uint>();
    //[SerializeField] TextAsset pngAsset = null;
    StageData stageData;

    public int texSize => tex.width;
    // inclusive
    Vector2Int minCursorInt => new Vector2Int(1, 1);
    // exclusive
    Vector2Int maxCursorInt => new Vector2Int(texSize - 1, texSize - 1);
    // inclusive
    Vector2 minCursor => new Vector2(minCursorInt.x, minCursorInt.y);
    // exclusive
    Vector2 maxCursor => new Vector2(maxCursorInt.x, maxCursorInt.y);

    string StageName => "teststage";
    [SerializeField] int maxIslandPixelArea = 0;

    public void LoadTexture(Texture2D inputTexture, StageData stageData, int maxIslandPixelArea) {
        tex = Instantiate(inputTexture);
        image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        this.stageData = stageData;
        this.maxIslandPixelArea = maxIslandPixelArea;
    }

    // 팔레트 정보 채워지고 난 뒤에 진행 상황 불러와야
    // 제대로 된 색깔로 채울 수 있다.
    internal void ResumeGame() {
        var stageSaveData = stageSaveManager.Load(StageName);
        coloredMinPoints = stageSaveData.coloredMinPoints;
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

    private void UpdateFillMinPoint(ref Vector2Int fillMinPoint, Vector2Int w) {
        w.y = texSize - w.y - 1;
        if (fillMinPoint.x > w.x || (fillMinPoint.x == w.x && fillMinPoint.y > w.y)) {
            fillMinPoint.x = w.x;
            fillMinPoint.y = w.y;
        }
    }

    void FloodFill(Color32[] bitmap, Vector2Int pt, Color32 targetColor, uint replacementColorUint, bool forceSolutionColor) {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(pt);
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
                    return;
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
                    return;
                }
                if ((e.y > 0) && ColorMatch(GetPixel(bitmap, e.x, e.y - 1), targetColor))
                    q.Enqueue(new Vector2Int(e.x, e.y - 1));
                if ((e.y < texSize - 1) && ColorMatch(GetPixel(bitmap, e.x, e.y + 1), targetColor))
                    q.Enqueue(new Vector2Int(e.x, e.y + 1));
                e.x++;
            }
        }
        SushiDebug.Log($"FloodFill algorithm found {pixelList.Count} pixels to be flooded. Starting from {pt} and found {fillMinPoint} as a min point.");
        if (pixelList.Count > 0) {
            // 이 지점부터 fillMinPoint는 유효한 값을 가진다.

            // 디버그 출력
            if (pixelList.Count <= 128) {
                foreach (var pixel in pixelList) {
                    Debug.Log($"Fill Pixel: {pixel.x}, {texSize - pixel.y - 1}");
                }
            }

            Debug.Log($"Fill Min Point: {fillMinPoint.x}, {fillMinPoint.y}");
            var solutionColorUint = stageData.islandDataByMinPoint[BlackConvert.GetP(fillMinPoint)].rgba;
            Debug.Log($"Solution Color (uint): {solutionColorUint}");
            if (forceSolutionColor || solutionColorUint == replacementColorUint) {
                var solutionColor = BlackConvert.GetColor32(solutionColorUint);
                Debug.Log($"Solution Color RGB: {solutionColor.r},{solutionColor.g},{solutionColor.b}");
                foreach (var pixel in pixelList) {
                    SetPixel(bitmap, pixel.x, pixel.y, solutionColor);
                }

                islandLabelSpawner.DestroyLabelByMinPoint(BlackConvert.GetP(fillMinPoint));

                coloredMinPoints.Add(BlackConvert.GetP(fillMinPoint));
            } else {
                // 틀리면 다시 흰색으로 칠해야 한다.
                foreach (var pixel in pixelList) {
                    SetPixel(bitmap, pixel.x, pixel.y, Color.white);
                }
            }
        }
    }

    private bool SetPixelAndUpdateMinPoint(Color32[] bitmap, ref Vector2Int fillMinPoint, ICollection<Vector2Int> pixelList, Color replacementColor, Vector2Int w) {
        // 일단 여기서 replacementColor로 변경 해 둬야 이 알고리즘이 작동한다.
        // 진짜 색깔로 칠하는 건 나중에 pixelList에 모아둔 값으로 제대로 한다.
        SetPixel(bitmap, w.x, w.y, replacementColor);
        pixelList.Add(w);
        if (pixelList.Count > maxIslandPixelArea) {
            Debug.LogError($"CRITICAL LOGIC ERROR: TOO BIG ISLAND. Allowed pixel area is {maxIslandPixelArea}!!! FloodFill() aborted.");
            return false;
        }
        UpdateFillMinPoint(ref fillMinPoint, w);
        return true;
    }

    public void OnPointerDown(PointerEventData eventData) {
    }

    void Fill(Vector2 p) {
        if (paletteButtonGroup.CurrentPaletteColor != Color.white) {

            var w = transform.GetComponent<RectTransform>().rect.width;
            var h = transform.GetComponent<RectTransform>().rect.height;

            //Debug.Log($"w={w} / h={h}");

            var ix = (int)((p.x + w / 2) / w * image.sprite.texture.width);
            var iy = (int)((p.y + h / 2) / h * image.sprite.texture.height);
            FloodFillVec2IntAndApply(ix, iy, false);
        }
    }

    public void FloodFillVec2IntAndApply(int ix, int iy, bool forceSolutionColor) {
        var bitmap = image.sprite.texture.GetPixels32();
        FloodFill(bitmap, new Vector2Int(ix, iy), Color.white, paletteButtonGroup.CurrentPaletteColorUint, forceSolutionColor);
        image.sprite.texture.SetPixels32(bitmap);
        image.sprite.texture.Apply();
    }

    public void LoadBatchFill(HashSet<uint> coloredMinPoints) {
        Debug.Log($"Starting batch fill of {coloredMinPoints.Count} points");
        if (coloredMinPoints.Count > 0) {
            var bitmap = image.sprite.texture.GetPixels32();
            foreach (var minPoint in coloredMinPoints) {
                var minPointVec2 = BlackConvert.GetPInverse(minPoint);
                Debug.Log($"... minPoint: {minPointVec2}");
                FloodFill(bitmap, minPointVec2, Color.white, 0xff000000/* alpha=1.0 black */, true);
            }
            image.sprite.texture.SetPixels32(bitmap);
            image.sprite.texture.Apply();
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (eventData.dragging == false) {
            //Debug.Log($"World position 1 = {eventData.pointerCurrentRaycast.worldPosition}");
            var loc = transform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition);
            //Debug.Log($"World position 2 = {eventData.pointerPressRaycast.worldPosition}");

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.GetComponent<RectTransform>(), eventData.position, Camera.main, out Vector2 localPoint)) {
                Fill(localPoint);
                //Debug.Log($"Local position = {localPoint}");
            }
        }
    }

    void OnApplicationPause(bool pause) {
        if (pause) {
            WriteStageSaveData();
        }
    }

    private void WriteStageSaveData() {
        stageSaveManager.Save(StageName, coloredMinPoints);
    }

    void OnApplicationQuit() {
        WriteStageSaveData();
    }
}
