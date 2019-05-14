using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GridWorld : MonoBehaviour, IPointerDownHandler {
    [SerializeField] Renderer planeRenderer = null;
    [SerializeField] Image image = null;
    [SerializeField] Texture2D tex = null;
    [SerializeField] Vector2 cursor;
    [SerializeField] PaletteButtonGroup paletteButtonGroup = null;
    Vector2Int cursorInt => new Vector2Int((int)cursor.x, (int)cursor.y);
    static readonly Color32 RED = new Color32(255, 0, 0, 255);
    static readonly Color32 BLUE = new Color32(0, 0, 255, 255);
    static readonly Color32 WHITE_TIK = new Color32(255, 255, 255, 0);
    static readonly Color32 WHITE_TOK = new Color32(255, 255, 255, 255);
    static readonly Vector2Int[] crossPattern = {
        new Vector2Int(0, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(+1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(0, +1),
    };


    int texSize => tex.width;
    // inclusive
    Vector2Int minCursorInt => new Vector2Int(1, 1);
    // exclusive
    Vector2Int maxCursorInt => new Vector2Int(texSize - 1, texSize - 1);
    // inclusive
    Vector2 minCursor => new Vector2(minCursorInt.x, minCursorInt.y);
    // exclusive
    Vector2 maxCursor => new Vector2(maxCursorInt.x, maxCursorInt.y);

    void Start() {
        //sprite.texture

        //tex = new Texture2D(texSize, texSize, TextureFormat.RGB24, false);
        //tex.SetPixels32(Enumerable.Repeat(WHITE_TOK, tex.width * tex.height).ToArray());
        //tex.wrapMode = TextureWrapMode.Clamp;
        //tex.alphaIsTransparency = false;
        //planeRenderer.material.mainTexture = tex;
        //tex = Instantiate(planeRenderer.material.mainTexture as Texture2D);

        tex = Instantiate(image.sprite.texture);

        var bitmap = tex.GetPixels32(0);

        FloodFill(bitmap, Vector2Int.zero, Color.white, Color.blue);
        FloodFill(bitmap, new Vector2Int(tex.width-1,tex.height-1), Color.white, Color.yellow);
        FloodFill(bitmap, new Vector2Int(tex.width/2,tex.height/2), Color.white, Color.magenta);

        for (int i = 0; i < 1000; i++) {
            bitmap[i] = Color.red;
        }

        tex.SetPixels32(bitmap);
        tex.Apply();

        //planeRenderer.material.mainTexture = tex;
        image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);



        cursor = Vector2.one * texSize / 2;
    }

    bool tikTok = false;

    void Update() {

        
        
        // var bitmap = tex.GetPixels32(0);

        // var oldWhite = tikTok ? WHITE_TIK : WHITE_TOK;
        // var newWhite = tikTok ? WHITE_TOK : WHITE_TIK;
        // FloodFill(bitmap, Vector2Int.zero, oldWhite, newWhite);

        // for (int i = 0; i < bitmap.Length; i++) {
        //     if (ColorMatch(bitmap[i], oldWhite)) {
        //         bitmap[i] = RED;
        //     }
        // }

        // tikTok = !tikTok;


        // tex.SetPixels32(bitmap);
        // SetPixelsByPattern(cursorInt, crossPattern, BLUE);
        // tex.Apply();

        // cursor = new Vector2(Mathf.Clamp(cursor.x - Input.GetAxis("Horizontal"), minCursor.x, maxCursor.x - 1), Mathf.Clamp(cursor.y - Input.GetAxis("Vertical"), minCursor.y, maxCursor.y - 1));
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
    
    void FloodFill(Color32[] bitmap, Vector2Int pt, Color32 targetColor, Color32 replacementColor) {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(pt);
        while (q.Count > 0) {
            var n = q.Dequeue();
            if (!ColorMatch(GetPixel(bitmap, n.x, n.y), targetColor))
                continue;
            Vector2Int w = n, e = new Vector2Int(n.x + 1, n.y);
            while ((w.x >= 0) && ColorMatch(GetPixel(bitmap, w.x, w.y), targetColor)) {
                SetPixel(bitmap, w.x, w.y, replacementColor);
                if ((w.y > 0) && ColorMatch(GetPixel(bitmap, w.x, w.y - 1), targetColor))
                    q.Enqueue(new Vector2Int(w.x, w.y - 1));
                if ((w.y < texSize - 1) && ColorMatch(GetPixel(bitmap, w.x, w.y + 1), targetColor))
                    q.Enqueue(new Vector2Int(w.x, w.y + 1));
                w.x--;
            }
            while ((e.x <= texSize - 1) && ColorMatch(GetPixel(bitmap, e.x, e.y), targetColor)) {
                SetPixel(bitmap, e.x, e.y, replacementColor);
                if ((e.y > 0) && ColorMatch(GetPixel(bitmap, e.x, e.y - 1), targetColor))
                    q.Enqueue(new Vector2Int(e.x, e.y - 1));
                if ((e.y < texSize - 1) && ColorMatch(GetPixel(bitmap, e.x, e.y + 1), targetColor))
                    q.Enqueue(new Vector2Int(e.x, e.y + 1));
                e.x++;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData) {
        Debug.Log($"World position 1 = {eventData.pointerCurrentRaycast.worldPosition}");
        var loc = transform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition);
        Debug.Log($"World position 2 = {eventData.pointerPressRaycast.worldPosition}");

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.GetComponent<RectTransform>(), eventData.position, Camera.main, out Vector2 localPoint)) {
            Fill(localPoint);
            Debug.Log($"Local position = {localPoint}");
        }
        //eventData.position

        //Physics2D.Raycast(Camera.main.transform.position, )

        //eventData.
        
        
    }

    void Fill(Vector2 p) {

        if (paletteButtonGroup.CurrentPaletteColor != Color.white) {

            var w = transform.GetComponent<RectTransform>().rect.width;
            var h = transform.GetComponent<RectTransform>().rect.height;

            Debug.Log($"w={w} / h={h}");

            var ix = (int)((p.x + w/2) / w * image.sprite.texture.width);
            var iy = (int)((p.y + h/2) / h * image.sprite.texture.height);

            var bitmap = image.sprite.texture.GetPixels32();
            FloodFill(bitmap, new Vector2Int(ix, iy), Color.white, paletteButtonGroup.CurrentPaletteColor);
            image.sprite.texture.SetPixels32(bitmap);
            image.sprite.texture.Apply();
        }
    }
}
