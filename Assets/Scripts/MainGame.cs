using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class MainGame : MonoBehaviour {
    [SerializeField] Texture2D defaultTexture = null;
    [SerializeField] GridWorld gridWorld = null;
    [SerializeField] TextAsset islandData = null;
    Dictionary<uint, uint> islandColorByMinPointPrimitive;

    void Awake() {
        using (var stream = new MemoryStream(islandData.bytes)) {
            var formatter = new BinaryFormatter();
            islandColorByMinPointPrimitive = (Dictionary<uint, uint>)formatter.Deserialize(stream);
            stream.Close();
        }

        Debug.Log($"{islandColorByMinPointPrimitive.Count} islands loaded.");

        if (StageButton.currentStageTexture != null) {
            gridWorld.LoadTexture(StageButton.currentStageTexture, islandColorByMinPointPrimitive);
        } else {
            gridWorld.LoadTexture(defaultTexture, islandColorByMinPointPrimitive);
        }
    }
}
