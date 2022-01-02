using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ConditionalDebug;
using JetBrains.Annotations;
using MiniJSON;
using UnityEngine;
using UnityEngine.Networking;
using Dict = System.Collections.Generic.Dictionary<string, object>;
using Random = System.Random;

public class ErrorReporter : MonoBehaviour
{
    public static ErrorReporter instance;

    // https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings
    static readonly Random RandomInstance = new Random();
    static readonly string ERROR_DEVICE_ID_KEY = "errorDeviceId";

    public static string RandomString(int length)
    {
        // ReSharper disable once StringLiteralTypo
        const string chars = "ACEFGHJKMNQRTUVWXY346"; // L, I, 1, 7, D, 0, O, 2, Z, P, 9, B, 8, S, 5 등 혼란을 주는 글씨는 뺐다.
        return new string(Enumerable.Repeat(chars, length).Select(s => s[RandomInstance.Next(s.Length)]).ToArray());
    }

    static string NewErrorPseudoId()
    {
        return $"E{RandomString(3)}-{RandomString(3)}";
    }

    public string GetOrCreateErrorDeviceId()
    {
        // PlayerPrefs에서 기기 고유 ID를 가져온다. (게임 지우면 리셋됨. 없으면 생성)
        var errorDeviceId = PlayerPrefs.GetString(ERROR_DEVICE_ID_KEY, NewErrorPseudoId());
        // 생성됐을 수도 있으니까 한번 저장
        PlayerPrefs.SetString(ERROR_DEVICE_ID_KEY, errorDeviceId);
        PlayerPrefs.Save();
        return errorDeviceId;
    }

    public async Task UploadSaveFileIncidentAsync(List<Exception> exceptionList, string st, bool notCriticalError)
    {
        var uploadSaveFileDbUrl = ConfigPopup.BaseUrl + "/save";
        ProgressMessage.instance.Open("\\저장 파일 문제 업로드 중...".Localized());

        var errorDeviceId = GetOrCreateErrorDeviceId();
        var url = $"{uploadSaveFileDbUrl}/{errorDeviceId}";
        var saveFile = new ErrorFile();
        var uploadDate = DateTime.UtcNow;
        try
        {
            saveFile.fields.uploadDate.timestampValue = uploadDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }
        catch
        {
            // ignored
        }

        try
        {
            saveFile.fields.stackTraceMessage.stringValue = string.Join("///", exceptionList.Select(e => e.ToString()));
        }
        catch
        {
            // ignored
        }

        try
        {
            saveFile.fields.appMetaInfo.stringValue = ConfigPopup.GetAppMetaInfo();
        }
        catch
        {
            // ignored
        }

        try
        {
            saveFile.fields.userId.stringValue = ConfigPopup.GetUserId();
        }
        catch
        {
            // ignored
        }

        try
        {
            LoadSaveDataSafe(ref saveFile.fields.saveData);
            LoadSaveDataSafe(ref saveFile.fields.saveData1);
            LoadSaveDataSafe(ref saveFile.fields.saveData2);
            LoadSaveDataSafe(ref saveFile.fields.saveData3);
            LoadSaveDataSafe(ref saveFile.fields.saveData4);
            LoadSaveDataSafe(ref saveFile.fields.saveData5);
            LoadSaveDataSafe(ref saveFile.fields.saveData6);
            LoadSaveDataSafe(ref saveFile.fields.saveData7);
        }
        catch (Exception e)
        {
            // 문제가 있는 파일을 업로드하는 것 조차 실패했다. 이건 수가 없네...
            ProgressMessage.instance.Close();
            ConfirmPopup.instance.Open($"SAVE FILE UPLOAD FAILED: {e}", () =>
            {
                if (notCriticalError == false)
                    Application.Quit();
                else
                    ConfirmPopup.instance.Close();
            });
            return;
        }

        try
        {
            using var httpClient = new HttpClient();
            var patchData = JsonUtility.ToJson(saveFile);
            using var patchContent = new StringContent(patchData);
            ConDebug.Log($"HttpClient PATCH TO {url}...");

            // PATCH 시작하고 기다린다.
            var patchTask = await httpClient.PatchAsync(new Uri(url), patchContent);

            ConDebug.Log($"HttpClient Result: {patchTask.ReasonPhrase}");

            if (patchTask.IsSuccessStatusCode)
            {
                var msg =
                    @"\$저장 데이터 개발팀으로 제출 결과$"
                        .Localized(errorDeviceId, patchData.Length, saveFile.fields.uploadDate.timestampValue);
                if (notCriticalError == false)
                    ConfirmPopup.instance.OpenTwoButtonPopup(msg, () => ConfigPopup.instance.OpenCommunity(),
                        () => SaveLoadManager.EnterRecoveryCode(exceptionList, st, false),
                        "\\업로드 완료".Localized(), "\\공식 카페 이동".Localized(), "\\복구 코드 입력".Localized());
                else
                    ConfirmPopup.instance.OpenTwoButtonPopup(msg, () => ConfirmPopup.instance.Close(),
                        () => SaveLoadManager.EnterRecoveryCode(exceptionList, st, true),
                        "\\업로드 완료".Localized(), "\\닫기".Localized(), "\\복구 코드 입력".Localized());
            }
            else
            {
                ShortMessage.instance.Show($"{patchTask.ReasonPhrase}");
                if (notCriticalError == false) // 다시 안내 팝업 보여주도록 한다.
                    SaveLoadManager.ProcessCriticalLoadError(exceptionList, st);
                else
                    ConfirmPopup.instance.Open($"SAVE FILE UPLOAD FAILED: {patchTask.ReasonPhrase}");
            }
        }
        catch (Exception e)
        {
            if (Debug.isDebugBuild) Debug.LogException(e);

            ConfirmPopup.instance.Open(e.Message, () =>
            {
                if (notCriticalError == false) // 다시 안내 팝업 보여주도록 한다.
                    SaveLoadManager.ProcessCriticalLoadError(exceptionList, st);
                else
                    ConfirmPopup.instance.Close();
            });
        }
        finally
        {
            // 어떤 경우가 됐든지 마지막으로는 진행 상황 창을 닫아야 한다.
            ProgressMessage.instance.Close();
        }
    }

