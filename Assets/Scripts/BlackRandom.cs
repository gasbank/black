using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public static class BlackRandom
{
    static readonly Random rng = new Random();

    public static Vector2 insideUnitCircle => UnityEngine.Random.insideUnitCircle;

    public static Vector3 onUnitSphere => UnityEngine.Random.onUnitSphere;

    public static void InitState(int seed)
    {
        //ConDebug.Log($"[RANDOM] InitState({seed})");
        UnityEngine.Random.InitState(seed);
    }

    public static float Range(float min, float max)
    {
        //ConDebug.Log($"[RANDOM] Range({min}, {max})");
        return UnityEngine.Random.Range(min, max);
    }

    public static int Range(int min, int max)
    {
        //ConDebug.Log($"[RANDOM] Range({min}, {max})");
        return UnityEngine.Random.Range(min, max);
    }

    public static long Range(long min, long max)
    {
        //ConDebug.Log($"[RANDOM] Range({min}, {max})");
        return (long) UnityEngine.Random.Range(min, max);
    }

    public static void Shuffle<T>(IList<T> list)
    {
        Shuffle(list, rng);
    }

    public static void Shuffle<T>(IList<T> list, Random random)
    {
        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = random.Next(n + 1);
            var value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static Color ColorHSV(float hueMin, float hueMax, float saturationMin, float saturationMax,
        float valueMin, float valueMax)
    {
        //ConDebug.Log($"[RANDOM] ColorHSV({hueMin}, {hueMax}, {saturationMin}, {saturationMax}, {valueMin}, {valueMax})");
        return UnityEngine.Random.ColorHSV(hueMin, hueMax, saturationMin, saturationMax, valueMin, valueMax);
    }
}