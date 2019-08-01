using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

namespace black_dev_tools {
    class Program {

        static void Main2(string[] args) {
            MaxSubRect.TestMaxHist();
            MaxSubRect.TestMaxRectangle();
        }

        static void Main(string[] args) {
            var sourcePngFileName = "/Users/kimgeoyeob/black/Art/colored/190527_Flowers_Colored.png";
            //var sourcePngFileName = "/Users/kimgeoyeob/black/Assets/Sprites/190719_128x128_Colored.png";
            //var sourcePngFileName = "/Users/kimgeoyeob/black/Assets/Sprites/190717_8x8_Colored.png";
            using (Image<Rgba32> image = Image.Load(sourcePngFileName)) {
                var whiteCount = 0;
                Dictionary<Rgba32, int> pixelCountByColor = new Dictionary<Rgba32, int>();
                Dictionary<Vector2Int, Rgba32> islandColorByMinPoint = new Dictionary<Vector2Int, Rgba32>();
                Dictionary<Vector2Int, int> islandPixelAreaByMinPoint = new Dictionary<Vector2Int, int>();
                Dictionary<Rgba32, int> islandCountByColor = new Dictionary<Rgba32, int>();
                Dictionary<int, int> islandCountByPixelArea = new Dictionary<int, int>();
                Dictionary<uint, ulong> maxRectByMinPoint = new Dictionary<uint, ulong>();

                for (int h = 0; h < image.Height; h++) {
                    for (int w = 0; w < image.Width; w++) {
                        var pixelColor = image[w, h];
                        if (pixelColor == Rgba32.White) {
                            whiteCount++;
                        }
                        IncreaseCountOfDictionaryValue(pixelCountByColor, pixelColor);

                        if (pixelColor == Rgba32.Black) {

                        } else {
                            List<Vector2Int> points = new List<Vector2Int>();
                            var fillMinPoint = FloodFill.Execute(image, new Vector2Int(w, h), pixelColor, Rgba32.Black, out var pixelArea, points);
                            if (fillMinPoint != new Vector2Int(image.Width, image.Height)) {
                                islandColorByMinPoint[fillMinPoint] = pixelColor;
                                islandPixelAreaByMinPoint[fillMinPoint] = pixelArea;
                                IncreaseCountOfDictionaryValue(islandCountByPixelArea, pixelArea);
                                IncreaseCountOfDictionaryValue(islandCountByColor, pixelColor);
                                var xMax = points.Max(e => e.x);
                                var xMin = points.Min(e => e.x);
                                var yMax = points.Max(e => e.y);
                                var yMin = points.Min(e => e.y);
                                var subRectW = xMax - xMin + 1;
                                var subRectH = yMax - yMin + 1;
                                var A = Enumerable.Range(0, subRectH).Select(e => new int[subRectW]).ToArray();
                                foreach (var point in points) {
                                    A[point.y - yMin][point.x - xMin] = 1;
                                }
                                var area = MaxSubRect.MaxRectangle(subRectH, subRectW, A, out var beginIndexR, out var endIndexR, out var beginIndexC, out var endIndexC);
                                Console.WriteLine($"Sub Rect: area:{area} [({yMin + beginIndexR},{xMin + beginIndexC})-({yMin + endIndexR},{xMin + endIndexC})]");
                                maxRectByMinPoint[GetP(fillMinPoint)] = GetRectRange(
                                    xMin + beginIndexC,
                                    yMin + beginIndexR,
                                    xMin + endIndexC,
                                    yMin + endIndexR);
                            } else {
                                throw new Exception("Invalid fill min point!");
                            }
                        }
                    }
                }
                //image.Save("bar.jpg"); // Automatic encoder selected based on extension.
                Console.WriteLine($"Total Pixel Count: {image.Width * image.Height}");
                Console.WriteLine($"White Count: {whiteCount}");

                var islandIndex = 0;
                foreach (var kv in islandColorByMinPoint) {
                    Console.WriteLine($"Island #{islandIndex} fillMinPoint={kv.Key}, color={kv.Value}, area={islandPixelAreaByMinPoint[kv.Key]}");
                    islandIndex++;
                }

                var colorCountIndex = 0;
                foreach (var kv in pixelCountByColor) {
                    islandCountByColor.TryGetValue(kv.Key, out var islandCount);
                    Console.WriteLine($"Color #{colorCountIndex} {kv.Key}: pixelCount={kv.Value}, islandCount={islandCount}");
                    colorCountIndex++;
                }

                var pixelAreaCountIndex = 0;
                foreach (var kv in islandCountByPixelArea.OrderByDescending(kv => kv.Key)) {
                    Console.WriteLine($"Pixel Area #{pixelAreaCountIndex} {kv.Key}: islandCount={kv.Value}");
                    pixelAreaCountIndex++;
                }

                Dictionary<uint, uint> islandColorByMinPointPrimitive = new Dictionary<uint, uint>();
                foreach (var kv in islandColorByMinPoint) {
                    var p = GetP(kv.Key);
                    var c = GetC(kv.Value);
                    islandColorByMinPointPrimitive[p] = c;
                }

                var imageName = Path.GetFileNameWithoutExtension(sourcePngFileName);

                var stageData = new StageData();
                foreach (var kv in islandPixelAreaByMinPoint) {
                    var p = GetP(kv.Key);
                    stageData.islandDataByMinPoint[p] = new IslandData {
                        pixelArea = islandPixelAreaByMinPoint[kv.Key],
                        rgba = GetC(islandColorByMinPoint[kv.Key]),
                        maxRect = maxRectByMinPoint[p],
                    };
                }

                var outputPath = Path.Combine("..", "Assets", "Island Data", imageName + ".bytes");
                using (var stream = File.Create(outputPath)) {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, stageData);
                    stream.Close();
                }

                Console.WriteLine($"{stageData.islandDataByMinPoint.Count} islands loaded.");
                Console.WriteLine($"Written to {outputPath}");
            }
        }

        private static ulong GetRectRange(int v1, int v2, int v3, int v4) {
            return (ulong)((ulong)v1 + ((ulong)v2 << 16) + ((ulong)v3 << 32) + ((ulong)v4 << 48));
        }

        private static uint GetC(Rgba32 v) {
            return ((uint)v.A << 24) + ((uint)v.B << 16) + ((uint)v.G << 8) + (uint)v.R;
        }

        private static uint GetP(Vector2Int k) {
            return ((uint)k.y << 16) + (uint)k.x;
        }

        private static void IncreaseCountOfDictionaryValue<T>(Dictionary<T, int> pixelCountByColor, T pixel) {
            pixelCountByColor.TryGetValue(pixel, out var currentCount);
            pixelCountByColor[pixel] = currentCount + 1;
        }
    }
}