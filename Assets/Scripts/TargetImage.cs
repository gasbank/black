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
        var newState = targetImageOutine.gameObject.activeSelf == false;
        targetImageOutine.gameObject.SetActive(newState);   
        islandLabelSpawner.SetLabelBackgroundImageActive(newState == false);
    }
}