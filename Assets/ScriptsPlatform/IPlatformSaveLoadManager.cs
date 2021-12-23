public interface IPlatformSaveLoadManager
{
    string GetLoadOverwriteConfirmMessage(byte[] bytes);
    string GetSaveOverwriteConfirmMessage(byte[] bytes);
    bool IsLoadRollback(byte[] bytes);
    bool IsSaveRollback(byte[] bytes);
    void SaveBeforeCloudSave();
}