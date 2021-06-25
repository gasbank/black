using UnityEngine;

public class MainCanvasDirector : MonoBehaviour {
    [SerializeField] CanvasGroup debugGroup;
    [SerializeField] CanvasGroup coinGroup;
    [SerializeField] CanvasGroup paletteGroup;

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
                    coinGroup.Hide();
                    paletteGroup.Hide();
                    break;
                case Mode.Finished:
                    debugGroup.Hide();
                    coinGroup.Hide();
                    paletteGroup.Hide();
                    break;
                case Mode.Finishing:
                    debugGroup.Hide();
                    coinGroup.Hide();
                    paletteGroup.Hide();
                    break;
                case Mode.Painting:
                    debugGroup.Hide();
                    coinGroup.Show();
                    paletteGroup.Show();
                    break;
                case Mode.Debug:
                    debugGroup.Show();
                    coinGroup.Show();
                    paletteGroup.Show();
                    break;
            }
        };
    }
#endif
}
