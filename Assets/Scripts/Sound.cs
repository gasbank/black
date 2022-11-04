using System.Collections.Generic;
using ConditionalDebug;
using UnityEngine;
using UnityEngine.Audio;

[DisallowMultipleComponent]
public class Sound : MonoBehaviour
{
    public enum BgmType
    {
        Normal,
        Fever,
        WhacACat
    }

    const float MutedVolume = -80.0f;

    public static Sound Instance;

    [SerializeField]
    AudioMixer audioMixer;

    [SerializeField]
    AudioSource bgmAudioSource;

    [SerializeField]
    float bgmAudioVolume;

    [SerializeField]
    BgmType bgmType = BgmType.Normal;

    [SerializeField]
    AudioClip blackNew;

    [SerializeField]
    AudioClip buttonClick;
    
    [SerializeField]
    AudioClip fillOkay;
    
    [SerializeField]
    AudioClip fillError;

    [SerializeField]
    AudioClip correctlyFinished;

    [SerializeField]
    AudioClip correctlyFinishedMild;

    [SerializeField]
    int currentNormalBgmIndex;

    [SerializeField]
    AudioClip dingaling;

    [SerializeField]
    AudioClip error;

    [SerializeField]
    AudioClip errorBuzzer;

    [SerializeField]
    AudioClip feverBgm;

    [SerializeField]
    AudioClip gatherStoredMax;

    [SerializeField]
    AudioSource gatherStoredMaxSfxAudioSource;

    [SerializeField]
    AudioClip jingleAchievement;

    [SerializeField]
    AudioClip longTada;

    [SerializeField]
    List<AudioClip> normalBgms;

    [SerializeField]
    AudioClip rubberImpact;

    [SerializeField]
    AudioSource sfxAudioSource;

    [SerializeField]
    float sfxAudioVolume;

    [SerializeField]
    AudioClip snap;

    [SerializeField]
    AudioClip softTada;

    [SerializeField]
    AudioClip tadaA;

    [SerializeField]
    AudioClip tadaF;

    [SerializeField]
    AudioClip tadaG;

    [SerializeField]
    AudioClip whacACatBgm;

    [SerializeField]
    AudioClip whooshAir;

    [SerializeField]
    AudioClip wipeStain;

    [SerializeField]
    AudioClip wipeStainFinish;

    [SerializeField]
    AudioClip microTick;

    [SerializeField]
    AudioClip popup;

    [SerializeField]
    AudioClip inception;
    
    static bool Verbose => false;

    public bool BgmAudioSourceActive
    {
        get => audioMixer.GetFloat("BgmVolume", out var v) && v >= 0;
        set => audioMixer.SetFloat("BgmVolume", value ? 0 : MutedVolume);
    }

    public bool SfxAudioSourceActive
    {
        get => audioMixer.GetFloat("SfxVolume", out var v) && v >= 0;
        set => audioMixer.SetFloat("SfxVolume", value ? 0 : MutedVolume);
    }

    public bool GatherStoredMaxSfxEnabled { get; set; }

    //슬라이더 값 간접 참조 (SaveFile IO)
    public float BgmAudioSourceVolume
    {
        get => Instance.bgmAudioSource.volume;
        set => Instance.bgmAudioSource.volume = value;
    }

    public float SfxAudioSourceVolume
    {
        get => Instance.sfxAudioSource.volume;
        set => Instance.sfxAudioSource.volume = value;
    }

    public float BgmAudioVolume
    {
        get => bgmAudioVolume;
        set => bgmAudioVolume = value;
    }

    public float SfxAudioVolume
    {
        get => sfxAudioVolume;
        set => sfxAudioVolume = value;
    }

    public BgmType CurrentBgmType
    {
        get => bgmType;
        private set => bgmType = value;
    }

    public int CurrentNormalBgmIdx
    {
        set
        {
            currentNormalBgmIndex = value;
            bgmAudioSource.clip = CurrentNormalBgm;
            if (CurrentBgmType == BgmType.Normal) PlayNormalBgm();
        }
    }

    AudioClip CurrentNormalBgm
    {
        get
        {
            if (currentNormalBgmIndex < normalBgms.Count) return normalBgms[currentNormalBgmIndex];

            return null;
        }
    }

    public void PlayButtonClick()
    {
        if (Verbose) ConDebug.Log(nameof(PlayButtonClick));

        // 게임 처음 로딩 중에 들어오는 건 재생하지 말자
        if (BlackContext.Instance != null && BlackContext.Instance.LoadedAtLeastOnce == false) return;

        if (SfxAudioSourceActive) Instance.sfxAudioSource.PlayOneShot(Instance.buttonClick);
    }

    public void PlayBlackNew()
    {
        if (SfxAudioSourceActive) //ConDebug.Log("SpawnNewBlack PlayBlackNew");
            Instance.sfxAudioSource.PlayOneShot(Instance.blackNew);
    }

    public void PlayWipeStain()
    {
        if (SfxAudioSourceActive) Instance.sfxAudioSource.PlayOneShot(Instance.wipeStain);
    }

    public void PlayWipeStainFinish()
    {
        if (SfxAudioSourceActive) Instance.sfxAudioSource.PlayOneShot(Instance.wipeStainFinish);
    }

    public void PlayTadaA()
    {
        if (SfxAudioSourceActive) Instance.sfxAudioSource.PlayOneShot(Instance.tadaA);
    }

    public void PlayTadaF()
    {
        if (SfxAudioSourceActive) Instance.sfxAudioSource.PlayOneShot(Instance.tadaF);
    }

