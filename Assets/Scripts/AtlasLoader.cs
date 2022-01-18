using ConditionalDebug;
using UnityEngine;
using UnityEngine.U2D;

//
// 사용하지도 않는 Atlas를 자꾸 로딩하려다가 실패했다고 경고 메시지가 뜨는 게 귀찮아서 더미로 하나 만들었다.
// (어딘가 참조가 걸려있는지는 모르겠지만, 찾기가 귀찮다.)
//
[DisallowMultipleComponent]
public class AtlasLoader : MonoBehaviour
{
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif
    
    void OnEnable()
    {
        SpriteAtlasManager.atlasRequested += RequestAtlas;
    }

    void OnDisable()
    {
        SpriteAtlasManager.atlasRequested -= RequestAtlas;
    }

    void RequestAtlas(string atlasTag, System.Action<SpriteAtlas> callback)
    {
        ConDebug.Log($"Atlas loading request ignored intentionally. Tag={atlasTag}", this);
        callback(null);
    }
}