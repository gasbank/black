public static class BlackRandom {
    static System.Random rng = new System.Random();

    public static void InitState(int seed) {
        //ConDebug.Log($"[RANDOM] InitState({seed})");
        UnityEngine.Random.InitState(seed);
    }

    public static float Range(float min, float max) {
        //ConDebug.Log($"[RANDOM] Range({min}, {max})");
        return UnityEngine.Random.Range(min, max);
    }

    public static int Range(int min, int max) {
        //ConDebug.Log($"[RANDOM] Range({min}, {max})");
        return UnityEngine.Random.Range(min, max);
    }
    
    public static long Range(long min, long max) {
        //ConDebug.Log($"[RANDOM] Range({min}, {max})");
        return (long)UnityEngine.Random.Range(min, max);
    }

    public static void Shuffle<T>(System.Collections.Generic.IList<T> list) {
        Shuffle(list, rng);
    }

    public static void Shuffle<T>(System.Collections.Generic.IList<T> list, System.Random random) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static UnityEngine.Color ColorHSV(float hueMin, float hueMax, float saturationMin, float saturationMax,
        float valueMin, float valueMax) {
        //ConDebug.Log($"[RANDOM] ColorHSV({hueMin}, {hueMax}, {saturationMin}, {saturationMax}, {valueMin}, {valueMax})");
        return UnityEngine.Random.ColorHSV(hueMin, hueMax, saturationMin, saturationMax, valueMin, valueMax);
    }

    public static UnityEngine.Vector2 insideUnitCircle {
        get {
            //ConDebug.Log($"[RANDOM] insideUnitCircle");
            return UnityEngine.Random.insideUnitCircle;
        }
    }

    public static UnityEngine.Vector3 onUnitSphere {
        get {
            //ConDebug.Log($"[RANDOM] onUnitSphere");
            return UnityEngine.Random.onUnitSphere;
        }
    }
}