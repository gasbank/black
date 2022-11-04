using System;
using System.Collections.Generic;
using ConditionalDebug;
using UnityEngine;

[DisallowMultipleComponent]
public class BackgroundTimeCompensator : MonoBehaviour, INetworkTimeSubscriber, IPlatformBackgroundTimeCompensator
{
    internal static BackgroundTimeCompensator Instance;

    readonly HashSet<MonoBehaviour> backgrounderSet = new HashSet<MonoBehaviour>();
    DateTime backgroundBeginNetworkTime = DateTime.MinValue;
    bool focusLost;
    int focusLostNetworkTimeRequestSequence;

    [SerializeField]
    NetworkTime networkTime;

    bool onApplicationPauseCalled;
    bool pendingBackgroundTimeCompensation;

    public void OnNetworkTimeStateChange(NetworkTime.QueryState state)
    {
        if (pendingBackgroundTimeCompensation)
        {
            ConDebug.Log($"Pending background time compensation starting now: networkTime.QueryState={state}");
            if (state == NetworkTime.QueryState.NoError)
            {
                pendingBackgroundTimeCompensation = false;
                ExecuteBackgroundTimeCompensation();
            }
            else if (state == NetworkTime.QueryState.Error)
            {
                pendingBackgroundTimeCompensation = false;
            }
        }
    }

    public void BeginBackgroundState(MonoBehaviour backgrounder)
    {
        var oldCount = backgrounderSet.Count;
        backgrounderSet.Add(backgrounder);
        if (oldCount == backgrounderSet.Count)
        {
            Debug.LogError($"Duplicated backgrounder added: {backgrounder.name}");
            return;
        }

        if (oldCount == 0 && backgrounderSet.Count != 0)
            OnBackgrounded(backgrounder);
        else
            ConDebug.Log($"Additional backgrounder registered: {backgrounder.name}");
    }

    public void EndBackgroundState(MonoBehaviour backgrounder)
    {
        var oldCount = backgrounderSet.Count;
        if (backgrounderSet.Remove(backgrounder) == false)
        {
            Debug.LogWarning($"Nonexistent backgrounder cannot be removed: {backgrounder.name}");
            return;
        }

        if (oldCount != 0 && backgrounderSet.Count == 0) OnForegrounded(backgrounder);
    }

    void OnApplicationPause(bool pause)
    {
        onApplicationPauseCalled = true;
    }

    void OnBackgrounded(MonoBehaviour backgrounder)
    {
        ConDebug.Log($"First backgrounder added. Starting background mode: {backgrounder.name}");

        onApplicationPauseCalled = false;

        Sound.Instance.StopTimeAndMuteAudioMixer();

        // 포커스 잃기 시작한 시간 기록
        // 네트워크 시각을 조회할 수 있을 때만 유효한 기록이다.
        if (networkTime.State == NetworkTime.QueryState.NoError)
        {
            focusLost = true;
            backgroundBeginNetworkTime = networkTime.EstimatedNetworkTime;
            focusLostNetworkTimeRequestSequence = networkTime.RequestSequence;
        }
        else
        {
            Debug.LogWarning(
                "First backgrounder added, however, we do not have any network time at this moment. Background time compensation will not working.");
        }
    }

    void OnForegrounded(MonoBehaviour backgrounder)
    {
        ConDebug.Log($"Last backgrounder removed. Finishing background mode: {backgrounder.name}");

        Sound.Instance.ResumeToNormalTimeAndResumeAudioMixer();

        // 잠깐 백그라운드에 갔다가 돌아오는 경우 지나간 시간만큼 보상을 해 준다. (게임을 켜 놓은 것 처럼)
        if (focusLost)
        {
            focusLost = false;
            ExecuteBackgroundTimeCompensationImmediatelyOrDelayed();
        }
    }

