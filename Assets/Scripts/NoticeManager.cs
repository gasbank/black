using System.Collections;
using ConditionalDebug;
using UnityEngine;
using UnityEngine.Networking;
using Dict = System.Collections.Generic.Dictionary<string, object>;

[DisallowMultipleComponent]
public class NoticeManager : MonoBehaviour
{
    public static NoticeManager instance;

    static public readonly string noticeUrlQueryStartIndexKey = "noticeUrlQueryStartIndexKey";

    // 유저가 직접 공지사항을 확인하려고 할 때 호출된다.
    // 공지사항 창이 열린다.
    public void Open()
    {
        BlackLogManager.Add(BlackLogEntry.Type.GameOpenNotice, 0, 0);
        //StopAllCoroutines();
        StartCoroutine(CheckNoticeCoro(false, null, null, null));
    }

    // 새 공지가 있다면 빨간색 느낌표를 보여준다.
    // 창을 열지는 않는다.
    public void CheckNoticeSilently()
    {
        // StopAllCoroutines();
        StartCoroutine(CheckNoticeCoro(true, BlackContext.instance.NoticeData.title,
            BlackContext.instance.NoticeData.text, BlackContext.instance.NoticeData.detailUrl));
        StopCoroutine(CheckNoticeCoro(true, BlackContext.instance.NoticeData.title,
            BlackContext.instance.NoticeData.text, BlackContext.instance.NoticeData.detailUrl));
    }

