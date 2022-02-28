using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class MuseumImage : MonoBehaviour
{
    [SerializeField]
    MuseumDebris[] debrisList;

    public bool IsAnyExclamationMarkShown => debrisList.Where(e => e != null).Any(e => e.IsExclamationMarkShown); 

    void OnEnable()
    {
        if (BlackContext.instance != null)
        {
            BlackContext.instance.OnGoldChanged += OnGoldChanged;
        }
    }

    void OnDisable()
    {
        if (BlackContext.instance != null)
        {
            BlackContext.instance.OnGoldChanged -= OnGoldChanged;
        }
    }

    void Start()
    {
        UpdateExclamationMark();
    }

    void OnGoldChanged()
    {
        UpdateExclamationMark();
    }

    void UpdateExclamationMark()
    {
        foreach (var t in debrisList)
        {
            if (t == null)
            {
                continue;
            }

            t.UpdateExclamationMark();
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    public void SetDebrisState(List<int> clearedDebrisIndexList)
    {
        for (var i = 0; i < debrisList.Length; i++)
        {
            if (debrisList[i] == null)
            {
                continue;
            }

            if (clearedDebrisIndexList.Contains(i))
            {
                debrisList[i].Close();
            }
            else
            {
                debrisList[i].Open();
            }
        }
    }

    public List<int> GetDebrisState()
    {
        var clearedDebrisIndexList = new List<int>();
        for (var i = 0; i < debrisList.Length; i++)
        {
            if (debrisList[i].IsOpen == false)
            {
                clearedDebrisIndexList.Add(i);
            }
        }

        return clearedDebrisIndexList;
    }
}