using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ImageAspectRatioHeightFitter : UIBehaviour
{
    [SerializeField]
    Image image;

    [SerializeField]
    RectTransform rt;

    void Update()
    {
        if (rt != null && image != null)
        {
            var spriteAspectRatio = (float) image.sprite.texture.width / image.sprite.texture.height;
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, rt.sizeDelta.x / spriteAspectRatio);
        }
    }
}