    IEnumerator DownloadAndGetSpriteCoro(string imageUrl, System.Action<Sprite> callback)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            var tex = ((DownloadHandlerTexture) request.downloadHandler).texture;
            var sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f),
                100.0f);
            callback(sprite);
        }
    }

    IEnumerator CheckNoticeCoro(bool silent, string oldTitle, string oldText, string oldDetailUrl)
    {
        if (silent == false)
        {
            ProgressMessage.instance.Open("\\공지사항 항목 확인중...".Localized());
        }

        var urlList = new[]
        {
            // Google Firebase
            string.Format("{0}/{1}", ConfigPopup.NoticeDbUrl, "notice"),
            // AliCloud
            "https://blacktycoon.oss-cn-beijing.aliyuncs.com/notice.json",
        };

        var succeeded = false;
        for (int i = 0; i < urlList.Length; i++)
        {
            var urlIndex = (i + PlayerPrefs.GetInt(noticeUrlQueryStartIndexKey, 0)) % urlList.Length;
            var coroResult = new CoroutineWithData(this,
                CheckNoticeCoroUrl(silent, oldTitle, oldText, oldDetailUrl, urlList[urlIndex]));
            yield return coroResult.coroutine;
            if ((bool) coroResult.result)
            {
                succeeded = true;
                PlayerPrefs.SetInt(noticeUrlQueryStartIndexKey, urlIndex);
                PlayerPrefs.Save();
                break;
            }
        }

        if (succeeded == false)
        {
            if (silent == false)
            {
                ShortMessage.instance.Show("\\공지사항 정보 수신에 실패했습니다.".Localized(), true);
                ProgressMessage.instance.Close();
            }
        }

        // var url1 = string.Format("{0}/{1}", ConfigPopup.NoticeDbUrl, "notice");
        // var coroResult = new CoroutineWithData(this, CheckNoticeCoroUrl(silent, oldTitle, oldText, oldDetailUrl, url1));
        // yield return coroResult.coroutine;
        // var webRequestCompleted = (bool)coroResult.result;
        // if (webRequestCompleted == false) {
        //     var url2 = "https://blacktycoon.oss-cn-beijing.aliyuncs.com/notice.json";
        //     yield return CheckNoticeCoroUrl(silent, oldTitle, oldText, oldDetailUrl, url2);
        // }
    }

    IEnumerator CheckNoticeCoroUrl(bool silent, string oldTitle, string oldText, string oldDetailUrl, string url)
    {
        var webRequestCompleted = false;
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            ConDebug.Log($"Notice: GET {url}");
            request.timeout = 5;
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                // 아무일도 하지 않는다. webRequestCompleted가 false인 채로 반환되게 된다.
            }
            else
            {
                ProgressMessage.instance.Close();
                var title = "";
                var text = "";
                var detailUrl = "";
                var topImageUrl = "";
                var popupImageUrl = "";
                var popupImageHeight = 0;

                try
                {
                    var dateStr = request.GetResponseHeader("Date");
                    var dateTimeNow = new System.DateTime();
                    if (string.IsNullOrEmpty(dateStr) == false && System.DateTime.TryParse(dateStr, out dateTimeNow))
                    {
                        dateTimeNow = dateTimeNow.ToUniversalTime();
                        ConDebug.Log($"Date header from notice: UTC {dateTimeNow}");
                    }
                    else
                    {
                        ConDebug.Log($"Date header from notice [***INVALID***]: dateStr={dateStr}");
                    }

                    var noticeDataRoot = MiniJSON.Json.Deserialize(request.downloadHandler.text) as Dict;

                    if (noticeDataRoot?["fields"] is Dict noticeData)
                    {
                        foreach (var notice in noticeData)
                        {
                            if (notice.Key == "title")
                            {
                                title = ((Dict) notice.Value)["stringValue"] as string;
                            }
                            else if (notice.Key == "text")
                            {
                                text = ((Dict) notice.Value)["stringValue"] as string;
                            }
                            else if (notice.Key == "url")
                            {
                                detailUrl = ((Dict) notice.Value)["stringValue"] as string;
                            }
                            else if (notice.Key == "topImageUrl")
                            {
                                topImageUrl = ((Dict) notice.Value)["stringValue"] as string;
                            }
                            else if (notice.Key == "popupImageUrl")
                            {
                                popupImageUrl = ((Dict) notice.Value)["stringValue"] as string;
                            }
                            else if (notice.Key == "popupImageHeight")
                            {
                                popupImageHeight = int.Parse(((Dict) notice.Value)["integerValue"] as string);
                            }
                        }
                    }

                    webRequestCompleted = true;
                }
                catch (System.Exception e)
                {
                    Debug.LogErrorFormat("Notice exception: {0}", e.ToString());
                    if (silent == false)
                    {
                        ConfirmPopup.instance.Open("\\공지사항 항목이 없습니다.".Localized());
                    }
                }

                if (webRequestCompleted == false)
                {
                    yield return webRequestCompleted;
                    yield break;
                }


                var isUpdatedNotice = oldTitle != title || oldText != text || oldDetailUrl != detailUrl;
                if (isUpdatedNotice)
                {
                    if (silent)
                    {
                        // 몰래 체크해서 새 공지가 있으니까 느낌표 보여준다.
                        ConfigPopup.instance.ActivateNoticeNewImage(true);
                    }
                    else
                    {
                        // 서버 DB가 값에 새줄 문자를 지원하지 않는다. (JSON 스펙)
                        // 직접 바꿔주자.
                        var textNewlined = text.Replace("\\n", "\n").Replace("\\r", "");
                        if (string.IsNullOrEmpty(detailUrl))
                        {
                            //ConfirmPopup.instance.Open(textNewlined, ConfirmPopup.instance.Close, title);
                            ConfirmPopup.instance.OpenPopup(textNewlined, ConfirmPopup.instance.Close, null, null,
                                title, Header.Normal, "\\확인".Localized(), null, null, "", "", false, null,
                                null, WidthType.Normal, 0, ShowPosition.Center,
                                ConfirmPopup.instance.Close, false, -1, FontManager.instance.SystemFont);
                        }
                        else
                        {
                            //ConfirmPopup.instance.OpenGeneralPopup(textNewlined, ConfirmPopup.instance.Close, () => Application.OpenURL(detailUrl), null, title, ConfirmPopup.Header.Normal, "\\확인".Localized(), "\\자세히 보기".Localized(), "");
                            ConfirmPopup.instance.OpenPopup(textNewlined, ConfirmPopup.instance.Close,
                                () => Application.OpenURL(detailUrl), null, title, Header.Normal,
                                "\\확인".Localized(), "\\자세히 보기".Localized(), null, "", "", false, null, null,
                                WidthType.Normal, 0, ShowPosition.Center,
                                ConfirmPopup.instance.Close, false, -1, FontManager.instance.SystemFont);
                        }

                        // 공지사항용 상단 이미지가 지정되어 있다면 다운로드해서 보여준다.
                        if (string.IsNullOrEmpty(topImageUrl) == false)
                        {
                            ProgressMessage.instance.Open("\\공지사항 항목 확인중...".Localized());
                            yield return DownloadAndGetSpriteCoro(topImageUrl,
                                sprite => ConfirmPopup.instance.ActivateTopImage(sprite));
                            ProgressMessage.instance.Close();
                        }

                        // 공지사항용 본문 이미지가 지정되어 있다면 다운로드해서 보여준다.
                        if (string.IsNullOrEmpty(popupImageUrl) == false && popupImageHeight > 0)
                        {
                            ProgressMessage.instance.Open("\\공지사항 항목 확인중...".Localized());
                            yield return DownloadAndGetSpriteCoro(popupImageUrl,
                                sprite => ConfirmPopup.instance.ActivatePopupImage(sprite, 0, popupImageHeight));
                            ProgressMessage.instance.Close();
                        }

                        // 마지막 본 공지 내용으로 갱신
                        BlackContext.instance.NoticeData.title = title;
                        BlackContext.instance.NoticeData.text = text;
                        BlackContext.instance.NoticeData.detailUrl = detailUrl;


                        // 새 공지든 아니든 한번 열었으니까 느낌표 숨긴다.
                        ConfigPopup.instance.ActivateNoticeNewImage(false);
                    }
                }
                else
                {
                    // 마지막 확인 이후 공지가 바뀌지 않았다. 느낌표 숨긴다.
                    ConfigPopup.instance.ActivateNoticeNewImage(false);
                }
            }
        }

        yield return webRequestCompleted;
    }
}