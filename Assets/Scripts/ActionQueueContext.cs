using System.Collections.Generic;
using UnityEngine.Events;

public class ActionQueueContext
{
    readonly List<UnityAction<ActionQueueContext>> actionList;
    int currentIndex = -1;

    ActionQueueContext(List<UnityAction<ActionQueueContext>> actionList)
    {
        this.actionList = actionList;
    }

    public static void Start(List<UnityAction<ActionQueueContext>> actionList)
    {
        new ActionQueueContext(actionList).ExecuteNext();
    }

    public bool ExecuteNext()
    {
        // actionList 안에 있는 작업이 성공하든 실패하든 currentIndex가 1 증가하는 것이
        // 보장되어 있어야 무한 재귀에 의한 스택 오버플로 혹은 무한 루프에 빠지지 않는다.
        currentIndex++;
        if (actionList != null && currentIndex >= 0 && currentIndex < actionList.Count)
        {
            actionList[currentIndex](this);
            return true;
        }

        return false;
    }
}