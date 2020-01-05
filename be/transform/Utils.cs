using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;

namespace Black
{
    class Utils
    {

        public static void IncreaseCountOfDictionaryValue<T>(Dictionary<T, int> pixelCountByColor, T pixel)
        {
            pixelCountByColor.TryGetValue(pixel, out var currentCount);
            pixelCountByColor[pixel] = currentCount + 1;
        }

        public static Rgba32 GetPixelClamped(Image<Rgba32> image, int x, int y)
        {
            return image[Math.Clamp(x, 0, image.Width - 1), Math.Clamp(y, 0, image.Height - 1)];
        }

        public static Rgba32 Average(Image<Rgba32> image, int x, int y, int radius)
        {
            Vector4 sum = Vector4.Zero;
            for (int h = y - radius; h <= y + radius; h++)
            {
                for (int w = x - radius; w <= x + radius; w++)
                {
                    var c = GetPixelClamped(image, w, h);
                    sum += c.ToVector4();
                }
            }
            var d = (radius * 2 + 1) * (radius * 2 + 1);
            var avg = new Rgba32(sum / d);
            return avg;
        }

        public static ulong GetRectRange(int v1, int v2, int v3, int v4)
        {
            return (ulong)((ulong)v1 + ((ulong)v2 << 16) + ((ulong)v3 << 32) + ((ulong)v4 << 48));
        }

        public static uint Rgba32ToUInt32(Rgba32 v)
        {
            return v.PackedValue;
            //return ((uint)v.A << 24) + ((uint)v.B << 16) + ((uint)v.G << 8) + (uint)v.R;
        }

        public static Rgba32 UInt32ToRgba32(uint v)
        {
            return new Rgba32(v);
        }

        public static uint Vector2IntToUInt32(Vector2Int k)
        {
            return ((uint)k.y << 16) + (uint)k.x;
        }

        public static Vector2Int UInt32ToVector2Int(uint v)
        {
            return new Vector2Int((int)(v & 0xffff), (int)(v >> 16));
        }
    }
}
