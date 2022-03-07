using Dirichlet.Numerics;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class MuseumDebris : MonoBehaviour
{
    [SerializeField]
    [AutoBindThis]
    Subcanvas subcanvas;
    
    [SerializeField]
    bool toBeClosed;

    [SerializeField]
    GameObject poofPrefab;

    [SerializeField]
    ScUInt128 clearPrice = 1;

    [SerializeField]
    EaselExclamationMark exclamationMark;

    public bool IsExclamationMarkShown
    {
        get
        {
            UpdateExclamationMark(); 
            return subcanvas.IsOpen && exclamationMark.gameObject.activeInHierarchy;
        }
    }
    
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
        exclamationMark = transform.GetComponentInChildren<EaselExclamationMark>();
    }
#endif

    public bool IsOpen => subcanvas.IsOpen;

    public void Open()
    {
        subcanvas.Open();
        UpdateExclamationMark();
    }

    public void Close()
    {
        subcanvas.Close();
    }

    public void OnClick()
    {
        if (toBeClosed) return;
        
        Sound.instance.PlayButtonClick();

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
            
            Sound.instance.PlayErrorBuzzer();
        }
        else
        {
            BlackContext.instance.SubtractGold(clearPrice);
            toBeClosed = true;

            var poof = Instantiate(poofPrefab, transform.parent).GetComponent<Poof>();

            var poofTransform = poof.transform;
            poofTransform.localPosition = transform.localPosition;
            poofTransform.localScale = Vector3.one;

            Sound.instance.PlayWhooshAir();
            
            Close();
            
            ConfirmPopup.instance.Close();
        }
    }

    [UsedImplicitly]
    void OpenPopup()
    {
    }

    [UsedImplicitly]
    void ClosePopup()
    {
    }

    public void UpdateExclamationMark()
    {
        if (BlackContext.instance != null)
        {
            exclamationMark.gameObject.SetActive(subcanvas.IsOpen &&
                                                 BlackContext.instance.Gold >= clearPrice.ToUInt128());
        }
    }
}