using UnityEngine;
using UnityEngine.EventSystems;

public class AchievementRedeemButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    AchievementEntry achievementEntry;

    [SerializeField]
    bool down;

    [SerializeField]
    float downDuration;

    [SerializeField]
    float lastRepeatRedeemTime;

    [SerializeField]
    string repeatAchievementCondition;

    [SerializeField]
    float repeatRedeemInterval = 0.5f;

    [SerializeField]
    float repeatRedeemThreshold = 1.5f;

    public bool RepeatedThresholdSatisfied => downDuration > repeatRedeemThreshold;

    public void OnPointerDown(PointerEventData eventData)
    {
        down = true;
        downDuration = 0;
        repeatAchievementCondition = achievementEntry.achievementData.condition;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        down = false;
        lastRepeatRedeemTime = 0;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        achievementEntry = GetComponentInParent<AchievementEntry>();
    }
#endif

    bool GetRepeatCondition(float step, float speed)
    {
        return downDuration > repeatRedeemThreshold * step
               && Time.time - lastRepeatRedeemTime > repeatRedeemInterval / speed;
    }

    void Update()
    {
        if (down)
        {
            downDuration += Time.deltaTime;
            if (repeatAchievementCondition == achievementEntry.achievementData.condition
                && (GetRepeatCondition(1.0f, 1.0f) || GetRepeatCondition(1.5f, 2.0f) ||
                    GetRepeatCondition(2.0f, 4.0f) || GetRepeatCondition(2.5f, 999.0f)))
            {
                achievementEntry.RedeemInternal();
                lastRepeatRedeemTime = Time.time;
            }
        }
    }
}