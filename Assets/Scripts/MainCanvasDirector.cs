using UnityEditor;
using UnityEngine;

public class MainCanvasDirector : MonoBehaviour
{
    [SerializeField]
    CanvasGroup coinGroup;

    [SerializeField]
    CanvasGroup debugGroup;

#if UNITY_EDITOR
    [SerializeField]
    Mode mode = Mode.Entering;
#endif

    [SerializeField]
    CanvasGroup paletteGroup;

#if UNITY_EDITOR
    void OnValidate()
    {
        EditorApplication.delayCall += () =>
        {
            switch (mode)
            {
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

#if UNITY_EDITOR
    enum Mode
    {
        Entering,
        Painting,
        Finishing,
        Finished,
        Debug
    }
#endif
}