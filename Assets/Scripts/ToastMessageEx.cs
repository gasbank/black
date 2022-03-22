using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

#if UNITY_EDITOR
#endif

public class ToastMessageEx : MonoBehaviour
{
    [SerializeField]
    public Image background;

    [SerializeField]
    public Text message;

    [SerializeField]
    public Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void PlayGoodAnim()
    {
        Debug.Log("PlayGoodAnim");
        animator.Play("ToastMessageGood", -1, 0f);
    }

    public void PlayWarnAnim()
    {
        Debug.Log("PlayWarnAnim");
        animator.Play("ToastMessageWarn", -1, 0f);
    }
}