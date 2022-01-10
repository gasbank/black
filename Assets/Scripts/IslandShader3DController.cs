using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class IslandShader3DController : MonoBehaviour
{
    [SerializeField]
    MeshRenderer rawImage;

    [SerializeField]
    [Range(0, 4)]
    int islandIndex;

    [SerializeField]
    bool fullRender;
    
    [SerializeField]
    bool singleIsland;

    [SerializeField]
    TargetImageQuadCamera targetImageQuadCamera;
    
    StageData stageData;

    static readonly int A1Tex = Shader.PropertyToID("_A1Tex");
    static readonly int A2Tex = Shader.PropertyToID("_A2Tex");
    static readonly int Palette = Shader.PropertyToID("_Palette");
    static readonly int IslandIndex = Shader.PropertyToID("_IslandIndex");
    static readonly int FullRender = Shader.PropertyToID("_FullRender");
    static readonly int SingleIsland = Shader.PropertyToID("_SingleIsland");
    static readonly int PaletteTex = Shader.PropertyToID("_PaletteTex");

    public void Initialize(StageMetadata stageMetadata)
    {
        var a1Tex = stageMetadata.A1Tex;
        var a2Tex = stageMetadata.A2Tex;

        rawImage.material.SetTexture(A1Tex, a1Tex);
        rawImage.material.SetTexture(A2Tex, a2Tex);

        using var stream = new MemoryStream(stageMetadata.RawStageData.bytes);
        var formatter = new BinaryFormatter();
        stageData = (StageData) formatter.Deserialize(stream);
        stream.Close();

        var colorUintArray =
            new[] {BlackConvert.GetC(new Color32(0, 0, 0, 255))} // 팔레트의 0번째는 언제나 검은색으로, 아웃라인 전용으로 예약되어 있다.
                .Concat(stageData.CreateColorUintArray())
                .ToArray();

        if (colorUintArray.Length > 64)
        {
            Debug.LogError("Maximum palette size 64 exceeded.");
        }
        else
        {
            var paletteArray = colorUintArray.Select(BlackConvert.GetColor).ToArray();
            rawImage.material.SetColorArray(Palette, paletteArray);
            
            var paletteTex = new Texture2D(64, 1, TextureFormat.RGBA32, false);
            Color[] paddedPaletteArray;
            if (paletteArray.Length < 64)
            {
                paddedPaletteArray = paletteArray.Concat(Enumerable.Repeat(Color.black, 64 - paletteArray.Length)).ToArray();
            }
            else
            {
                paddedPaletteArray = paletteArray;
            }
            paletteTex.SetPixels(paddedPaletteArray);
            paletteTex.filterMode = FilterMode.Point;
            paletteTex.wrapMode = TextureWrapMode.Clamp;
            paletteTex.Apply();

            rawImage.material.SetTexture(PaletteTex, paletteTex);
        }

        SetIslandIndex(0);
        
        rawImage.material.SetFloat(FullRender, fullRender ? 1 : 0);
        rawImage.material.SetFloat(SingleIsland, singleIsland ? 1 : 0);
        
        targetImageQuadCamera.ClearCameraOnce();
    }

    public void SetIslandIndex(int inIslandIndex)
    {
        islandIndex = inIslandIndex;
        rawImage.material.SetInt(IslandIndex, islandIndex);
        targetImageQuadCamera.RenderOneFrame();
    }
}