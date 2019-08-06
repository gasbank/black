using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetImage : MonoBehaviour {
    [SerializeField] Image targetImage = null;

    void OnEnable() {
        targetImage.material = Instantiate(targetImage.material);
    }

    void OnDisable() {
        Destroy(targetImage.material);
    }

    public void SetTargetImageMaterialTexture(Texture2D tex) {
        targetImage.material.SetTexture("ColorTexture", tex);
    }
}
