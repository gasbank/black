using UnityEngine;
using UnityEngine.UI;

public class ToastMessageEx : MonoBehaviour
{
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
    }

    public void PlayWarnAnim(string msg)
    {
        Debug.Log("PlayWarnAnim");
        message.text = msg;
        animator.Play("ToastMessageWarn", -1, 0f);
    }
}