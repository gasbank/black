using System;

public class NotSupportedBlackSaveDataVersionException : NotSupportedException
{
    public NotSupportedBlackSaveDataVersionException(int saveFileVersion)
    {
        SaveFileVersion = saveFileVersion;
    }

    public int SaveFileVersion { get; }
}