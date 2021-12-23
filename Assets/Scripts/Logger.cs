using ConditionalDebug;
using UnityEngine;

public static class Logger
{
    public static void WriteLine(string s)
    {
#if UNITY_2020
        ConDebug.Log(s);
#else
        Logger.WriteLine(s);
#endif
    }

    public static void WriteErrorLine(string s)
    {
#if UNITY_2020
        Debug.LogError(s);
#else
        Logger.WriteLine(s);
#endif
    }
}