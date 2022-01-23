using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToastMessage : MonoBehaviour
{
    public static ToastMessage instance;

    [SerializeField]
    public GameObject WarnUI;

    [SerializeField]
    public Text WarnMessage;

    [SerializeField]
    public GameObject InfoUI;

    [SerializeField]
    public Text InfoMessage;

    [SerializeField]
    public GameObject InfoActionUI;

    [SerializeField]
    public Text InfoActionDescMessage;

    [SerializeField]
    public Text InfoActionClickMessage;

    private IEnumerator runningCoroutine;
    private GameObject currentUI;

    public void ShowWarnMessage(string message)
    {
        WarnMessage.text = message;
        currentUI = WarnUI;

        if (runningCoroutine != null)
        {
            currentUI.SetActive(false);
            StopCoroutine(runningCoroutine);
        }

        runningCoroutine = ShowAndHide(WarnUI);
        StartCoroutine(runningCoroutine);
    }

    public void ShowInfoMessage(string message)
    {
    }

    public void ShowInfoActionMessage(string message)
    {
    }

    private void ShowMessage()
    {
    }

    private IEnumerator ShowAndHide(GameObject ui)
    {
        const float fadeInSpeed = 5.0f;
        const float fadeOutSpeed = 1.1f;

        var cg = GetComponent<CanvasGroup>();
        cg.alpha = 0;
        ui.SetActive(true);

        while (cg.alpha < 1)
        {
            cg.alpha += fadeInSpeed * Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(1.0f);

        while (cg.alpha > 0)
        {
            cg.alpha -= fadeOutSpeed * Time.deltaTime;
            yield return null;
        }

        ui.SetActive(false);
        yield break;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
