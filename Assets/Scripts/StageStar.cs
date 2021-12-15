using System.Linq;
using UnityEngine;

public class StageStar : MonoBehaviour
{
    [SerializeField]
    GameObject[] starList;

    public int StarCount
    {
        get => starList.Count(e => e.activeSelf);
        set
        {
            foreach (var s in starList.Take(value)) s.SetActive(true);

            foreach (var s in starList.Skip(value)) s.SetActive(false);
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        starList = transform.Cast<Transform>().Select(e => e.gameObject).ToArray();
    }
#endif
}