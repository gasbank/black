using System;
using Dirichlet.Numerics;
using JetBrains.Annotations;
using UnityEngine;
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

    public event Action Cleared;

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
        
        Sound.Instance.PlayButtonClick();

        ConfirmPopup.Instance.OpenYesNoPopup(
            @"\잔해를 치울까요? {0}골드가 필요합니다.".Localized(clearPrice),
            TryClearDebris,
            ConfirmPopup.Instance.Close);
    }

    void TryClearDebris()
    {
        if (BlackContext.Instance.Gold < clearPrice.ToUInt128())
        {
            ConfirmPopup.Instance.Open(
                @"\골드가 부족합니다.".Localized(),
                ConfirmPopup.Instance.Close);
            
            Sound.Instance.PlayErrorBuzzer();
        }
        else
        {
            ClearDebrisInternal(true, true);
        }
    }

    public void ClearForAdmin()
    {
#if DEV_BUILD
        if (IsOpen)
        {
            ClearDebrisInternal(false, false);
        }
#endif
    }

    void ClearDebrisInternal(bool spendGold, bool closeConfirmPopup)
    {
        if (spendGold)
        {
            BlackContext.Instance.SubtractGold(clearPrice);
        }

        toBeClosed = true;

        var poof = Instantiate(poofPrefab, transform.parent).GetComponent<Poof>();

        var poofTransform = poof.transform;
        poofTransform.localPosition = transform.localPosition;
        poofTransform.localScale = Vector3.one;

        Sound.Instance.PlayWhooshAir();

        Close();

        if (closeConfirmPopup)
        {
            ConfirmPopup.Instance.Close();
        }

        Cleared?.Invoke();
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
        if (BlackContext.Instance != null)
        {
            exclamationMark.gameObject.SetActive(subcanvas.IsOpen &&
                                                 BlackContext.Instance.Gold >= clearPrice.ToUInt128());
        }
    }
}
