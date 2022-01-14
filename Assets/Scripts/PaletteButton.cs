using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PaletteButton : MonoBehaviour
{
    [SerializeField]
    GameObject check;

    [SerializeField]
    TextMeshProUGUI colorNumberText;

    [SerializeField]
    uint colorUint;

    [SerializeField]
    Image gauge;

    [SerializeField]
    Image image;

    public bool Check
    {
        get => check.activeSelf;
        // ReSharper disable once ValueParameterNotUsed
        set
        {
            foreach (Transform t in transform.parent)
            {
                var pb = t.GetComponent<PaletteButton>();
                if (pb != null)
                {
                    var active = t == transform;
                    pb.check.SetActive(active);
                    if (active)
                    {
                        pb.NotifyPaletteChange();            
                    }
                }
            }
        }
    }

    void NotifyPaletteChange()
    {
        MainGame.instance.OnPaletteChange(int.Parse(colorNumberText.text) - 1);
    }

    public float ColoredRatio
    {
        set => gauge.fillAmount = value;
        get => gauge.fillAmount;
    }

    public Color PaletteColor => image.color;
    public uint ColorUint => colorUint;

    public int ColorIndex
    {
        get => int.Parse(colorNumberText.text);
        set => colorNumberText.text = value.ToString();
    }

    public void SetColor(uint colorUint)
    {
        this.colorUint = colorUint;
        image.color = BlackConvert.GetColor(colorUint);
        colorNumberText.color = image.color.grayscale < 0.5f ? Color.white : Color.black;
    }

    void Awake()
    {
        ColoredRatio = 0;
    }

    public void PlayCheckSound()
    {
        Sound.instance.PlayButtonClick();
    }
}