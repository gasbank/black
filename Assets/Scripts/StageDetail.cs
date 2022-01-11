using System.Threading.Tasks;
using ConditionalDebug;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageDetail : MonoBehaviour
{
    public static StageDetail instance;

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

    public static bool IsAllCleared => BlackContext.instance.LastClearedStageId >= Data.dataSet.StageSequenceData.Count;

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

    public async void OpenPopupAfterLoadingAsync()
    {
        var lastClearedStageId = BlackContext.instance.LastClearedStageId;
        
        ConDebug.Log($"Last Cleared Stage ID: {lastClearedStageId}");

        if (lastClearedStageId < 0) lastClearedStageId = 0;

        if (IsAllCleared)
        {
            //Debug.LogError("lastClearedStageId exceeds Data.dataSet.StageMetadataList count.");
            ConfirmPopup.instance.Open(@"\모든 스테이지를 깼습니다!\n진정한 미술관 재건이 시작되는 다음 업데이트를 기대 해 주세요!".Localized(), ConfirmPopup.instance.Close);
            return;
        }
        
        stageProgress.ProgressInt = lastClearedStageId % 5;

        ProgressMessage.instance.Open(@"\그림을 준비하는 중...".Localized());

        // 마지막 클리어한 ID는 1-based이고, 아래 함수는 0-based로 작동하므로
        // 다음으로 플레이할 스테이지를 가져올 때는 그대로 ID를 넘기면 된다.
        var stageMetadata = await LoadStageMetadataByZeroBasedIndexAsync(lastClearedStageId);

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

        ProgressMessage.instance.Close();
        var resumed = stageButton.SetStageMetadata(stageMetadata);
        subcanvas.Open();

        // 하다가 만 스테이지면 대기 시간 있어선 안된다.
        // 초반 스테이지는 대기 시간 없다.
        if (resumed || stageMetadata.StageSequenceData.skipLock)
        {
            stageLocker.Unlock();
        }

        if (easelExclamationMark != null)
        {
            easelExclamationMark.SetActive(false);
        }

        if (bottomTip != null)
        {
            if (BlackContext.instance.LastClearedStageId == 0)
            {
                bottomTip.SetMessage("\\시작하기를 눌러 색칠을 시작하세요~!".Localized());
                bottomTip.OpenSubcanvas();
            }
            else if (BlackContext.instance.LastClearedStageId == 1)
            {
                bottomTip.SetMessage("\\잘 했어요! 다음 스테이지도 어서어서 색칠합시다.".Localized());
                bottomTip.OpenSubcanvas();
            }
            else if (BlackContext.instance.LastClearedStageId == 4)
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

        if (BlackContext.instance.LastClearedStageId == 0)
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
        Sound.instance.PlayButtonClick();

        if (stageLocker.Locked == false
#if BLACK_ADMIN
            || Application.isEditor && Keyboard.current[Key.LeftShift].isPressed
#endif
        )
        {
            stageButton.SetStageMetadataToCurrent();
            SaveLoadManager.Save(BlackContext.instance, ConfigPopup.instance, Sound.instance, Data.instance, null);
            SceneManager.LoadScene("Main");
        }
        else
        {
            var adContext = new BlackAdContext(stageLocker.Unlock);
            PlatformAdMobAds.instance.TryShowRewardedAd(adContext);
        }
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