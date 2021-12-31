using System;

public class BlackAdContext
{
    public BlackAdContext(Action executeReward)
    {
        ExecuteReward = executeReward;
    }

    public Action ExecuteReward { get; }
}