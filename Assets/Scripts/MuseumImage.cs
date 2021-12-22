using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class MuseumImage : MonoBehaviour
{
    [SerializeField]
    MuseumDebris[] debrisList;

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