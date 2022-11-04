using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
#if ADDRESSABLES
using UnityEngine.AddressableAssets;
#endif
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageDetailPopup : MonoBehaviour
{
    // 본 컴포넌트는 리플레이용과 새 스테이지 언락용 별개로 쓰고 있으므로 싱글턴 패턴 쓰지 않는다.
    //public static StageDetailPopup instance;

    [SerializeField]
    StageButton stageButton;

    [SerializeField]
    Subcanvas subcanvas;

    [SerializeField]
    StageProgress stageProgress;

    [SerializeField]
    GameObject easelExclamationMark;

    [SerializeField]
    BottomTip bottomTip;

    [SerializeField]
    StageLocker stageLocker;

    [SerializeField]
    Text startStageButtonText;

    [SerializeField]
    IslandShader3DController islandShader3DController;

    [SerializeField]
    bool replay;

    public static bool IsAllCleared => BlackContext.Instance.LastClearedStageId >= Data.dataSet.StageSequenceData.Count;

    public float StageLockDetailTime
    {
        get => stageLocker.RemainTime;
        set => stageLocker.RemainTime = value;
    }

    void Start()
    {
        SetInitialBottomTip();
    }

    void OnEnable()
    {
        stageLocker.OnStageLocked += OnStageLocked;
        stageLocker.OnStageUnlocked += OnStageUnlocked;
    }

    void OnDisable()
    {
        stageLocker.OnStageLocked -= OnStageLocked;
        stageLocker.OnStageUnlocked -= OnStageUnlocked;
    }

    // stageId가 아니라 stageIndex로 받는 점을 유의한다.
    // lastClearedStageId를 인자로 넣으면 다음으로 플레이 할 판으로 설정된다.
    public async Task OpenPopupAfterLoadingAsync(int stageIndex)
    {
        if (stageIndex < 0) stageIndex = 0;

        // 한번 깼던 판 다시 꺠려고 하는 것인가?
        replay = stageIndex + 1 <= BlackContext.Instance.LastClearedStageId;

        if (IsAllCleared && replay == false)
        {
            //Debug.LogError("lastClearedStageId exceeds Data.dataSet.StageMetadataList count.");
            ConfirmPopup.Instance.Open(@"\모든 스테이지를 깼습니다!\n진정한 미술관 재건이 시작되는 다음 업데이트를 기대 해 주세요!".Localized(),
                ConfirmPopup.Instance.Close);
            return;
        }

        stageProgress.ProgressInt = stageIndex % 5;

        ProgressMessage.Instance.Open(@"\그림을 준비하는 중...".Localized());

        // 마지막 클리어한 ID는 1-based이고, 아래 함수는 0-based로 작동하므로
        // 다음으로 플레이할 스테이지를 가져올 때는 그대로 ID를 넘기면 된다.
        var stageMetadata = await LoadStageMetadataByZeroBasedIndexAsync(stageIndex);

        if (stageMetadata == null)
        {
            // 중대 문제
            Debug.LogError("Stage metadata is null");
            return;
        }

        islandShader3DController.Initialize(stageMetadata);

        var stageSaveData = StageSaveManager.Load(stageMetadata.name);
        if (stageSaveData != null)
        {
            foreach (var minPoint in stageSaveData.coloredMinPoints)
            {
                if (stageMetadata.StageData.islandDataByMinPoint.TryGetValue(minPoint, out var islandData))
                {
                    islandShader3DController.EnqueueIslandIndex(islandData.index);
                }
                else
                {
                    Debug.LogError($"Island data (min point = {minPoint} cannot be found in StageData.");
                }
            }
        }

        ProgressMessage.Instance.Close();
        var resumed = stageButton.SetStageMetadata(stageMetadata);
        subcanvas.Open();

        // 하다가 만 스테이지면 대기 시간 있어선 안된다.
        // 초반 스테이지는 대기 시간 없다.
        if (replay || resumed || stageMetadata.StageSequenceData.skipLock)
        {
            stageLocker.Unlock();
        }
        else
        {
            stageLocker.Lock();
        }

        stageProgress.Show(replay == false);

        if (easelExclamationMark != null)
        {
            easelExclamationMark.SetActive(false);
        }

        if (bottomTip != null)
        {
            if (BlackContext.Instance.LastClearedStageId == 0)
            {
                bottomTip.SetMessage("\\시작하기를 눌러 색칠을 시작하세요~!".Localized());
                bottomTip.OpenSubcanvas();
            }
            else if (BlackContext.Instance.LastClearedStageId == 1)
            {
                bottomTip.SetMessage("\\잘 했어요! 다음 스테이지도 어서어서 색칠합시다.".Localized());
                bottomTip.OpenSubcanvas();
            }
            else if (BlackContext.Instance.LastClearedStageId == 4)
            {
                bottomTip.SetMessage("\\이번 스테이지는 시간제한이 있는 '관문 스테이지'예요! 파이팅!!!".Localized());
                bottomTip.OpenSubcanvas();
            }
        }
    }

    public static async Task<StageMetadata> LoadStageMetadataByZeroBasedIndexAsync(ScInt zeroBasedIndex)
    {
        if (zeroBasedIndex < 0 || zeroBasedIndex >= Data.dataSet.StageMetadataLocList.Count)
        {
            Debug.LogError($"Stage index {zeroBasedIndex} (zero-based) is out of range");
            return null;
        }
        
        var stageMetadataLoc = Data.dataSet.StageMetadataLocList[zeroBasedIndex];
        if (stageMetadataLoc == null)
        {
            Debug.LogError($"Stage metadata at index {zeroBasedIndex} (zero-based) is null");
            return null;
        }

        var stageMetadata = await Addressables.LoadAssetAsync<StageMetadata>(stageMetadataLoc).Task;
        if (stageMetadata == null)
        {
            Debug.LogError($"Stage metadata with zero based index {zeroBasedIndex} is null");
            return null;
        }

        stageMetadata.StageIndex = zeroBasedIndex;
        return stageMetadata;
    }

    [UsedImplicitly]
    public void OpenPopup()
    {
    }

    [UsedImplicitly]
    void ClosePopup()
    {
        if (easelExclamationMark != null)
        {
            easelExclamationMark.SetActive(IsAllCleared == false);
        }

        SetInitialBottomTip();
    }

    void SetInitialBottomTip()
    {
        if (bottomTip == null) return;

        if (BlackContext.Instance.LastClearedStageId == 0)
        {
            bottomTip.SetMessage("\\이젤을 터치해서 색칠할 그림을 확인해봐요.".Localized());
        }
        else
        {
            bottomTip.CloseSubcanvas();
        }
    }

    public void OnStageStartButton()
    {
        Sound.Instance.PlayButtonClick();

        if (stageLocker.Locked == false
#if DEV_BUILD
            || Application.isEditor && Keyboard.current[Key.LeftShift].isPressed
#endif
        )
        {
            if (replay)
            {
                var stageMetadata = stageButton.GetStageMetadata();
                var stageTitle = Data.dataSet.StageSequenceData[stageMetadata.StageIndex].title;

                ConfirmPopup.Instance.OpenYesNoPopup(
                    @"\'{0}' 스테이지를 시작할까요?\n\n설정 메뉴에서 언제든지 미술관으로 돌아올 수 있습니다.".Localized(stageTitle),
                    GoToMain, ConfirmPopup.Instance.Close);
            }
            else
            {
                GoToMain();
            }
        }
        else if (PlatformAdMobAds.Instance != null)
        {
            var adContext = new BlackAdContext(stageLocker.Unlock);
            PlatformAdMobAds.Instance.TryShowRewardedAd(adContext);
        }
        else
        {
            ConfirmPopup.Instance.OpenSimpleMessage(
                Application.isEditor
                    ? "Ad not supported on this platform. Click the button while Left Shift key."
                    : "Ad Mob instance not found.");
        }
    }

    void GoToMain()
    {
        stageButton.SetStageMetadataToCurrent(replay);
        SaveLoadManager.Save(BlackContext.Instance, ConfigPopup.Instance, Sound.Instance, Data.Instance,
            null);
        SceneManager.LoadScene("Main");
    }

    void OnStageUnlocked()
    {
        startStageButtonText.text = @"\바로 시작".Localized();
    }

    void OnStageLocked()
    {
        startStageButtonText.text = @"\광고 시청 후 바로 시작".Localized();
    }
}