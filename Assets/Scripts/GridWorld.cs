using System;
using System.Collections;
using System.Collections.Generic;
using ConditionalDebug;
using Dirichlet.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GridWorld : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    static bool Verbose => false;

    readonly HashSet<uint> coloredMinPoints = new HashSet<uint>();

    [SerializeField]
    GameObject animatedCoinPrefab;

    [SerializeField]
    int coin;

    [SerializeField]
    RectTransform coinIconRt;

    [SerializeField]
    TextMeshProUGUI coinText;

    public Dictionary<uint, int> coloredIslandCountByColor = new Dictionary<uint, int>();

    [SerializeField]
    PlayableDirector finaleDirector;

    [SerializeField]
    public ComboEffect comboEffector;

    [SerializeField]
    ScInt gold = 0;

    [SerializeField]
    IslandLabelSpawner islandLabelSpawner;

    [SerializeField]
    PaletteButtonGroup paletteButtonGroup;

    Canvas rootCanvas;

    [SerializeField]
    RectTransform rt;

    StageData stageData;

    [SerializeField]
    StageSaveManager stageSaveManager;

    [SerializeField]
    Texture2D tex;

    [SerializeField]
    MainGame mainGame;

    [SerializeField]
    IslandShader3DController islandShader3DController;
    
    public Texture2D Tex => tex;

    public int TexSize => Tex.width;

    public string StageName { get; set; } = "TestStage";

    public int Coin
    {
        get => coin;
        set
        {
            coin = value;
            coinText.text = coin.ToString();
        }
    }

    public int Gold => gold;

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.dragging == false)
        {
            if (Verbose) ConDebug.Log($"World position 1 = {eventData.pointerCurrentRaycast.worldPosition}");
            transform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition);
            if (Verbose) ConDebug.Log($"World position 2 = {eventData.pointerPressRaycast.worldPosition}");

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, Camera.main,
                out var localPoint))
            {
                // 유저가 선택한 팔레트 색상
                var currentColorUint = paletteButtonGroup.CurrentPaletteColorUint;

                var a1Tex = mainGame.StageMetadata.A1Tex;
                var a2Tex = mainGame.StageMetadata.A2Tex;
                
                ConvertLocalPointToIxy(localPoint, out var ix, out var iy);
                var a1Float = a1Tex.GetPixel(ix, iy);
                var a2Float = a2Tex.GetPixel(ix, iy);
                if (Verbose)
                {
                    ConDebug.Log($"Local Point: {localPoint}, IXY: ({ix},{iy}), A1f={a1Float}, A2f={a2Float}");
                }

                var a1 = (int) (a1Float.a * 255);
                var a2 = (int) (a2Float.a * 255);
                var paletteIndex = a1 & ((1 << 6) - 1);
                var islandIndex = ((a1 >> 6) & 0x3) | (a2 << 2);
                
                var fillResult = FillResult.NotDetermined;
                
                if (currentColorUint == PaletteButtonGroup.InvalidPaletteColor)
                {
                    fillResult = FillResult.NoPaletteSelected;
                }
                else if (islandIndex == 0 && paletteIndex == 0)
                {
                    fillResult = FillResult.Outline;
                }
                else
                {
                    if (islandIndex <= 0 || islandIndex >= 1 + stageData.CachedIslandDataList.Count)
                    {
                        Debug.LogError("Out of range island index. [LOGIC ERROR]");
                        fillResult = FillResult.OutsideOfCanvas;
                    }

                    if (paletteIndex <= 0 || paletteIndex >= 1 + stageData.CachedPaletteArray.Length)
                    {
                        Debug.LogError("Out of range island index. [LOGIC ERROR]");
                        fillResult = FillResult.OutsideOfCanvas;
                    }
                }

                if (fillResult == FillResult.NotDetermined)
                {
                    if (islandIndex > 0 && paletteIndex > 0)
                    {
                        // 유저가 선택한 칸의 정답 색상
                        var solutionColorUint = stageData.CachedIslandDataList[islandIndex - 1].IslandData.rgba;
                        
                        // 유저가 선택한 칸의 Min Point
                        var fillMinPointUint = stageData.CachedIslandDataList[islandIndex - 1].MinPoint;
                        
                        if (solutionColorUint == currentColorUint)
                        {
                            if (islandLabelSpawner.ContainsMinPoint(fillMinPointUint))
                            {
                                fillResult = FillResult.Good;

                                // 실제로 채우기 렌더링한다.
                                islandShader3DController.SetIslandIndex(islandIndex);

                                UpdatePaletteBySolutionColor(fillMinPointUint, solutionColorUint, false);
                            }
                            else
                            {
                                // 이미 올바르게 칠해진 칸이다.
                                fillResult = FillResult.AlreadyFilled;
                            }
                        }
                        else
                        {
                            fillResult = FillResult.WrongColor;
                        }
                    }
                    else
                    {
                        fillResult = FillResult.Outline;
                    }
                }

                if (fillResult == FillResult.Good)
                {
                    // 특별 코인 획득 연출 - 아직 완성되지 않은 기능이므로 런칭스펙에서는 빼자.
                    //StartAnimateFillCoin(localPoint);

                    Sound.instance.PlayFillOkay();

                    BlackContext.instance.StageCombo++;
                    comboEffector.Play(BlackContext.instance.StageCombo);

                    // 이번에 칠한 칸이 마지막 칸인가? (모두 칠했는가?)
                    if (IsLabelByMinPointEmpty
#if BLACK_ADMIN
                        || (Application.isEditor && Keyboard.current[Key.LeftShift].isPressed)
#endif
                    )
                    {
                        StartFinale();
                    }
                }
                else
                {
                    // 잘못된 팔레트 버튼으로 칠하려고 했을 때만 오류로 표시
                    if (fillResult == FillResult.WrongColor)
                    {
                        // TODO: 아래 메시지 번역키로 대응 필요
                        ToastMessageEx.instance.PlayWarnAnim("색을 다시 확인해주세요");
                        
                        if (!BlackContext.instance.ComboAdminMode)
                        {
                            BlackContext.instance.StageCombo = 0;
                        }
                    }
                }

                if (Verbose) ConDebug.Log($"Local position = {localPoint}");
            }
        }
    }

    bool IsLabelByMinPointEmpty => islandLabelSpawner.IsLabelByMinPointEmpty;

    void StartFinale()
    {
        mainGame.DeactivateTime();
        finaleDirector.Play(finaleDirector.playableAsset);
        UpdateLastClearedStageIdAndSave();
    }

    void UpdateLastClearedStageIdAndSave()
    {
        if (!StageButton.CurrentStageMetadata)
        {
            if (Verbose)
                ConDebug.Log(
                    "Current stage metadata is not set. Last cleared stage ID will not be updated. (Did you start the play mode from Main scene?)");
            return;
        }

        var stageName = StageButton.CurrentStageMetadata.name;
        for (var i = 0; i < Data.dataSet.StageSequenceData.Count; i++)
        {
            if (Data.dataSet.StageSequenceData[i].stageName == stageName)
            {
                var oldClearedStageId = BlackContext.instance.LastClearedStageId;
                var newClearedStageId = i + 1;

                BlackContext.instance.LastClearedStageId = Mathf.Max(oldClearedStageId, newClearedStageId);

                // 스테이지 클리어에 진전이 있었다. 보상을 준다.
                if (newClearedStageId > oldClearedStageId)
                {
                    // 관문 스테이지는 추가 골드를 더 준다.
                    RewardGoldAmount = new UInt128(newClearedStageId % 5 == 0 ? 3 : 1);
                    BlackContext.instance.AddPendingGold(RewardGoldAmount);

                    BlackContext.instance.AchievementGathered.MaxBlackLevel =
                        (UInt128) BlackContext.instance.LastClearedStageId.ToInt();
                }
            }
        }

        var combo = (UInt128) BlackContext.instance.StageCombo.ToInt();
        if (BlackContext.instance.AchievementGathered.MaxColoringCombo < combo)
        {
            BlackContext.instance.AchievementGathered.MaxColoringCombo = combo;
        }

        SaveLoadManager.Save(BlackContext.instance, ConfigPopup.instance, Sound.instance, Data.instance, null);
    }

    public UInt128 RewardGoldAmount { get; private set; }

