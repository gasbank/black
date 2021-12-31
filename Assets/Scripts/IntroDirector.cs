using UnityEngine;
using UnityEngine.Playables;

[DisallowMultipleComponent]
public class IntroDirector : MonoBehaviour
{
    public static IntroDirector instance;
    
    [SerializeField]
    PlayableDirector director;
    
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    public void ResumeDirector()
    {
        director.Resume();
    }
}