using ConditionalDebug;
using UnityEngine;
using UnityEngine.UI;

public class TargetImage : MonoBehaviour
{
    [SerializeField]
    IslandLabelSpawner islandLabelSpawner;

    [SerializeField]
    RawImage targetImage;

    [SerializeField]
    Image targetImageOutine;

    void OnEnable()
    {
        //targetImage.material = Instantiate(targetImage.material);
    }

    void OnDisable()
    {
        //Destroy(targetImage.material);
    }

    public void SetTargetImageMaterialTexture(Texture2D tex)
    {
        targetImage.material.SetTexture("ColorTexture", tex);
    }

    public void SetTargetImageMaterial(Material material)
    {
        targetImage.material = material;
    }

    public void ToggleDebugView()
    {
        var alphaOffset = targetImage.material.GetFloat("AlphaOffset");
        var activateOutline = false;
        if (alphaOffset == 0)
        {
            // 디버그 렌더링
            alphaOffset = 1;
            activateOutline = false;
            ConDebug.Log("Start debug view...");
        }
        else
        {
            // 일반 렌더링
            alphaOffset = 0;
            activateOutline = true;
            ConDebug.Log("Start normal view...");
        }

        targetImage.material.SetFloat("AlphaOffset", alphaOffset);
        targetImageOutine.gameObject.SetActive(activateOutline);
        islandLabelSpawner.SetLabelBackgroundImageActive(activateOutline == false);
    }
}