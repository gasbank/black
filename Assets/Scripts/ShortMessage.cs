using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ShortMessage : MonoBehaviour, IPlatformShortMessage
{
    public static IPlatformShortMessage instance;
    public Image image;

    [SerializeField]
    Sprite logSprite;

    IEnumerator playingAnimation;
    public float stretchTime = 0.2f;
    public Text text;
    public float visibleTime = 3.0f;

    [SerializeField]
    Sprite warningSprite;

    public void Show(string message, bool isWarning = false)
    {
        gameObject.SetActive(true);
        image.sprite = isWarning ? warningSprite : logSprite;

        // 애니메이션이 이미 실행 중일 경우 정지
        if (playingAnimation != null) StopCoroutine(playingAnimation);

        // 애니메이션 실행
        playingAnimation = ShowAnimation(message);
        StartCoroutine(playingAnimation);

        Sound.instance.PlayError();
    }

    void Awake()
    {
        image = GetComponent<Image>();
    }

    IEnumerator ShowAnimation(string message)
    {
        text.text = string.Empty;

        // y축 늘리면서 표시
        var t = 0f;
        var step = Time.deltaTime / stretchTime;
        while (t < 1f)
        {
            var yScale = Mathf.Lerp(0f, 1f, t);
            transform.localScale = new Vector3(1f, yScale, 1f);
            t += step;

            // 절반이 지났을 때 텍스트 표시
            if (t > 0.5f) text.text = message;

            yield return null;
        }

        transform.localScale = Vector3.one;

        yield return new WaitForSeconds(visibleTime);

        // y축 줄이면서 숨김
        t = 0f;
        while (t < 1f)
        {
            var yScale = Mathf.Lerp(1f, 0f, t);
            transform.localScale = new Vector3(1f, yScale, 1f);
            t += step;

            // 절반이 지났을 때 텍스트 숨김
            if (t > 0.5f) text.text = string.Empty;
            yield return null;
        }

        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);

        playingAnimation = null;
    }
}