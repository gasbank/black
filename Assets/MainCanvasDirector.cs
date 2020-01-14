using UnityEngine;

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
    }
    [SerializeField] Mode mode = Mode.Entering;

    void OnValidate() {
        switch (mode) {
            case Mode.Entering:
            break;
            case Mode.Finished:
            break;
            case Mode.Finishing:
            break;
            case Mode.Painting:
            break;
        }
    }
}
