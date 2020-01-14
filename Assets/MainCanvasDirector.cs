using UnityEngine;

public static class CanvasGroupExtension {
    public static void Show(this CanvasGroup cg) {
        cg.alpha = 1.0f;
        cg.blocksRaycasts = true;
    }

    public static void Hide(this CanvasGroup cg) {
        cg.alpha = 0.0f;
        cg.blocksRaycasts = false;
    }
}

public class MainCanvasDirector : MonoBehaviour {
    [SerializeField] CanvasGroup debugGroup = null;
    [SerializeField] CanvasGroup nameplateGroup = null;
    [SerializeField] CanvasGroup coinGroup = null;
    [SerializeField] CanvasGroup paletteGroup = null;



    enum Mode {
        Entering,
        Painting,
        Finishing,
        Finished,
        Debug,
    }
    [SerializeField] Mode mode = Mode.Entering;

#if UNITY_EDITOR
    void OnValidate() {
        UnityEditor.EditorApplication.delayCall += () => {
            switch (mode) {
                case Mode.Entering:
                    debugGroup.Hide();
                    nameplateGroup.Hide();
                    coinGroup.Hide();
                    paletteGroup.Hide();
                    break;
                case Mode.Finished:
                    debugGroup.Hide();
                    nameplateGroup.Show();
                    coinGroup.Hide();
                    paletteGroup.Hide();
                    break;
                case Mode.Finishing:
                    debugGroup.Hide();
                    nameplateGroup.Hide();
                    coinGroup.Hide();
                    paletteGroup.Hide();
                    break;
                case Mode.Painting:
                    debugGroup.Hide();
                    nameplateGroup.Hide();
                    coinGroup.Show();
                    paletteGroup.Show();
                    break;
                case Mode.Debug:
                    debugGroup.Show();
                    nameplateGroup.Show();
                    coinGroup.Show();
                    paletteGroup.Show();
                    break;
            }
        };
    }
#endif
}