    public void PlayTadaG()
    {
        if (SfxAudioSourceActive) Instance.sfxAudioSource.PlayOneShot(Instance.tadaG);
    }

    public void PlaySoftTada()
    {
        if (SfxAudioSourceActive) Instance.sfxAudioSource.PlayOneShot(Instance.softTada);
    }

    public void PlayError()
    {
        if (SfxAudioSourceActive) Instance.sfxAudioSource.PlayOneShot(Instance.error);
    }

    public void PlaySnap()
    {
        if (SfxAudioSourceActive) Instance.sfxAudioSource.PlayOneShot(Instance.snap);
    }

    public void PlayRubberImpact()
    {
        if (SfxAudioSourceActive) Instance.sfxAudioSource.PlayOneShot(Instance.rubberImpact);
    }

    public void PlayCorrectlyFinished()
    {
        if (SfxAudioSourceActive) Instance.sfxAudioSource.PlayOneShot(Instance.correctlyFinished);
    }

    public void PlayCorrectlyFinishedMild()
    {
        if (SfxAudioSourceActive) Instance.sfxAudioSource.PlayOneShot(Instance.correctlyFinishedMild);
    }

    public void PlayWhooshAir()
    {
        if (SfxAudioSourceActive) Instance.sfxAudioSource.PlayOneShot(Instance.whooshAir);
    }

    public void PlayJingleAchievement()
    {
        if (SfxAudioSourceActive) Instance.sfxAudioSource.PlayOneShot(Instance.jingleAchievement);
    }

    public void PlayErrorBuzzer()
    {
        if (SfxAudioSourceActive) Instance.sfxAudioSource.PlayOneShot(Instance.errorBuzzer);
    }

    public void PlayGatherStoredMax()
    {
        if (GatherStoredMaxSfxEnabled) Instance.gatherStoredMaxSfxAudioSource.PlayOneShot(Instance.gatherStoredMax);
    }

    public void PlayLongTada()
    {
        if (SfxAudioSourceActive) Instance.sfxAudioSource.PlayOneShot(Instance.longTada);
    }

    public void PlayDingaling()
    {
        if (SfxAudioSourceActive) Instance.sfxAudioSource.PlayOneShot(Instance.dingaling);
    }

    public void PlayMicroTick()
    {
        if (SfxAudioSourceActive) Instance.sfxAudioSource.PlayOneShot(Instance.microTick);
    }
    
    public void PlayPopup()
    {
        if (SfxAudioSourceActive) Instance.sfxAudioSource.PlayOneShot(Instance.popup);
    }
    
    public void PlayInception()
    {
        if (SfxAudioSourceActive) Instance.sfxAudioSource.PlayOneShot(Instance.inception);
    }

    public void PlayWhacACatBgm()
    {
        if (BgmAudioSourceActive)
        {
            Instance.bgmAudioSource.Stop();
            Instance.bgmAudioSource.PlayOneShot(Instance.whacACatBgm);
            CurrentBgmType = BgmType.WhacACat;
        }
    }

    public void PlayNormalBgm()
    {
        if (BgmAudioSourceActive)
        {
            bgmAudioSource.enabled = false;
            // ReSharper disable once Unity.InefficientPropertyAccess
            bgmAudioSource.enabled = true;
            CurrentBgmType = BgmType.Normal;
        }
    }

    public void PlayFeverBgm()
    {
        if (BgmAudioSourceActive)
        {
            Instance.bgmAudioSource.Stop();
            Instance.bgmAudioSource.PlayOneShot(Instance.feverBgm);
            CurrentBgmType = BgmType.Fever;
        }
    }

    public void StopCurrentBgm()
    {
        Instance.bgmAudioSource.Stop();
    }


    public void StopTimeAndMuteAudioMixer()
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (Time.timeScale != 1) Debug.LogError("Time.timeScale expected to be 1 at this moment!");

        Time.timeScale = 0;
        audioMixer.SetFloat("MasterVolume", MutedVolume);
    }

    public void ResumeToNormalTimeAndResumeAudioMixer()
    {
        Time.timeScale = 1;
        audioMixer.SetFloat("MasterVolume", 0.0f);
    }

    public void EnableBgmVolume(bool b)
    {
        ConDebug.Log($"EnableBgmVolume {b}");
        UpdateBgmVolume(b);
    }

    void UpdateBgmVolume(bool b)
    {
        audioMixer.SetFloat("BgmVolume", b ? 20f * Mathf.Log10(BgmAudioVolume) : MutedVolume);
    }

    public void EnableSfxVolume(bool b)
    {
        ConDebug.Log($"EnableSfxVolume {b}");
        audioMixer.SetFloat("SfxVolume", b ? 20f * Mathf.Log10(SfxAudioVolume) : MutedVolume);
    }

    public void PlayFillOkay()
    {
        if (Verbose) ConDebug.Log(nameof(PlayFillOkay));

        // 게임 처음 로딩 중에 들어오는 건 재생하지 말자
        if (BlackContext.Instance != null && BlackContext.Instance.LoadedAtLeastOnce == false) return;

        if (SfxAudioSourceActive) Instance.sfxAudioSource.PlayOneShot(Instance.fillOkay);
    }
    
    public void PlayFillError()
    {
        if (Verbose) ConDebug.Log(nameof(PlayFillError));

        // 게임 처음 로딩 중에 들어오는 건 재생하지 말자
        if (BlackContext.Instance != null && BlackContext.Instance.LoadedAtLeastOnce == false) return;

        if (SfxAudioSourceActive) Instance.sfxAudioSource.PlayOneShot(Instance.fillError);
    }
}