using System;
using UnityEngine;

[DisallowMultipleComponent]
public class EaselExclamationMark : MonoBehaviour
{
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    void Start()
    {
        gameObject.SetActive(StageDetail.IsAllCleared == false);
    }
}