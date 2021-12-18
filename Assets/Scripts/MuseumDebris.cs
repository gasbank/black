using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class MuseumDebris : MonoBehaviour
{
    [SerializeField]
    bool toBeDestroyed;

    [SerializeField]
    GameObject poofPrefab;

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    public void OnClick()
    {
        if (toBeDestroyed) return;

        ConfirmPopup.instance.OpenYesNoPopup("잔해를 치울까요? 1골드가 필요합니다.", () =>
            {
                toBeDestroyed = true;

                var poof = Instantiate(poofPrefab, transform.parent).GetComponent<Poof>();
                
                var poofTransform = poof.transform;
                poofTransform.localPosition = transform.localPosition;
                poofTransform.localScale = Vector3.one;
                
                //poof.SetFinishAction(() => { Destroy(gameObject); });
                Destroy(gameObject);
                
                ConfirmPopup.instance.Close();
            },
            ConfirmPopup.instance.Close);
    }
}