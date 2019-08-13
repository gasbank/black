using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetImage : MonoBehaviour {
    [SerializeField] Image targetImage = null;
    [SerializeField] Image targetImageOutine = null;
    [SerializeField] IslandLabelSpawner islandLabelSpawner = null;

    void OnEnable() {
        targetImage.material = Instantiate(targetImage.material);
    }

    void OnDisable() {
        Destroy(targetImage.material);
    }

    public void SetTargetImageMaterialTexture(Texture2D tex) {
        targetImage.material.SetTexture("ColorTexture", tex);
    }

    public void ToggleDebugView() {
        var alphaOffset = targetImage.material.GetFloat("AlphaOffset");
        var activateOutline = false;
        if (alphaOffset == 0) {
            // 디버그 렌더링
            alphaOffset = 1;
            activateOutline = false;
            SushiDebug.Log("Start debug view...");
        } else {
            // 일반 렌더링
            alphaOffset = 0;
            activateOutline = true;
            SushiDebug.Log("Start normal view...");
        }
        targetImage.material.SetFloat("AlphaOffset", alphaOffset);
        targetImageOutine.gameObject.SetActive(activateOutline);
        islandLabelSpawner.SetLabelBackgroundImageActive(activateOutline == false);
    }
}
