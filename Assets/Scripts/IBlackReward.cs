public interface IBlackReward
{
    RewardType RewardType { get; }
    ScInt Amount { get; }
    string AmountPostfixed { get; }
    ScString NotificationDesc { get; }
}