#if UNITY_EDITOR
    void OnValidate()
    {
        rt = GetComponent<RectTransform>();
    }
#endif

    void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        Coin = 0;
    }

    public void LoadTexture(Texture2D inputTexture, StageData inStageData)
    {
        tex = inputTexture;
        stageData = inStageData;
    }

    // 팔레트 정보 채워지고 난 뒤에 진행 상황 불러와야
    // 제대로 된 색깔로 채울 수 있다.
    internal void ResumeGame()
    {
        try
        {
            var stageSaveData = StageSaveManager.Load(StageName) ?? stageSaveManager.CreateStageSaveData(StageName);
            
            // 이미 모두 다 칠한 상태인데, 재플레이 기능으로 들어온거라면 복원할 필요 없다. 
            if (stageData.islandDataByMinPoint.Count <= stageSaveData.coloredMinPoints.Count
                && StageButton.CurrentStageMetadataReplay)
            {
                stageSaveData = stageSaveManager.CreateStageSaveData(StageName);
            }
            
            stageSaveManager.RestoreCameraState(stageSaveData);
            RestorePaletteAndFillState(stageSaveData.coloredMinPoints);
            mainGame.SetRemainTime(stageSaveData.remainTime);
            if (IsLabelByMinPointEmpty)
            {
                StartFinale();
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            DeleteSaveFileAndReloadScene();
        }
    }

    public void DeleteSaveFileAndReloadScene()
    {
        DeleteSaveFile();
        SceneManager.LoadScene("Main");
    }

    public void DeleteSaveFile()
    {
        StageSaveManager.DeleteSaveFile(StageName);
    }
    
    void UpdatePaletteBySolutionColor(uint fillMinPointUint, uint solutionColorUint, bool batch)
    {
        islandLabelSpawner.DestroyLabelByMinPoint(fillMinPointUint);

        coloredMinPoints.Add(fillMinPointUint);

        if (coloredIslandCountByColor.TryGetValue(solutionColorUint, out var coloredIslandCount))
            coloredIslandCount++;
        else
            coloredIslandCount = 1;

        coloredIslandCountByColor[solutionColorUint] = coloredIslandCount;
        paletteButtonGroup.UpdateColoredCount(solutionColorUint, coloredIslandCount, batch);
    }

    enum FillResult
    {
        NotDetermined,
        Good,
        Outline,
        WrongColor,
        AlreadyFilled,
        NoPaletteSelected,
        OutsideOfCanvas,
    }

    void ConvertLocalPointToIxy(Vector2 localPoint, out int ix, out int iy)
    {
        var rect = rt.rect;
        var w = rect.width;
        var h = rect.height;

        ix = (int) ((localPoint.x + w / 2) / w * Tex.width);
        iy = (int) ((localPoint.y + h / 2) / h * Tex.height);

        if (Verbose) ConDebug.Log($"w={w} / h={h}");
    }

    void RestorePaletteAndFillState(HashSet<uint> inColoredMinPoints)
    {
        if (Verbose)
        {
            ConDebug.Log($"Starting batch fill of {inColoredMinPoints.Count} points");
        }

        if (inColoredMinPoints.Count <= 0) return;
        
        foreach (var minPoint in inColoredMinPoints)
        {
            if (stageData.islandDataByMinPoint.TryGetValue(minPoint, out var islandData))
            {
                islandShader3DController.EnqueueIslandIndex(islandData.index);
                UpdatePaletteBySolutionColor(minPoint, islandData.rgba, true);
            }
            else
            {
                Debug.LogError($"Island data (min point = {minPoint} cannot be found in StageData.");
            }
        }
    }

    void StartAnimateFillCoin(Vector2 localPoint)
    {
        var animatedCoin = Instantiate(animatedCoinPrefab, transform).GetComponent<AnimatedCoin>();
        animatedCoin.Rt.anchoredPosition = localPoint;
        animatedCoin.TargetRt = coinIconRt;
        var animatedCoinTransform = animatedCoin.transform;
        animatedCoinTransform.SetParent(rootCanvas.transform, true);
        animatedCoinTransform.localScale = Vector3.one;
        animatedCoin.GridWorld = this;
        gold++;
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            WriteStageSaveData();
        }
    }

    public void WriteStageSaveData()
    {
        // 스테이지 별 진행 상황 데이터
        stageSaveManager.Save(StageName, coloredMinPoints, mainGame.GetRemainTime());

        // 전체 저장 데이터
        SaveLoadManager.Save(BlackContext.instance, ConfigPopup.instance, Sound.instance, Data.instance, null);
    }

    void OnApplicationQuit()
    {
        WriteStageSaveData();
    }
}