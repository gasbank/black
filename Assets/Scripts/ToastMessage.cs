using UnityEngine;
using UnityEngine.UI;

public class ToastMessage : MonoBehaviour
{
    public static ToastMessage instance;

    [SerializeField]
    public Image background;

    [SerializeField]
    public Text message;

    [SerializeField]
    public Animator animator;


    public void PlayGoodAnim(string msg)
    {
        Debug.Log("PlayGoodAnim");

        message.text = msg;

        animator.Play("ToastMessageGood", -1, 0f);
        Sound.instance.PlayJingleAchievement();
    }

    public void PlayWarnAnim(string msg)
    {
        Debug.Log("PlayWarnAnim");

        message.text = msg;
        animator.Play("ToastMessageWarn", -1, 0f);
        Sound.instance.PlayFillError();
    }
}