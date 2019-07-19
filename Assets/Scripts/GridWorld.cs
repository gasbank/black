using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GridWorld : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    [SerializeField] Image image = null;
    [SerializeField] Texture2D tex = null;
    [SerializeField] PaletteButtonGroup paletteButtonGroup = null;
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

    public void LoadTexture(Texture2D inputTexture, StageData stageData) {
        tex = Instantiate(inputTexture);
        image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        this.stageData = stageData;
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

    private static uint GetC(Color32 v) {
        return ((uint)v.a << 24) + ((uint)v.b << 16) + ((uint)v.g << 8) + (uint)v.r;
    }

    private uint GetP(Vector2Int k) {
        return ((uint)k.y << 16) + (uint)k.x;
    }

    void FloodFill(Color32[] bitmap, Vector2Int pt, Color32 targetColor, Color32 replacementColor) {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(pt);
        var fillMinPoint = new Vector2Int(texSize, texSize);
        List<Vector2Int> pixelList = new List<Vector2Int>();
        while (q.Count > 0) {
            var n = q.Dequeue();
            if (!ColorMatch(GetPixel(bitmap, n.x, n.y), targetColor))
                continue;
            Vector2Int w = n, e = new Vector2Int(n.x + 1, n.y);
            while ((w.x >= 0) && ColorMatch(GetPixel(bitmap, w.x, w.y), targetColor)) {
                SetPixel(bitmap, w.x, w.y, replacementColor);
                pixelList.Add(w);
                UpdateFillMinPoint(ref fillMinPoint, w);
                if ((w.y > 0) && ColorMatch(GetPixel(bitmap, w.x, w.y - 1), targetColor))
                    q.Enqueue(new Vector2Int(w.x, w.y - 1));
                if ((w.y < texSize - 1) && ColorMatch(GetPixel(bitmap, w.x, w.y + 1), targetColor))
                    q.Enqueue(new Vector2Int(w.x, w.y + 1));
                w.x--;
            }
            while ((e.x <= texSize - 1) && ColorMatch(GetPixel(bitmap, e.x, e.y), targetColor)) {
                SetPixel(bitmap, e.x, e.y, replacementColor);
                pixelList.Add(e);
                UpdateFillMinPoint(ref fillMinPoint, e);
                if ((e.y > 0) && ColorMatch(GetPixel(bitmap, e.x, e.y - 1), targetColor))
                    q.Enqueue(new Vector2Int(e.x, e.y - 1));
                if ((e.y < texSize - 1) && ColorMatch(GetPixel(bitmap, e.x, e.y + 1), targetColor))
                    q.Enqueue(new Vector2Int(e.x, e.y + 1));
                e.x++;
            }
        }
        if (pixelList.Count > 0) {
            if (pixelList.Count <= 256) {
                foreach (var pixel in pixelList) {
                    //Debug.Log($"Fill Pixel: {pixel.x}, {texSize - pixel.y - 1}");    
                }
            }
            Debug.Log($"Fill Min Point: {fillMinPoint.x}, {fillMinPoint.y}");
            var solutionColorUint = stageData.islandDataByMinPoint[GetP(fillMinPoint)].rgba;
            Color32 solutionColor = new Color32(
                (byte)(solutionColorUint & 0xff),
                (byte)((solutionColorUint >> 8) & 0xff),
                (byte)((solutionColorUint >> 16) & 0xff),
                (byte)((solutionColorUint >> 24) & 0xff));
            Debug.Log($"Solution Color RGB: {solutionColor.r},{solutionColor.g},{solutionColor.b}");
            foreach (var pixel in pixelList) {
                SetPixel(bitmap, pixel.x, pixel.y, solutionColor);
            }
        }
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

            var bitmap = image.sprite.texture.GetPixels32();
            FloodFill(bitmap, new Vector2Int(ix, iy), Color.white, paletteButtonGroup.CurrentPaletteColor);
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
}
