using System;
using UnityEngine;
using UnityEngine.Playables;

[DisallowMultipleComponent]
public class IntroDirector : MonoBehaviour
{
    [SerializeField]
    PlayableDirector director;
    
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    void Start()
    {
    }
}