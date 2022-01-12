using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class IslandShader3DController : MonoBehaviour
{
    [SerializeField]
    StageMetadata stageMetadata;

    [SerializeField]
    Material targetMaterial;

    [SerializeField]
    bool fullRender;

    [SerializeField]
    TargetImageQuadCamera targetImageQuadCamera;

    [SerializeField]
    RawImage targetRawImage;

    StageData stageData;

    static readonly int A1Tex = Shader.PropertyToID("_A1Tex");
    static readonly int A2Tex = Shader.PropertyToID("_A2Tex");
    static readonly int Palette = Shader.PropertyToID("_Palette");
    static readonly int IslandIndex = Shader.PropertyToID("_IslandIndex");
    static readonly int FullRender = Shader.PropertyToID("_FullRender");
    static readonly int PaletteTex = Shader.PropertyToID("_PaletteTex");

    public void Initialize(StageMetadata inStageMetadata)
    {
        stageMetadata = inStageMetadata;
        
        if (stageMetadata != null)
        {
            targetMaterial = Instantiate(targetMaterial);
            if (targetRawImage != null)
            {
                targetRawImage.material = targetMaterial;
            }
        }

        var a1Tex = inStageMetadata.A1Tex;
        var a2Tex = inStageMetadata.A2Tex;

        targetMaterial.SetTexture(A1Tex, a1Tex);
        targetMaterial.SetTexture(A2Tex, a2Tex);

        using var stream = new MemoryStream(inStageMetadata.RawStageData.bytes);
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
            targetMaterial.SetColorArray(Palette, paletteArray);

            var paletteTex = new Texture2D(64, 1, TextureFormat.RGBA32, false);
            Color[] paddedPaletteArray;
            if (paletteArray.Length < 64)
            {
                paddedPaletteArray = paletteArray.Concat(Enumerable.Repeat(Color.black, 64 - paletteArray.Length))
                    .ToArray();
            }
            else
            {
                paddedPaletteArray = paletteArray;
            }

            paletteTex.SetPixels(paddedPaletteArray);
            paletteTex.filterMode = FilterMode.Point;
            paletteTex.wrapMode = TextureWrapMode.Clamp;
            paletteTex.Apply();

            targetMaterial.SetTexture(PaletteTex, paletteTex);
        }

        EnqueueIslandIndex(0);

        targetMaterial.SetFloat(FullRender, fullRender ? 1 : 0);

        if (targetImageQuadCamera != null)
        {
            targetImageQuadCamera.ClearCameraOnce();
        }
    }

    public void SetIslandIndex(int islandIndex)
    {
        targetMaterial.SetInt(IslandIndex, islandIndex);
        if (targetImageQuadCamera)
        {
            targetImageQuadCamera.RenderOneFrame();
        }
    }

    public void EnqueueIslandIndex(int islandIndex)
    {
        if (targetImageQuadCamera != null)
        {
            targetImageQuadCamera.EnqueueIslandIndex(islandIndex);
        }
    }
}