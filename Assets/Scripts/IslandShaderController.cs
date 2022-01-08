using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IslandShaderController : MonoBehaviour
{
    [SerializeField]
    RawImage rawImage;

    [SerializeField]
    [Range(0, 4)]
    int islandIndex;

    static readonly int A1Tex = Shader.PropertyToID("_A1Tex");
    static readonly int A2Tex = Shader.PropertyToID("_A2Tex");
    static readonly int Palette = Shader.PropertyToID("_Palette");
    static readonly int IslandIndex = Shader.PropertyToID("_IslandIndex");

    // Start is called before the first frame update
    void Start()
    {
        var a1Tex = new Texture2D(2, 2, TextureFormat.Alpha8, false, false);
        a1Tex.SetPixel(0, 0, new Color32(0, 0, 0, 0 | 0));
        a1Tex.SetPixel(1, 0, new Color32(0, 0, 0, 1 | (1 << 6)));
        a1Tex.SetPixel(0, 1, new Color32(0, 0, 0, 2 | (2 << 6)));
        a1Tex.SetPixel(1, 1, new Color32(0, 0, 0, 3 | (3 << 6)));
        a1Tex.filterMode = FilterMode.Point;
        a1Tex.wrapMode = TextureWrapMode.Clamp;
        a1Tex.Apply();
        
        var a2Tex = new Texture2D(2, 2, TextureFormat.Alpha8, false, false);
        a2Tex.SetPixel(0, 0, new Color32(0,0,0,0));
        a2Tex.SetPixel(1, 0, new Color32(0,0,0,0));
        a2Tex.SetPixel(0, 1, new Color32(0,0,0,0));
        a2Tex.SetPixel(1, 1, new Color32(0,0,0,0));
        a2Tex.filterMode = FilterMode.Point;
        a2Tex.wrapMode = TextureWrapMode.Clamp;
        a2Tex.Apply();
        
        rawImage.material.SetColorArray(Palette, new List<Color>
        {
            Color.red, Color.green, Color.blue, Color.white
        });
        rawImage.material.SetTexture(A1Tex, a1Tex);
        rawImage.material.SetTexture(A2Tex, a2Tex);
        
        InvokeRepeating(nameof(IncreaseIslandIndex), 0, 1.0f);
    }

    void IncreaseIslandIndex()
    {
        islandIndex = (islandIndex + 1) % 4;
        rawImage.material.SetInt(IslandIndex, islandIndex);
    }
}