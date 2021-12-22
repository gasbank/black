using Dirichlet.Numerics;
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

    [SerializeField]
    ScUInt128 clearPrice = 1;

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    public void OnClick()
    {
        if (toBeDestroyed) return;

        ConfirmPopup.instance.OpenYesNoPopup(
            @"\잔해를 치울까요? {0}골드가 필요합니다.".Localized(clearPrice),
            TryClearDebris,
            ConfirmPopup.instance.Close);
    }

    void TryClearDebris()
    {
        if (BlackContext.instance.Gold < clearPrice.ToUInt128())
        {
            ConfirmPopup.instance.Open(
                @"\골드가 부족합니다.".Localized(),
                ConfirmPopup.instance.Close);
        }
        else
        {
            BlackContext.instance.SubtractGold(clearPrice);
            toBeDestroyed = true;

            var poof = Instantiate(poofPrefab, transform.parent).GetComponent<Poof>();

            var poofTransform = poof.transform;
            poofTransform.localPosition = transform.localPosition;
            poofTransform.localScale = Vector3.one;

            //poof.SetFinishAction(() => { Destroy(gameObject); });
            Destroy(gameObject);

            ConfirmPopup.instance.Close();
        }
    }
}