    void ExecuteBackgroundTimeCompensationImmediatelyOrDelayed()
    {
        // 네트워크 시각이 이 시점(프로그램이 백그라운드에서 막 되살아난 시점)에서 networkTime.OnApplicationPause()의 호출이 완료되어
        // 재조회가 완료되어 있으면 참 좋겠지만, 대부분 아닐 것이다.
        // 그렇다면 바로 백그라운드 시간 보상을 해 줄 수 있고, 아니라면 좀 나중에 해야 한다.
        if (focusLostNetworkTimeRequestSequence != networkTime.RequestSequence &&
            networkTime.State == NetworkTime.QueryState.NoError)
        {
            // 백그라운드 모드를 잘 다녀왔고, 다녀오고 재조회까지 신속히 정상 완료 됐다면 이 경우다.
            ExecuteBackgroundTimeCompensation(GetBackgroundDurationUsec());
        }
        else if (onApplicationPauseCalled == false && networkTime.State == NetworkTime.QueryState.NoError)
        {
            // <see cref="BackgroundTimeCompensator"/>가 인정할 때는 백그라운드 상태가 있었지만,
            // 실제로 네트워크 타임을 재조회할 필요가 없는 상태다. (왜냐면 <see cref="BackgroundTimeCompensator.OnApplicationPause(bool)"/>가 호출되지 않았기 때문에)
            // 이 경우도 실제로 흐른 시간 만큼 보상 해 주면 된다.
            ExecuteBackgroundTimeCompensation(GetBackgroundDurationUsec());
        }
        else
        {
            // 네트워크 시간 재조회가 필요하지만 아직은 안됐다.
            // 기다렸다 네트워크 시간 재조회가 되면 하자. 
            ConDebug.Log("Background time compensation delayed since renewed network time not received yet.");
            pendingBackgroundTimeCompensation = true;
        }
    }

    long GetBackgroundDurationUsec()
    {
        return (long) (GetBackgroundDuration() * 1000 * 1000);
    }

    double GetBackgroundDuration()
    {
        if (networkTime.State == NetworkTime.QueryState.NoError)
        {
            if (backgroundBeginNetworkTime != DateTime.MinValue)
            {
                var backgroundedDuration = (networkTime.EstimatedNetworkTime - backgroundBeginNetworkTime).TotalSeconds;
                if (backgroundedDuration > 0) return backgroundedDuration;

                Debug.LogError(
                    $"{nameof(GetBackgroundDuration)} returned {backgroundedDuration}! returning 0 instead...");
                return 0;
            }

            Debug.LogError(
                $"{nameof(GetBackgroundDuration)} should not be called when {nameof(backgroundBeginNetworkTime)} is unavailable.");
            return 0;
        }

        Debug.LogError(
            $"{nameof(GetBackgroundDurationUsec)} should not be called when {nameof(NetworkTime)} is unavailable.");
        return 0;
    }

    void ExecuteBackgroundTimeCompensation()
    {
        ConDebug.Log("Background time compensation started.");
        if (networkTime.State != NetworkTime.QueryState.NoError)
            Debug.LogError(
                "ExecuteBackgroundTimeCompensation() should be called after network time received successfully.");
        else
            ExecuteBackgroundTimeCompensation(GetBackgroundDurationUsec());
    }

    void OnEnable()
    {
        networkTime.Register(this);
    }

    void OnDisable()
    {
        networkTime.Unregister(this);
    }

    static void ExecuteBackgroundTimeCompensation(long usec)
    {
        ConDebug.Log($"ExecuteBackgroundTimeCompensation() {(float) usec / 1e6:f1} sec ({usec} usec) elapsed.");
        if (usec > 0)
        {
            if (BlackContext.Instance != null)
            {
                // TODO Background Compensation
            }
        }
        else
        {
            Debug.LogError($"ExecuteBackgroundTimeCompensation() with negative usec? {usec}");
        }
    }

    internal void AppendCompensationStartedFrom(DateTime beginNetworkTime)
    {
        if (beginNetworkTime == DateTime.MinValue)
        {
            ConDebug.Log("No needed to compensate: start ticks is minimum.");
        }
        else
        {
            if (backgroundBeginNetworkTime != DateTime.MinValue)
            {
                Debug.LogError("CRITICAL ERROR: backgroundBeginNetworkTime should be DateTime.MinValue at this time");
            }
            else
            {
                backgroundBeginNetworkTime = beginNetworkTime;
                ExecuteBackgroundTimeCompensationImmediatelyOrDelayed();
            }
        }
    }
}