using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class TopProfile : MonoBehaviour
{
    [SerializeField]
    RawImage rawImage;

    [SerializeField]
    Text nickname;

    [SerializeField]
    bool lastAuthenticated;

    static bool Authenticated => Social.localUser != null && Social.localUser.authenticated;

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    void Start()
    {
        UpdateProfile();
        InvokeRepeating(nameof(UpdateProfile), 0.0f, 1.0f);
    }

    void Update()
    {
        var authenticated = Authenticated;
        if (lastAuthenticated == false && authenticated)
        {
            // 이번 프레임에 로그인했다.
            UpdateProfile();
        }
        else if (lastAuthenticated && authenticated == false)
        {
            // 이번 프레임에 로그아웃했다.
            UpdateProfile();
        }

        lastAuthenticated = authenticated;
    }

    void UpdateProfile()
    {
        if (Authenticated)
        {
            nickname.text = Social.localUser.userName;
            rawImage.texture = Social.localUser.image;
        }
        else
        {
            nickname.text = "\\(오프라인)".Localized();
            rawImage.texture = null;
        }
    }
}