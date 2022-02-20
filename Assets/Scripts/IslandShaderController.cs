using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class IslandShaderController : MonoBehaviour
{
    [SerializeField]
    RawImage rawImage;

    [SerializeField]
    [Range(0, 4)]
    int islandIndex;

#if ADDRESSABLES
    [SerializeField]
    AssetReferenceStageMetadata stageMetadataRef;
#endif
    [SerializeField]
    bool fullRender;

    [SerializeField]
    bool singleIsland;

    StageData stageData;

    static readonly int A1Tex = Shader.PropertyToID("_A1Tex");
    static readonly int A2Tex = Shader.PropertyToID("_A2Tex");
    static readonly int Palette = Shader.PropertyToID("_Palette");
    static readonly int IslandIndex = Shader.PropertyToID("_IslandIndex");
    static readonly int FullRender = Shader.PropertyToID("_FullRender");
    static readonly int SingleIsland = Shader.PropertyToID("_SingleIsland");

#if ADDRESSABLES
    async void Start()
    {
        var stageMetadata = await stageMetadataRef.LoadAssetAsync().Task;

        var a1Tex = stageMetadata.A1Tex;
        var a2Tex = stageMetadata.A2Tex;

        rawImage.materialForRendering.SetTexture(A1Tex, a1Tex);
        rawImage.materialForRendering.SetTexture(A2Tex, a2Tex);

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
            rawImage.materialForRendering.SetColorArray(Palette, paletteArray);
        }

        rawImage.materialForRendering.SetInt(IslandIndex, 0);

        InvokeRepeating(nameof(IncreaseIslandIndex), 5.0f, 0.01f);
    }
#endif

    void Update()
    {
        rawImage.materialForRendering.SetFloat(FullRender, fullRender ? 1 : 0);
        rawImage.materialForRendering.SetFloat(SingleIsland, singleIsland ? 1 : 0);
    }

    void IncreaseIslandIndex()
    {
        islandIndex = (islandIndex + 1) % (1 + stageData.islandDataByMinPoint.Count); // 첫 번째 섬은 언제나 외곽선 전용이다.
        rawImage.material.SetInt(IslandIndex, islandIndex);
    }
}