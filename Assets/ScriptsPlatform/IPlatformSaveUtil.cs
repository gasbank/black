using System;
using System.Collections.Generic;

public interface IPlatformSaveUtil
{
    byte[] SerializeSaveData();
    Dictionary<string, byte[]> DeserializeSaveData(byte[] bytes);
    void LoadDataAndLoadSplashScene(Dictionary<string, byte[]> remoteSaveDict);
    string GetEditorSaveResultText(byte[] savedData, Dictionary<string, byte[]> remoteSaveDict, string path);
    TimeSpan GetPlayed();
    string GetDesc(byte[] bytes);
    void DebugPrintCloudMetadata(byte[] bytes);
    bool IsValidCloudMetadata(byte[] bytes);
}