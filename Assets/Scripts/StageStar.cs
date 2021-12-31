using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StageStar : MonoBehaviour
{
    [SerializeField]
    GameObject[] starList;

    public int StarCount
    {
        set
        {
            foreach (var s in starList.Take(value)) s.SetActive(true);

            foreach (var s in starList.Skip(value)) s.SetActive(false);
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        starList = transform
            .Cast<Transform>()
            .Select(e => e.gameObject)
            .Where(e => e.TryGetComponent<Image>(out _))
            .ToArray();
    }
#endif
}