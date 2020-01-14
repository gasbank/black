using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameThickness : MonoBehaviour
{
    [SerializeField] float thickness = 20.0f;
    [SerializeField] RectTransform m00 = null;
    [SerializeField] RectTransform m01 = null;
    [SerializeField] RectTransform m02 = null;
    [SerializeField] RectTransform m10 = null;
    [SerializeField] RectTransform m12 = null;
    [SerializeField] RectTransform m20 = null;
    [SerializeField] RectTransform m21 = null;
    [SerializeField] RectTransform m22 = null;

#if UNITY_EDITOR
    void OnValidate() {
        if (Application.isPlaying == false) {
            UnityEditor.EditorApplication.delayCall += () => {
                m00.sizeDelta = m02.sizeDelta = m20.sizeDelta = m22.sizeDelta = Vector2.one * thickness;
                m01.sizeDelta = m21.sizeDelta = new Vector2(m01.sizeDelta.x, thickness);
                m10.sizeDelta = m12.sizeDelta = new Vector2(thickness, m10.sizeDelta.y);
            };
        }
    }
#endif
}
