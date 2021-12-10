using UnityEngine;

public static class CanvasGroupUtil {
    public static void SetActiveCheap(this CanvasGroup canvasGroup, bool active) {
        canvasGroup.alpha = active ? 1 : 0;
        canvasGroup.blocksRaycasts = active;
    }
}
