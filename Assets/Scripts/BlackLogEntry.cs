using System;

[Serializable]
public struct BlackLogEntry : IPlatformLogEntryType
{
    public enum Type
    {
        // === 보석 변경 이벤트 ===
        GemSection = 1000,

        // 인앱 결제로 보석 증가
        GemAddPurchase,

        // 무료로 받아서 보석 증가
        GemAddFree,

        // 광고 보고 보석 증가
        GemAddRewardVideo,

        // 업적 보상으로 보석 증가
        GemAddAchievement,

        // 보석 0으로
        GemToZero,

        // 어드민 커맨드로 증가
        GemAddAdmin,

        // 저장 데이터에서 불러온 값
        GemToLoaded,

        // 보석으로 에너지 사면서 줄어든 것
        GemSubtractGemToGold,

        // 밀린 보석 지급
        GemAddPending,

        // 보석 추가 시 오버플로우
        GemAddOverflowFreeGem,
        GemAddOverflowPaidGem,

        // === 응급 서비스 수령 이벤트 ===
        ServiceSection = 2000,

        // 응급 서비스 보석 항목 받음 (아직 더하기 전)
        ServiceGem,

        // 응급 서비스 에너지 항목 받음 (아직 더하기 전)
        ServiceGold,

        // === 게임 이벤트 ===
        GameSection = 4000,

        // 새 게임 시작
        GameReset,

        // 이전 저장 데이터 읽기 성공
        GameLoaded,

        // 이전 저장 데이터 읽기 실패
        GameLoadFailure,

        // 저장 성공
        GameSaved,

        // 저장 실패
        GameSaveFailure,

        // 클라우드 불러오기 시작
        GameCloudLoadBegin,

        // 클라우드 불러오기 끝
        GameCloudLoadEnd,

        // 클라우드 불러오기 실패
        GameCloudLoadFailure,

        // 클라우드 저장 시작
        GameCloudSaveBegin,

        // 클라우드 저장 끝
        GameCloudSaveEnd,

        // 클라우드 저장 실패
        GameCloudSaveFailure,

        // 중대 결함 발견
        GameCriticalError,

        // 리더보드 버튼 누르기
        GameOpenLeaderboard,

        // 공지사항 확인
        GameOpenNotice,

        // 어드민 커맨드 열기
        GameCheatEnabled,

        // 인앱 결제 성공
        GamePurchased,

        // 인앱 결제 실패
        GamePurchaseFailure,

        // 응급 서비스 버튼 누르기
        GameOpenService,

        // 업적 버튼 누르기
        GameOpenAchievements,

        // === 에너지 증가 이벤트 ===
        GoldSection = 7000,
        GoldAddPending,

        GoldAddAdmin,

        // 상점 아이템 종류별 구매 비용 차감
        GoldSubtractShopItemSection = 7100,

        // === 깜짝 감사선물 이벤트 ===
        DailyRewardSection = 8000,

        // 깜짝 감사선물 받기 이벤트
        DailyRewardRewardSection = 8100
    }

    public long ticks;
    public int type;
    public int arg1;
    public long arg2;

    public string TypeStr => ((Type) type).ToString();

    string ToColoredTypeString(string typeStr)
    {
        var coloredTypeStr = typeStr;
        if (coloredTypeStr.StartsWith("GemSubtract"))
            coloredTypeStr = $"<color=red>{coloredTypeStr}</color>";
        else if (coloredTypeStr.StartsWith("GemAdd")) coloredTypeStr = $"<color=green>{coloredTypeStr}</color>";
        return coloredTypeStr;
    }

    public override string ToString()
    {
        return ToStringAligned(TypeStr);
    }

    public string ToColoredString()
    {
        return ToStringAligned(ToColoredTypeString(TypeStr));
    }

    public string ToTabbedString()
    {
        return ToStringTabbed(TypeStr);
    }

    string ToStringAligned(string typeStr)
    {
        return string.Format("{0} {1,-20} {2,6:n0} {3:n0}",
            new DateTime(ticks, DateTimeKind.Utc).ToLocalTime().ToString("MM/dd HH:mm"),
            typeStr,
            arg1,
            arg2);
    }

    string ToStringTabbed(string typeStr)
    {
        return string.Format("{0}\t{1}\t{2}\t{3}",
            new DateTime(ticks, DateTimeKind.Utc).ToLocalTime().ToString("MM/dd HH:mm"),
            typeStr,
            arg1,
            arg2);
    }

    public int GameCloudLoadBegin => (int) Type.GameCloudLoadBegin;
    public int GameCloudSaveBegin => (int) Type.GameCloudSaveBegin;
    public int GameCloudSaveEnd => (int) Type.GameCloudSaveEnd;
    public int GameCloudLoadFailure => (int) Type.GameCloudLoadFailure;
    public int GameCloudLoadEnd => (int) Type.GameCloudLoadEnd;
    public int GameCloudSaveFailure => (int) Type.GameCloudSaveFailure;
    public int GameOpenLeaderboard => (int) Type.GameOpenLeaderboard;
    public int GameOpenAchievements => (int) Type.GameOpenAchievements;
}