using System;
using System.IO;
using UnityEngine;

public static class FileUtil {
    public static void SaveAtomically(string fileName, byte[] bytes) {
        var temporaryPath = GetTempPath();
        using (var tempFile = File.Create(temporaryPath, 4 * 1024, FileOptions.WriteThrough)) {
            tempFile.Write(bytes, 0, bytes.Length);
            tempFile.Close();
        }
        var savePath = GetPath(fileName);
        File.Delete(savePath);
        File.Move(temporaryPath, savePath);
    }

    public static string GetPath(string fileName) {
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    public static string GetTempPath() {
        return Path.Combine(Application.temporaryCachePath, Guid.NewGuid().ToString());
    }
}
