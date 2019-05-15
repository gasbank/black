using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainGame : MonoBehaviour {
    [SerializeField] Texture2D defaultTexture = null;
    [SerializeField] GridWorld gridWorld = null;

    void Awake() {
        if (StageButton.currentStageTexture != null) {
            gridWorld.LoadTexture(StageButton.currentStageTexture);
        } else {
            gridWorld.LoadTexture(defaultTexture);
        }
    }
}
