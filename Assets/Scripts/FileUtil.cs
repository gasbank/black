using System.IO;
using UnityEngine;

public static class FileUtil {
    public static void SaveAtomically(string name, byte[] bytes) {
        var temporaryPath = GetTempPath(name);
        using (var tempFile = File.Create(temporaryPath, 4 * 1024, FileOptions.WriteThrough)) {
            tempFile.Write(bytes, 0, bytes.Length);
            tempFile.Close();
        }
        var savePath = GetPath(name);
        File.Delete(savePath);
        File.Move(temporaryPath, savePath);
    }

    static string GetFileName(string name) => $"{name}.save";

    public static string GetPath(string name) {
        return Path.Combine(Application.persistentDataPath, GetFileName(name));
    }

    public static string GetTempPath(string name) {
        return Path.Combine(Application.temporaryCachePath, GetFileName(name));
    }
}
