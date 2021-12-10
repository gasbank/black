public class NotSupportedBlackSaveDataVersionException : System.NotSupportedException {
    public int SaveFileVersion { get; }
    public NotSupportedBlackSaveDataVersionException(int saveFileVersion) {
        SaveFileVersion = saveFileVersion;
    }
}
