using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

[DisallowMultipleComponent]
public class DialogGroup : MonoBehaviour
{
    [SerializeField]
    Subcanvas subcanvas;

    [SerializeField]
    Animator dialogContentAnimator;

    [SerializeField]
    JasoTypewriter talkTypewriter;

    [SerializeField]
    Sprite checkHintFeatureGuideSprite;

    TaskCompletionSource<bool> tryNextTsc;

    static readonly int Appear = Animator.StringToHash("Appear");
    static readonly int Disappear = Animator.StringToHash("Disappear");

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    async void Start()
    {
        // 광고 로딩, iOS ATT 권한 획득
        // (광고 로딩이 오래 걸리기 때문에 아래 광고 시작 이벤트 대화보다 빨리 시작해두는 게 좋다.)
        if (BlackContext.Instance.LastClearedStageId >= 3)
        {
#if GOOGLE_MOBILE_ADS
            PlatformAdMobAdsInit.Instance.Init();
#endif
        }
        
        // 신규 유저 어서오고
        if (BlackContext.Instance.LastClearedStageId == 0 && BlackContext.Instance.LastClearedStageIdEvent < 0)
        {
            await StartWelcomeDialogInternalAsync();
        }

        // 3번째 스테이지까지 하면 그 다음부터 광고 봐야 하고
        if (BlackContext.Instance.LastClearedStageId == 3 && BlackContext.Instance.LastClearedStageIdEvent < 3)
        {
            await StartFairyDialogInternalAsync();
        }

        BlackContext.Instance.LastClearedStageIdEvent = BlackContext.Instance.LastClearedStageId;
    }

    public async void StartFairyDialogAsync() => await StartFairyDialogInternalAsync();

    static async Task StartWelcomeDialogInternalAsync()
    {
        var talkList = new[]
        {
            "부모님의 원수...\n\n아니 원수 같은 부모님이 다 쓰러져가는 미술관을 물려줬다.",
            "미술관을 물려준다길래 다니던 회사에 호기롭게 사표도 던졌건만,",
            "과장님께 다시 연락해 봐도 받지 않는다.",
            "이제 더는 물러설 곳이 없다. 어떻게든 이 미술관을 살려야 한다.",
        };

        var i = 0;
        foreach (var talk in talkList)
        {
            var tsc = new TaskCompletionSource<bool>();
            var notLast = i < talkList.Length - 1;
            ConfirmPopup.Instance.OpenYesImagePopup($"당신의 스토리 ({talkList.Length} 중 {i + 1})", null, talk,
                notLast ? "다음" : "시작하기",
                () =>
                {
                    Sound.Instance.PlayButtonClick();

                    if (notLast == false)
                    {
                        Sound.Instance.PlayInception();
                    }

                    tsc.SetResult(true);
                });
            await tsc.Task;
            i++;
        }

        ConfirmPopup.Instance.Close();
    }

    async Task StartFairyDialogInternalAsync()
    {
        subcanvas.Open();
        talkTypewriter.ClearText();

        dialogContentAnimator.SetTrigger(Appear);
        Sound.Instance.PlayPopup();

        await Task.Delay(1000);

        var talkList = new[]
        {
            "미술관 관장님,\n저는 체커의 요정이에요.",
            "어쩌면 아마도 앞으로는...\n광고가 나올지도 몰라요.",
            "대신 제가 게임을 좀 더\n재미있게 만들어 볼게요.",
            "감사한 관장님,\n앞으로도 즐겨주세요!"
        };

        foreach (var talk in talkList)
        {
            var tsc = new TaskCompletionSource<bool>();
            talkTypewriter.StartType(false, talk, () => { tsc.SetResult(true); });
            await tsc.Task;
            tryNextTsc = new TaskCompletionSource<bool>();
            await tryNextTsc.Task;
        }

        dialogContentAnimator.SetTrigger(Disappear);
        Sound.Instance.PlayPopup();

        await Task.Delay(1000);

        Sound.Instance.PlayJingleAchievement();

        ConfirmPopup.Instance.OpenYesImagePopup("새 기능", checkHintFeatureGuideSprite,
            "축하합니다!\n이제부터 색칠할 곳이 격자 무늬로 강조됩니다.", "좋았어!", ConfirmPopup.Instance.Close);

        subcanvas.Close();
    }

    public void TryNextTalk()
    {
        tryNextTsc?.TrySetResult(true);
    }

    [UsedImplicitly]
    void OpenPopup()
    {
    }

    [UsedImplicitly]
    void ClosePopup()
    {
    }
}