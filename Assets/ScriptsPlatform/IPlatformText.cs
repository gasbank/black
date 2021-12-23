public interface IPlatformText
{
    string Str_InternetRequiredForAds { get; }
    string Str_AdsAborted { get; }
    string Str_UnityAdsError { get; }
    string Str_AdMobError { get; }
    string Str_FacebookAdsError { get; }
    string Str_SearchingAds { get; }
    string ConfirmMessage { get; }
    string LoginErrorMessage { get; }
    string LoginErrorTitle { get; }
    string GetNetworkTimeQueryProgressText(int oneBasedIndex, int totalCount);
}