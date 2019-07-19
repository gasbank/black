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
        static void AreEqual(int actual, int expected) {
            Debug.Assert(actual == expected, $"Expected: {expected}, Actual: {actual}");
        }

        static void Main(string[] args) {
            //MaxSubRect.TestCode();
            var beginIndex = 0;
            var endIndex = 0;
            var area = 0;
            area = MaxSubRect.MaxHist(new int[] { 100, 0, 6, 2, 5, 4, 5, 1, 6 }, out beginIndex, out endIndex);
            AreEqual(area, 100);
            AreEqual(beginIndex, 0);
            AreEqual(endIndex, 1);
            area = MaxSubRect.MaxHist(new int[] { 6, 2, 5, 4, 5, 1, 6 }, out beginIndex, out endIndex);
            AreEqual(area, 12);
            AreEqual(beginIndex, 2);
            AreEqual(endIndex, 5);
            area = MaxSubRect.MaxHist(new int[] { 6, 2, 5, 4, 5, 1, 6, 100 }, out beginIndex, out endIndex);
            AreEqual(area, 100);
            AreEqual(beginIndex, 7);
            AreEqual(endIndex, 8);
            area = MaxSubRect.MaxHist(new int[] { 100, 100, 6, 2, 5, 4, 5, 1, 6 }, out beginIndex, out endIndex);
            AreEqual(area, 200);
            AreEqual(beginIndex, 0);
            AreEqual(endIndex, 2);
            area = MaxSubRect.MaxHist(new int[] { 100, 100, 100, 100, 200, 200, 5, 1, 6 }, out beginIndex, out endIndex);
            AreEqual(area, 600);
            AreEqual(beginIndex, 0);
            AreEqual(endIndex, 6);
            area = MaxSubRect.MaxHist(new int[] { 1 }, out beginIndex, out endIndex);
            AreEqual(area, 1);
            AreEqual(beginIndex, 0);
            AreEqual(endIndex, 1);
            area = MaxSubRect.MaxHist(new int[] { 5, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, out beginIndex, out endIndex);
            AreEqual(area, 10);
            AreEqual(beginIndex, 0);
            AreEqual(endIndex, 10);
            area = MaxSubRect.MaxHist(new int[] { 1, 1, 1, 1, 1, 5, 1, 1, 1, 1 }, out beginIndex, out endIndex);
            AreEqual(area, 10);
            AreEqual(beginIndex, 0);
            AreEqual(endIndex, 10);
            area = MaxSubRect.MaxHist(new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 5, 1 }, out beginIndex, out endIndex);
            AreEqual(area, 10);
            AreEqual(beginIndex, 0);
            AreEqual(endIndex, 10);
            area = MaxSubRect.MaxHist(new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 5 }, out beginIndex, out endIndex);
            AreEqual(area, 10);
            AreEqual(beginIndex, 0);
            AreEqual(endIndex, 10);
            area = MaxSubRect.MaxHist(new int[] { 1, 1, 5 }, out beginIndex, out endIndex);
            AreEqual(area, 5);
            AreEqual(beginIndex, 2);
            AreEqual(endIndex, 3);
        }

        static void Main2(string[] args) {
            var sourcePngFileName = "/Users/kimgeoyeob/black/Art/190527_Colored.png";
            //var sourcePngFileName = "/Users/kimgeoyeob/black/Assets/Sprites/190717_8x8_Colored.png";
            using (Image<Rgba32> image = Image.Load(sourcePngFileName)) {
                var whiteCount = 0;
                Dictionary<Rgba32, int> pixelCountByColor = new Dictionary<Rgba32, int>();
                Dictionary<Vector2Int, Rgba32> islandColorByMinPoint = new Dictionary<Vector2Int, Rgba32>();
                Dictionary<Vector2Int, int> islandPixelAreaByMinPoint = new Dictionary<Vector2Int, int>();
                Dictionary<Rgba32, int> islandCountByColor = new Dictionary<Rgba32, int>();
                Dictionary<int, int> islandCountByPixelArea = new Dictionary<int, int>();
                
                for (int h = 0; h < image.Height; h++) {
                    for (int w = 0; w < image.Width; w++) {
                        var pixelColor = image[w, h];
                        if (pixelColor == Rgba32.White) {
                            whiteCount++;
                        }
                        IncreaseCountOfDictionaryValue(pixelCountByColor, pixelColor);

                        if (pixelColor == Rgba32.Black) {

                        } else {
                            var fillMinPoint = FloodFill.Execute(image, new Vector2Int(w, h), pixelColor, Rgba32.Black, out var pixelArea);
                            if (fillMinPoint != new Vector2Int(image.Width, image.Height)) {
                                islandColorByMinPoint[fillMinPoint] = pixelColor;
                                islandPixelAreaByMinPoint[fillMinPoint] = pixelArea;
                                IncreaseCountOfDictionaryValue(islandCountByPixelArea, pixelArea);
                                IncreaseCountOfDictionaryValue(islandCountByColor, pixelColor);
                            } else {
                                throw new Exception("Invalid fill min point!");
                            }
                        }
                    }
                }
                image.Save("bar.jpg"); // Automatic encoder selected based on extension.
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
                        rgba = GetC(islandColorByMinPoint[kv.Key])
                    };
                }

                using (var stream = File.Create("../Assets/Island Data/" + imageName + ".bytes")) {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, stageData);
                    stream.Close();
                }
            }
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