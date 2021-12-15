using UnityEditor;
using UnityEngine;

public class MainCanvasDirector : MonoBehaviour
{
    [SerializeField]
    CanvasGroup coinGroup;

    [SerializeField]
    CanvasGroup debugGroup;

    [SerializeField]
    Mode mode = Mode.Entering;

    [SerializeField]
    CanvasGroup paletteGroup;

    [SerializeField]
    CanvasGroup achieveGroup;

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
                    achieveGroup.Hide();
                    break;
                case Mode.Finished:
                    debugGroup.Hide();
                    coinGroup.Hide();
                    paletteGroup.Hide();
                    achieveGroup.Hide();
                    break;
                case Mode.Finishing:
                    debugGroup.Hide();
                    coinGroup.Hide();
                    paletteGroup.Hide();
                    achieveGroup.Hide();
                    break;
                case Mode.Painting:
                    debugGroup.Hide();
                    coinGroup.Show();
                    paletteGroup.Show();
                    achieveGroup.Hide();
                    break;
                case Mode.Debug:
                    debugGroup.Show();
                    coinGroup.Show();
                    paletteGroup.Show();
                    achieveGroup.Hide();
                    break;
            }
        };
    }
#endif

    enum Mode
    {
        Entering,
        Painting,
        Finishing,
        Finished,
        Debug
    }
}