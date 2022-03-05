using System;
using UnityEngine;

[DisallowMultipleComponent]
public class ActiveDebrisButtonGroup : MonoBehaviour
{
    [SerializeField]
    MuseumDebris activeMuseumDebris;
    
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    void Update()
    {
        if (activeMuseumDebris == null)
        {
            return;
        }

        transform.position = activeMuseumDebris.transform.position;
    }
}