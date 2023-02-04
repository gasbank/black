using ConditionalDebug;
using UnityEngine;

public static class Logger
{
    public static void WriteLine(string s)
    {
#if UNITY_2020_1_OR_NEWER
        ConDebug.Log(s);
#else
        Logger.WriteLine(s);
#endif
    }

    public static void WriteErrorLine(string s)
    {
#if UNITY_2020_1_OR_NEWER
        Debug.LogError(s);
#else
        Logger.WriteLine(s);
#endif
    }
}