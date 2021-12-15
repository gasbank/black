// https://forum.unity.com/threads/little-script-apply-and-revert-several-prefab-at-once.295311/

using UnityEditor;
using UnityEngine;

public class ApplySelectedPrefabs : EditorWindow {
    [MenuItem("Tools/Apply all selected prefabs", false, 5)]
    static void ApplyPrefabs() {
        foreach (GameObject go in Selection.gameObjects) {
            PrefabUtility.ApplyPrefabInstance(go, InteractionMode.UserAction);
        }
    }

    [MenuItem("Tools/Revert all selected prefabs", false, 6)]
    static void ResetPrefabs() {
        foreach (GameObject go in Selection.gameObjects) {
            PrefabUtility.RevertObjectOverride(go, InteractionMode.UserAction);
        }
    }

    [MenuItem("Tools/Revert all selected prefab instances", false, 7)]
    static void ResetPrefabInstances() {
        foreach (GameObject go in Selection.gameObjects) {
            PrefabUtility.RevertPrefabInstance(go, InteractionMode.UserAction);
        }
    }

    [MenuItem("Tools/Revert all selected prefab instances (transform) %#w", false, 8)]
    static void ResetToPrefabState() {
        foreach (GameObject go in Selection.gameObjects) {
            var transform = go.GetComponent<Transform>();
            if (transform != null) {
                RevertPropertyOverride(transform);
            }
        }
    }

    private static void RevertPropertyOverride(Component rt) {
        var serializedObject = new SerializedObject(rt);
        var serializedProperty = serializedObject.GetIterator();
        while (serializedProperty.NextVisible(true)) {
            PrefabUtility.RevertPropertyOverride(serializedProperty, InteractionMode.UserAction);
        }
    }
}
