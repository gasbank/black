using UnityEngine;

public class BlackConvert {
    public static uint GetC(Color32 v) {
        return ((uint)v.a << 24) + ((uint)v.b << 16) + ((uint)v.g << 8) + (uint)v.r;
    }

    public static Color GetColor(uint v) {
        return new Color {
            r = (v & 0xff) / 255.0f,
            g = ((v >> 8) & 0xff) / 255.0f,
            b = ((v >> 16) & 0xff) / 255.0f,
            a = ((v >> 24) & 0xff) / 255.0f,
        };
    }

    public static uint GetP(Vector2Int k) {
        return ((uint)k.y << 16) + (uint)k.x;
    }
}
