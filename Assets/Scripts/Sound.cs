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

    public static Sound instance;

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

    static bool Verbose => false;

    public bool BgmAudioSourceActive
    {
        get => bgmAudioSource.enabled;
        set => bgmAudioSource.enabled = value;
    }

    public bool SfxAudioSourceActive
    {
        get => sfxAudioSource.enabled;
        set => sfxAudioSource.enabled = value;
    }

    public bool GatherStoredMaxSfxEnabled { get; set; }

    //슬라이더 값 간접 참조 (SaveFile IO)
    public float BgmAudioSourceVolume
    {
        get => instance.bgmAudioSource.volume;
        set => instance.bgmAudioSource.volume = value;
    }

    public float SfxAudioSourceVolume
    {
        get => instance.sfxAudioSource.volume;
        set => instance.sfxAudioSource.volume = value;
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
        if (BlackContext.instance != null && BlackContext.instance.LoadedAtLeastOnce == false) return;

        if (SfxAudioSourceActive) instance.sfxAudioSource.PlayOneShot(instance.buttonClick);
    }

    public void PlayBlackNew()
    {
        if (SfxAudioSourceActive) //ConDebug.Log("SpawnNewBlack PlayBlackNew");
            instance.sfxAudioSource.PlayOneShot(instance.blackNew);
    }

    public void PlayWipeStain()
    {
        if (SfxAudioSourceActive) instance.sfxAudioSource.PlayOneShot(instance.wipeStain);
    }

    public void PlayWipeStainFinish()
    {
        if (SfxAudioSourceActive) instance.sfxAudioSource.PlayOneShot(instance.wipeStainFinish);
    }

    public void PlayTadaA()
    {
        if (SfxAudioSourceActive) instance.sfxAudioSource.PlayOneShot(instance.tadaA);
    }

    public void PlayTadaF()
    {
        if (SfxAudioSourceActive) instance.sfxAudioSource.PlayOneShot(instance.tadaF);
    }

    public void PlayTadaG()
    {
        if (SfxAudioSourceActive) instance.sfxAudioSource.PlayOneShot(instance.tadaG);
    }

    public void PlaySoftTada()
    {
        if (SfxAudioSourceActive) instance.sfxAudioSource.PlayOneShot(instance.softTada);
    }

    public void PlayError()
    {
        if (SfxAudioSourceActive) instance.sfxAudioSource.PlayOneShot(instance.error);
    }

    public void PlaySnap()
    {
        if (SfxAudioSourceActive) instance.sfxAudioSource.PlayOneShot(instance.snap);
    }

    public void PlayRubberImpact()
    {
        if (SfxAudioSourceActive) instance.sfxAudioSource.PlayOneShot(instance.rubberImpact);
    }

    public void PlayCorrectlyFinished()
    {
        if (SfxAudioSourceActive) instance.sfxAudioSource.PlayOneShot(instance.correctlyFinished);
    }

    public void PlayCorrectlyFinishedMild()
    {
        if (SfxAudioSourceActive) instance.sfxAudioSource.PlayOneShot(instance.correctlyFinishedMild);
    }

    public void PlayWhooshAir()
    {
        if (SfxAudioSourceActive) instance.sfxAudioSource.PlayOneShot(instance.whooshAir);
    }

    public void PlayJingleAchievement()
    {
        if (SfxAudioSourceActive) instance.sfxAudioSource.PlayOneShot(instance.jingleAchievement);
    }

    public void PlayErrorBuzzer()
    {
        if (SfxAudioSourceActive) instance.sfxAudioSource.PlayOneShot(instance.errorBuzzer);
    }

    public void PlayGatherStoredMax()
    {
        if (GatherStoredMaxSfxEnabled) instance.gatherStoredMaxSfxAudioSource.PlayOneShot(instance.gatherStoredMax);
    }

    public void PlayLongTada()
    {
        if (SfxAudioSourceActive) instance.sfxAudioSource.PlayOneShot(instance.longTada);
    }

    public void PlayDingaling()
    {
        if (SfxAudioSourceActive) instance.sfxAudioSource.PlayOneShot(instance.dingaling);
    }

    public void PlayWhacACatBgm()
    {
        if (BgmAudioSourceActive)
        {
            instance.bgmAudioSource.Stop();
            instance.bgmAudioSource.PlayOneShot(instance.whacACatBgm);
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
            instance.bgmAudioSource.Stop();
            instance.bgmAudioSource.PlayOneShot(instance.feverBgm);
            CurrentBgmType = BgmType.Fever;
        }
    }

    public void StopCurrentBgm()
    {
        instance.bgmAudioSource.Stop();
    }


    public void StopTimeAndMuteAudioMixer()
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (Time.timeScale != 1) Debug.LogError("Time.timeScale expected to be 1 at this moment!");

        Time.timeScale = 0;
        audioMixer.SetFloat("MasterVolume", -80.0f);
    }

    public void ResumeToNormalTimeAndResumeAudioMixer()
    {
        Time.timeScale = 1;
        audioMixer.SetFloat("MasterVolume", 0.0f);
    }

    public void EnableBgmVolume(bool b)
    {
        ConDebug.Log($"EnableBgmVolume {b}");
        audioMixer.SetFloat("BgmVolume", b ? 20f * Mathf.Log10(BgmAudioVolume) : -80.0f);
    }

    public void EnableSfxVolume(bool b)
    {
        ConDebug.Log($"EnableSfxVolume {b}");
        audioMixer.SetFloat("SfxVolume", b ? 20f * Mathf.Log10(SfxAudioVolume) : -80.0f);
    }

    public void PlayFillOkay()
    {
        if (Verbose) ConDebug.Log(nameof(PlayFillOkay));

        // 게임 처음 로딩 중에 들어오는 건 재생하지 말자
        if (BlackContext.instance != null && BlackContext.instance.LoadedAtLeastOnce == false) return;

        if (SfxAudioSourceActive) instance.sfxAudioSource.PlayOneShot(instance.fillOkay);
    }
    
    public void PlayFillError()
    {
        if (Verbose) ConDebug.Log(nameof(PlayFillError));

        // 게임 처음 로딩 중에 들어오는 건 재생하지 말자
        if (BlackContext.instance != null && BlackContext.instance.LoadedAtLeastOnce == false) return;

        if (SfxAudioSourceActive) instance.sfxAudioSource.PlayOneShot(instance.fillError);
    }
}