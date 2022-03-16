using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class CharacterSpawner : MonoBehaviour
{
    [SerializeField]
    GameObject characterPrefab;

    [SerializeField]
    GameObject character3DPrefab;

    [SerializeField]
    Transform spawnPoint;

    [SerializeField]
    Transform characterParent;

    [SerializeField]
    Transform character3DParent;

    [SerializeField]
    Camera cam;

    [SerializeField]
    Camera cam3D;

    [SerializeField]
    Sort3DGroup sort3DGroup;

    [SerializeField]
    Sprite[] visitorSpriteList;

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    IEnumerator Start()
    {
        while (Application.isPlaying && sort3DGroup.RuntimeListCount < 20)
        {
            SpawnVisitor();
            yield return new WaitForSeconds(3.0f);
        }
    }

    void SpawnVisitor()
    {
        var character = Instantiate(characterPrefab, characterParent).GetComponent<Character>();
        var char3D = Instantiate(character3DPrefab, character3DParent).GetComponent<Character3D>();

        character.Char3D = char3D;
        char3D.Character = character;

        character.Cam = cam;
        char3D.Cam = cam3D;

        char3D.SetAgentPosition(spawnPoint.position);

        sort3DGroup.Add(char3D);

        var visitorSpriteIndex = Random.Range(0, visitorSpriteList.Length / 2 - 1) * 2;

        character.Sprite1 = visitorSpriteList[visitorSpriteIndex];
        character.Sprite2 = visitorSpriteList[visitorSpriteIndex + 1];
    }
}