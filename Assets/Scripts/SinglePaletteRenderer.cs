using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class SinglePaletteRenderer : MonoBehaviour
{
    [SerializeField]
    RawImage targetRawImage;
    
    Material targetMaterial;
    
    static readonly int A1Tex = Shader.PropertyToID("_A1Tex");
    static readonly int A2Tex = Shader.PropertyToID("_A2Tex");
    static readonly int RenderColor = Shader.PropertyToID("_RenderColor");
    static readonly int PaletteIndex = Shader.PropertyToID("_PaletteIndex");

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    public void Initialize(StageMetadata stageMetadata)
    {   
        // Material 인스턴스 런타임에 복제 생성한다.

        if (targetRawImage != null)
        {
            targetRawImage.material = Instantiate(targetRawImage.material);
            targetMaterial = targetRawImage.material;
        }

        if (targetMaterial == null)
        {
            Debug.LogError("Target material null");
            return;
        }

        var a1Tex = stageMetadata.A1Tex;
        var a2Tex = stageMetadata.A2Tex;

        targetMaterial.SetTexture(A1Tex, a1Tex);
        targetMaterial.SetTexture(A2Tex, a2Tex);

        //targetMaterial.SetColor(RenderColor, new Color(0.3f, 0.3f, 0.3f, 0.3f));
        targetMaterial.SetColor(RenderColor, Color.red);
    }

    public void SetPaletteIndex(int paletteIndex)
    {
        targetMaterial.SetInt(PaletteIndex, paletteIndex);
    }
}