using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboEffect : MonoBehaviour
{
    [SerializeField]
    public Sprite[] ComboNumL1;

    [SerializeField]
    public Sprite[] ComboNumL2;

    [SerializeField]
    public Sprite[] ComboNumL3;

    [SerializeField]
    public Sprite[] ComboNumL4;

    [SerializeField]
    public Sprite[] ComboNumL5;

    [SerializeField]
    public Sprite[] ComboNumL6;

    [SerializeField]
    public GameObject[] PlaceHolder;

    [SerializeField]
    public Image ComboText;

    [SerializeField]
    public GameObject ComboEffectPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator ComboAnimation(ScInt combo)
    {
        var obj = Instantiate(ComboEffectPrefab, transform.parent, false).GetComponent<ComboEffect>();
        var position = obj.transform.position;

        Sprite[] numSprite = null;
        Vector2 size = new Vector2();
        var level = combo / 10;
        switch (level.ToInt())
        {
            case 0:
                numSprite = ComboNumL1;
                size.x = 35;
                size.y = 46;
                break;
            case 1:
                numSprite = ComboNumL2;
                size.x = 35;
                size.y = 46;
                break;
            case 2:
                numSprite = ComboNumL3;
                size.x = 35;
                size.y = 46;
                break;
            case 3:
                numSprite = ComboNumL4;
                size.x = 44;
                size.y = 57;
                break;
            case 4:
                numSprite = ComboNumL5;
                size.x = 44;
                size.y = 57;
                break;
            case 5:
                numSprite = ComboNumL6;
                size.x = 47;
                size.y = 67;
                break;
            default:
                numSprite = ComboNumL6;
                size.x = 47;
                size.y = 67;
                break;
        }

        var nums = new List<int>();
        while (combo > 0)
        {
            nums.Add(combo % 10);
            combo /= 10;
        }
        nums.Reverse();

        var comboPos = obj.ComboText.rectTransform.anchoredPosition;  // 139x39
        for (var idx = 0; idx < nums.Count; idx++)
        {
            var elem = obj.PlaceHolder[idx].GetComponent<Image>();
            elem.sprite = numSprite[nums[idx]];
            elem.rectTransform.sizeDelta = size;
            elem.rectTransform.anchoredPosition = new Vector3(
                comboPos.x - (60 + (size.x - 3) * (nums.Count - idx)),
                comboPos.y
            );

            elem.enabled = true;
        }
        
        obj.ComboText.enabled = true;

        const float duration = 0.65f;
        const float shift = 0.7f;
        var currentTime = 0f;
        while(currentTime < duration)
        {
            var alpha = Mathf.Lerp(1f, 0f, currentTime/duration);
            foreach (var num in obj.PlaceHolder)
            {
                var tmpColor = num.GetComponent<Image>().color;
                tmpColor.a = alpha;
                num.GetComponent<Image>().color = tmpColor;
            }

            var color = obj.ComboText.color;
            color.a = alpha;
            obj.ComboText.color = color;
            
            obj.transform.position = new Vector3(
                position.x, 
                position.y + shift * (1f - alpha),
                position.z);
            currentTime += Time.deltaTime; 
            yield return null;
        }

        foreach (var num in obj.PlaceHolder)
            num.GetComponent<Image>().enabled = false;
        obj.ComboText.enabled = false;
        obj.enabled = false;

        Destroy(obj.gameObject);
        yield break;
// var text = Instantiate(comboText, comboText.transform.parent, false);
//         obj.text = $"{BlackContext.instance.StageCombo} Combo{(BlackContext.instance.StageCombo == 1 ? "" : "s")}";
//         var position = comboText.transform.position;
//         text.transform.position = position;
//         text.enabled = true;
//         
//         float duration = 0.60f;
//         float shift = 0.7f;
//         float currentTime = 0f;
//         while(currentTime < duration)
//         {
//             float alpha = Mathf.Lerp(1f, 0f, currentTime/duration);
//             text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
//             text.transform.position = new Vector3(
//                 position.x, 
//                 position.y + shift * (1f - alpha),
//                 position.z);
//             currentTime += Time.deltaTime;
//             yield return null;
//         }
//         
//         text.enabled = false;
//         Destroy(text);
//         yield break;
    }
    
    public void Play(ScInt combo)
    {
        StartCoroutine(nameof(ComboAnimation), combo);
    }
}
