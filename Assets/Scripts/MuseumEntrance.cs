using System;
using UnityEngine;

[DisallowMultipleComponent]
public class MuseumEntrance : MonoBehaviour
{
    [SerializeField]
    CharacterSpawner charSpawner;
    
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    void OnTriggerExit(Collider other)
    {
        var char3D = other.GetComponentInParent<Character3D>();
        if (char3D == null) return;

        // 입장 완료 (입구 영역에서 벗어남)
        char3D.SetEnterFinished();
    }

    void OnTriggerEnter(Collider other)
    {
        var char3D = other.GetComponentInParent<Character3D>();
        if (char3D == null) return;

        // 입장 완료가 된 상태에서만 퇴장할 수 있음
        if (char3D.IsEnterFinished)
        {
            charSpawner.Despawn(char3D);
        }
    }
}