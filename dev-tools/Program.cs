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

namespace black_dev_tools {
    class Program {
        static void Main(string[] args) {
            using (Image<Rgba32> image = Image.Load("/Users/kimgeoyeob/black/Art/190527_Colored.png")) {
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
            }
        }

        private static void IncreaseCountOfDictionaryValue<T>(Dictionary<T, int> pixelCountByColor, T pixel) {
            pixelCountByColor.TryGetValue(pixel, out var currentCount);
            pixelCountByColor[pixel] = currentCount + 1;
        }
    }
}