    void LoadSaveDataSafe(ref ErrorFile.Fields.BytesValueData fieldsSaveData)
    {
        try
        {
            fieldsSaveData.bytesValue =
                Convert.ToBase64String(File.ReadAllBytes(SaveLoadManager.LoadFileName));
        }
        catch
        {
            fieldsSaveData.bytesValue = "";
        }

        SaveLoadManager.DecreaseSaveDataSlotAndWrite();
    }

    internal void ProcessRecoveryCode(List<Exception> exceptionList, string st, string recoveryCode)
    {
        StartCoroutine(ProcessRecoveryCodeCoro(exceptionList, st, recoveryCode));
    }

    IEnumerator ProcessRecoveryCodeCoro(List<Exception> exceptionList, string st, string recoveryCode)
    {
        var recoveryDbUrl = ConfigPopup.BaseUrl + "/recovery";
        ProgressMessage.instance.Open("\\복구 코드 확인중...".Localized());
        recoveryCode = recoveryCode.Trim();
        // 복구 코드를 특별히 입력하지 않았을 경우에는 Error Device ID가 기본으로 쓰인다.
        if (string.IsNullOrEmpty(recoveryCode))
        {
            recoveryCode = GetOrCreateErrorDeviceId();
        }
        // ReSharper disable once StringLiteralTypo
        else if (recoveryCode == "deleteall")
        {
            // 파일 삭제하고 새 게임 시작하는 개발자용 복구 코드
            SaveLoadManager.DeleteSaveFileAndReloadScene();
            yield break;
        }

        var url = $"{recoveryDbUrl}/{recoveryCode}";
        using var request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        ProgressMessage.instance.Close();
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            ShortMessage.instance.Show("\\복구 정보 수신에 실패했습니다.".Localized(), true);
        }
        else
        {
            try
            {
                ConDebug.LogFormat("URL Text: {0}", request.downloadHandler.text);
                if (Json.Deserialize(request.downloadHandler.text) is Dict recoveryDataRoot)
                {
                    foreach (var kv in recoveryDataRoot)
                    {
                        ConDebug.LogFormat("root key: {0}", kv.Key);
                    }

                    if (recoveryDataRoot["fields"] is Dict recoveryData)
                    {
                        foreach (var kv in recoveryData)
                        {
                            ConDebug.LogFormat("fields key: {0}", kv.Key);
                        }

                        foreach (var recovery in recoveryData)
                        {
                            var recoveryIndexParsed = int.TryParse(recovery.Key, out _);
                            // 이미 받았거나 이상한 항목은 스킵
                            if (recoveryIndexParsed == false) continue;
                            var fields = (Dict) ((Dict) ((Dict) recovery.Value)["mapValue"])["fields"];
                            var isValidErrorDeviceId = false;
                            var saveDataBase64 = "";
                            byte[] saveData = null;
                            var recoveryErrorDeviceId = "";
                            //var serviceValue = service.Value as 
                            foreach (var recoveryItem in fields)
                                if (recoveryItem.Key == "errorDeviceId")
                                {
                                    recoveryErrorDeviceId = ((Dict) recoveryItem.Value)["stringValue"] as string;
                                    if (recoveryErrorDeviceId == GetOrCreateErrorDeviceId())
                                        isValidErrorDeviceId = true;
                                }
                                else if (recoveryItem.Key == "saveData")
                                {
                                    saveDataBase64 = ((Dict) recoveryItem.Value)["stringValue"] as string;
                                    if (string.IsNullOrEmpty(saveDataBase64) == false)
                                    {
                                        saveData = Convert.FromBase64String(saveDataBase64);
                                    }
                                }

                            ConDebug.LogFormat("Error Device ID: {0}", GetOrCreateErrorDeviceId());
                            ConDebug.LogFormat("Recovery Error Device ID: {0}", recoveryErrorDeviceId);
                            ConDebug.LogFormat("Save Data Base64 ({0} bytes): {1}",
                                saveDataBase64?.Length ?? 0, saveDataBase64);

                            if (isValidErrorDeviceId && saveData != null && saveData.Length > 0)
                            {
                                // 복구 성공!!
                                // 새로운 세이브 파일 쓰고, 다시 Splash 신 로드
                                ConDebug.LogFormat("Writing recovery save data {0} bytes", saveData.Length);
                                File.WriteAllBytes(SaveLoadManager.SaveFileName, saveData);
                                // 일반적인 저장 경로가 아니고 파일을 직접 만들어낸 것이라서 수동으로 저장 슬롯 인덱스 증가시켜 줘야
                                // 다음에 직전에 저장한 슬롯의 저장 데이터를 불러온다.
                                SaveLoadManager.IncreaseSaveDataSlotAndWrite();
                                Splash.LoadSplashScene();
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
                // 딱히 할 수 있는 게 없다
            }

            // 여기까지 왔으면 복구가 제대로 안됐다는 뜻이다.
            ConfirmPopup.instance.Open(@"\$복구 코드 오류$".Localized(),
                () => SaveLoadManager.ProcessCriticalLoadError(exceptionList, st));
        }
    }

    // 유저가 응급 제출한 오류가 있는 세이브 파일을 개발자가 재현해 볼 때 사용한다.
    // domain: save 또는 playLog
    internal void ProcessUserSaveCode(string userSaveCode, string domain)
    {
        StartCoroutine(ProcessUserSaveCodeCoro(userSaveCode, domain));
    }

    IEnumerator ProcessUserSaveCodeCoro(string userSaveCode, string domain)
    {
        var saveDbUrl = ConfigPopup.BaseUrl + "/" + domain;
        ProgressMessage.instance.Open("\\유저 세이브 코드 확인중...".Localized());
        userSaveCode = userSaveCode.Trim();
        var url = $"{saveDbUrl}/{userSaveCode}";
        ConDebug.LogFormat("URL: {0}", url);
        using var request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();
        ProgressMessage.instance.Close();
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            ShortMessage.instance.Show("\\복구 정보 수신에 실패했습니다.".Localized(), true);
        }
        else
        {
            try
            {
                if (Json.Deserialize(request.downloadHandler.text) is Dict userSaveDataRoot)
                {
                    // userSaveDataFields는 정렬되어 있지 않다. saveData, saveData, saveData3, ... 순으로
                    // 로드 시도하기 위해서 필터링 및 정렬한다.
                    if (userSaveDataRoot["fields"] is Dict userSaveDataFields)
                        foreach (var fieldName in userSaveDataFields.Keys.Where(e => e.StartsWith("saveData"))
                            .OrderBy(e => e))
                        {
                            ConDebug.Log($"Checking save data field name '{fieldName}'...");
                            if (userSaveDataFields[fieldName] is Dict userSaveDataFieldsSaveData && userSaveDataFieldsSaveData.Keys.Count > 0)
                            {
                                var userSaveDataFieldsSaveDataStringValue =
                                    (userSaveDataFieldsSaveData.ContainsKey("bytesValue")
                                        ? userSaveDataFieldsSaveData["bytesValue"]
                                        : userSaveDataFieldsSaveData["stringValue"]) as string;

                                var saveDataBase64 = userSaveDataFieldsSaveDataStringValue;
                                var saveData = Convert.FromBase64String(saveDataBase64 ?? throw new NullReferenceException());

                                ConDebug.LogFormat("Save Data Base64 ({0} bytes): {1}",
                                    saveDataBase64.Length, saveDataBase64);

                                if (saveData.Length > 0)
                                {
                                    // 기존 세이브 데이터는 모두 지운다.
                                    SaveLoadManager.DeleteAllSaveFiles();

                                    ConDebug.LogFormat("Writing recovery save data {0} bytes", saveData.Length);
                                    File.WriteAllBytes(SaveLoadManager.SaveFileName, saveData);
                                    // 일반적인 저장 경로가 아니고 파일을 직접 만들어낸 것이라서 수동으로 저장 슬롯 인덱스 증가시켜 줘야
                                    // 다음에 직전에 저장한 슬롯의 저장 데이터를 불러온다.
                                    SaveLoadManager.IncreaseSaveDataSlotAndWrite();
                                    Splash.LoadSplashScene();
                                    yield break;
                                }

                                ConDebug.Log($"Save data field name '{fieldName}' is empty!");
                            }
                            else
                            {
                                ConDebug.Log($"Save data field name '{fieldName}' is empty!");
                            }
                        }
                }
            }
            catch (Exception e)
            {
                // 딱히 할 수 있는 게 없다
                Debug.LogException(e);
            }

            // 여기까지 왔으면 복구가 제대로 안됐다는 뜻이다.
            ConfirmPopup.instance.Open(
                "\\유저 세이브 코드가 잘못됐거나, 복구 데이터가 존재하지 않습니다.\\n\\n확인을 눌러 처음 화면으로 돌아갑니다.".Localized());
        }
    }

    [Serializable]
    class ErrorFile
    {
        public Fields fields = new Fields();

        [Serializable]
        public class Fields
        {
            public StringValueData appMetaInfo = new StringValueData();
            public BytesValueData saveData = new BytesValueData();
            public BytesValueData saveData1 = new BytesValueData();
            public BytesValueData saveData2 = new BytesValueData();
            public BytesValueData saveData3 = new BytesValueData();
            public BytesValueData saveData4 = new BytesValueData();
            public BytesValueData saveData5 = new BytesValueData();
            public BytesValueData saveData6 = new BytesValueData();
            public BytesValueData saveData7 = new BytesValueData();
            public StringValueData stackTraceMessage = new StringValueData();
            public TimestampValueData uploadDate = new TimestampValueData();
            public StringValueData userId = new StringValueData();

            [Serializable]
            public class BytesValueData
            {
                [UsedImplicitly]
                public string bytesValue;
            }

            [Serializable]
            public class TimestampValueData
            {
                public string timestampValue;
            }

            [Serializable]
            public class StringValueData
            {
                [UsedImplicitly]
                public string stringValue;
            }
        }
    }
}