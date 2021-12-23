using System;
using System.IO;
using UnityEngine;
using RemoteSaveDictionary = System.Collections.Generic.Dictionary<string, byte[]>;

public class PlatformEditor : MonoBehaviour, IPlatformBase
{
    static int screenshotIndex = 0;

    [SerializeField]
    PlatformInterface platformInterface;

    [SerializeField]
    PlatformSaveUtil platformSaveUtil;

    static string RemoteSaveFileForEditor => Application.persistentDataPath + "/" + PlatformSaveUtil.remoteSaveFileName;

    public bool CheckLoadSavePrecondition(string progressMessage, Action onNotLoggedIn, Action onAbort)
    {
        if (string.IsNullOrEmpty(progressMessage) == false)
            PlatformInterface.instance.progressMessage.Open(progressMessage);
        return true;
    }

    public void ExecuteCloudLoad()
    {
        platformSaveUtil.ShowLoadProgressPopup();

        var remoteSaveDict =
            PlatformInterface.instance.saveUtil.DeserializeSaveData(File.ReadAllBytes(RemoteSaveFileForEditor));

        PlatformInterface.instance.saveUtil.LoadDataAndLoadSplashScene(remoteSaveDict);
    }

    public void ExecuteCloudSave()
    {
        PlatformInterface.instance.saveLoadManager.SaveBeforeCloudSave();
        platformSaveUtil.ShowSaveProgressPopup();

        var savedData = PlatformInterface.instance.saveUtil.SerializeSaveData();

        using (var f = File.Create(RemoteSaveFileForEditor))
        {
            f.Write(savedData, 0, savedData.Length);
        }

        var remoteSaveDict = PlatformInterface.instance.saveUtil.DeserializeSaveData(savedData);
        ShowSaveResultPopup(savedData, remoteSaveDict, RemoteSaveFileForEditor);
    }

    public void Login(Action<bool, string> onAuthResult)
    {
        onAuthResult(false, "Not supported platform");
    }

    public void Logout()
    {
        PlatformInterface.instance.logger.Log("PlatformEditor.Logout()");
    }

    public void GetCloudLastSavedMetadataAsync(Action<byte[]> onPeekResult)
    {
        platformSaveUtil.ShowPeekProgressPopup();

        onPeekResult(File.Exists(RemoteSaveFileForEditor) ? File.ReadAllBytes(RemoteSaveFileForEditor) : null);
    }

    public void PreAuthenticate()
    {
    }

    public bool LoginFailedLastTime()
    {
        return true;
    }

    public void RegisterAllNotifications(string title, string body, string largeIcon, int localHours)
    {
        PlatformInterface.instance.logger.LogFormat(
            "RegisterAllNotifications: title={0}, body={1}, largeIcon={2}, localHours={3}", title, body, largeIcon,
            localHours);
    }

    public void ClearAllNotifications()
    {
        PlatformInterface.instance.logger.LogFormat("ClearAllNotifications");
    }

    public void OnCloudSaveResult(string result)
    {
        throw new NotImplementedException();
    }

    public void OnCloudLoadResult(string result, byte[] data)
    {
        throw new NotImplementedException();
    }

    public void RequestUserReview()
    {
        Application.OpenURL(PlatformInterface.instance.config.GetUserReviewUrl());
    }

    public void RegisterSingleNotification(string title, string body, int afterMs, string largeIcon)
    {
        PlatformInterface.instance.logger.LogFormat("RegisterSingleNotification: title={0}, body={1}, afterMs={2}",
            title, body, afterMs);
    }

    public string GetAccountTypeText()
    {
        return PlatformInterface.instance.textHelper.GetText("platform_account_editor");
    }

    public void Report(string reportPopupTitle, string mailTo, string subject, string text, byte[] saveData)
    {
        var str = $"버그 메일을 보냅니다. 수신자: {mailTo}, 제목: {subject}, 본문: {text}, 세이브데이터크기: {saveData.Length} bytes";
        PlatformInterface.instance.confirmPopup.Open(str);
    }

    public void ShareScreenshot(byte[] pngData)
    {
        PlatformInterface.instance.logger.LogFormat("ShareScreenshot: pngData length {0} bytes", pngData.Length);

        var screenshotFileName = "screenshot.png";
        if (Application.isEditor)
        {
            screenshotFileName = $"screenshot{screenshotIndex:D3}.png";
            screenshotIndex++;
        }

        File.WriteAllBytes(screenshotFileName, pngData);
        PlatformInterface.instance.logger.Log($"ShareScreenshot: successfully written to {screenshotFileName}");
    }

    void ShowSaveResultPopup(byte[] savedData, RemoteSaveDictionary remoteSaveDict, string path)
    {
        PlatformInterface.instance.progressMessage.Close();
        var text = PlatformInterface.instance.saveUtil.GetEditorSaveResultText(savedData, remoteSaveDict, path);
        PlatformInterface.instance.confirmPopup.Open(text);
    }

    // 가장 가까운 'localHours시'를 나타내는 DateTime을 반환한다.
    public static DateTime GetNextLocalHours(int localHours)
    {
        var now = DateTime.Now;
        var notificationDate0000 = DateTime.Today;
        var notificationDate0900 = notificationDate0000.AddHours(localHours);
        if (notificationDate0900 < now) notificationDate0900 = notificationDate0900.AddDays(1);

        return notificationDate0900;
    }
}