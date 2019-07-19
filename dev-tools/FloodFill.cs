using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace black_dev_tools {
    partial class FloodFill {
        static bool ColorMatch(Rgba32 a, Rgba32 b) {
            return a == b;
        }

        static Rgba32 GetPixel(Image<Rgba32> bitmap, int x, int y) {
            return bitmap[x, y];
        }

        static void SetPixel(Image<Rgba32> bitmap, int x, int y, Rgba32 c) {
            bitmap[x, y] = c;
        }

        public static Vector2Int Execute(Image<Rgba32> bitmap, Vector2Int pt, Rgba32 targetColor, Rgba32 replacementColor, out int pixelArea, List<Vector2Int> points) {
            Queue<Vector2Int> q = new Queue<Vector2Int>();
            q.Enqueue(pt);
            var fillMinPoint = new Vector2Int(bitmap.Width, bitmap.Height);
            pixelArea = 0;
            while (q.Count > 0) {
                var n = q.Dequeue();
                if (!ColorMatch(GetPixel(bitmap, n.x, n.y), targetColor))
                    continue;
                Vector2Int w = n, e = new Vector2Int(n.x + 1, n.y);
                while ((w.x >= 0) && ColorMatch(GetPixel(bitmap, w.x, w.y), targetColor)) {
                    SetPixel(bitmap, w.x, w.y, replacementColor);
                    UpdateFillMinPoint(ref fillMinPoint, w);
                    points.Add(w);
                    pixelArea++;
                    if ((w.y > 0) && ColorMatch(GetPixel(bitmap, w.x, w.y - 1), targetColor))
                        q.Enqueue(new Vector2Int(w.x, w.y - 1));
                    if ((w.y < bitmap.Height - 1) && ColorMatch(GetPixel(bitmap, w.x, w.y + 1), targetColor))
                        q.Enqueue(new Vector2Int(w.x, w.y + 1));
                    w.x--;
                }
                while ((e.x <= bitmap.Width - 1) && ColorMatch(GetPixel(bitmap, e.x, e.y), targetColor)) {
                    SetPixel(bitmap, e.x, e.y, replacementColor);
                    points.Add(e);
                    UpdateFillMinPoint(ref fillMinPoint, e);
                    pixelArea++;
                    if ((e.y > 0) && ColorMatch(GetPixel(bitmap, e.x, e.y - 1), targetColor))
                        q.Enqueue(new Vector2Int(e.x, e.y - 1));
                    if ((e.y < bitmap.Height - 1) && ColorMatch(GetPixel(bitmap, e.x, e.y + 1), targetColor))
                        q.Enqueue(new Vector2Int(e.x, e.y + 1));
                    e.x++;
                }
            }
            return fillMinPoint;
        }

        private static void UpdateFillMinPoint(ref Vector2Int fillMinPoint, Vector2Int w) {
            if (fillMinPoint.x > w.x || (fillMinPoint.x == w.x && fillMinPoint.y > w.y)) {
                fillMinPoint = w;
            }
        }
    }
}