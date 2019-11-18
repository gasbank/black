using System;
using System.Collections.Generic;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace black_dev_tools {
    class Program {

        static void Main2(string[] args) {
            MaxSubRect.TestMaxHist();
            MaxSubRect.TestMaxRectangle();
        }

        static void Main(string[] args) {
            if (args.Length != 2) {
                Console.Out.WriteLine("Provide only two arguments [Input.png] [Stage Name]");
                return;
            }

            var sourcePngFileName = args[0]; //"../Art/colored/Necklace.png";
            var stageName = args[1];// Path.GetFileNameWithoutExtension(sourcePngFileName);
            var outputDir = Path.Combine("..", "Assets", "Stages", stageName);
            Directory.CreateDirectory(outputDir);

            // 완벽한 검은색은 Outline, 그 이외의 모든 색은 흰색으로 칠해진
            // 파일을 만든다.
            WriteOutlineImageFile(sourcePngFileName, stageName, outputDir);

            // 이미지 파일을 열어봅시다~
            using (Image<Rgba32> image = Image.Load(sourcePngFileName)) {
                // 색상 별 픽셀 수
                Dictionary<Rgba32, int> pixelCountByColor = new Dictionary<Rgba32, int>();
                // Min Point 별 (섬 별) 섬 색상
                Dictionary<Vector2Int, Rgba32> islandColorByMinPoint = new Dictionary<Vector2Int, Rgba32>();
                // Min Point 별 (섬 별) 섬 픽셀 수(면적)
                Dictionary<Vector2Int, int> islandPixelAreaByMinPoint = new Dictionary<Vector2Int, int>();
                // 색상 별 섬 수
                Dictionary<Rgba32, int> islandCountByColor = new Dictionary<Rgba32, int>();
                // 픽셀 수(면적) 별 섬 수
                Dictionary<int, int> islandCountByPixelArea = new Dictionary<int, int>();
                // Min Point 별 (섬 별) Max Rect
                Dictionary<uint, ulong> maxRectByMinPoint = new Dictionary<uint, ulong>();

                // 각 픽셀에 대해서 반복한다.
                for (int h = 0; h < image.Height; h++) {
                    for (int w = 0; w < image.Width; w++) {
                        var pixelColor = image[w, h];

                        if (pixelColor == Rgba32.Black) {
                            // 경계선 색상(검은색)이면 할 일이 없다.
                        } else {
                            // (w, h) 좌표부터 검은색이 아닌 색을 검은색으로 채우면서 픽셀 수집한다.
                            // 수집한 모든 픽셀은 points에, points의 min point는 반환값으로 받는다.
                            var coord = new Vector2Int(w, h);
                            var fillMinPoint = FloodFill.ExecuteFillIfNotBlack(image, coord, Rgba32.Black, out var pixelArea, out var points, out var originalColors);
                            if (fillMinPoint != new Vector2Int(image.Width, image.Height)) {
                                if (originalColors.Count > 1) {
                                    // 한 섬에 색상이 여러 가지라면 가장 많은 색상이 최종 색깔이 되도록 하자.
                                    // 주로 경계선 주변에서 경계선과 섬 색깔이 블렌딩되면서 다른 색깔이 되는 패턴이다.
                                    //var prominentColor = originalColors.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
                                    var prominentColor = originalColors.OrderByDescending(e => e.Value).Where(e => e.Key != Rgba32.White).First().Key;

                                    pixelColor = prominentColor;

                                    // foreach (var originalColor in originalColors) {
                                    //     Console.WriteLine($"{originalColor.Key} = {originalColor.Value}");
                                    // }
                                    // throw new Exception($"Island color is not uniform! It has {originalColors.Count} colors in it! coord={coord}");
                                }
                                if (originalColors.Count == 0) {
                                    throw new Exception($"Island color is empty. Is this possible?");
                                }

                                if (pixelColor == Rgba32.White) {
                                    throw new Exception($"Island color is WHITE?! Fix it!");
                                }

                                IncreaseCountOfDictionaryValue(pixelCountByColor, pixelColor);

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

                Console.WriteLine($"Total Pixel Count: {image.Width * image.Height}");
                pixelCountByColor.TryGetValue(Rgba32.White, out var whiteCount);
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
                    if (kv.Key == Rgba32.White) {
                        throw new Exception("Palette color should not be white!");
                    }
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

                // 너무 작은 섬이 있다면 원본 파일(sourcePngFileName)에다가
                // 작은 섬을 제거한(검은색으로 칠한) 상태를 만들어 원본 위치와 같은 곳에 쓴다.
                // 그리고 프로그램 종료시킨다.
                bool smallIslandRemoved = false;
                using (Image<Rgba32> outlineImage = Image.Load(sourcePngFileName)) {
                    
                    foreach (var island in islandPixelAreaByMinPoint) {
                        if (island.Value < 4 * 4) {

                            var fillMinPoint = FloodFill.ExecuteFillIfNotBlack(outlineImage, island.Key, Rgba32.Black, out var pixelArea, out var points, out var originalColors);
                            if (fillMinPoint != new Vector2Int(outlineImage.Width, outlineImage.Height) && pixelArea == island.Value) {
                                smallIslandRemoved = true;
                            } else {
                                Console.WriteLine("Logic Error!");
                            }
                        }
                    }

                    if (smallIslandRemoved) {
                        // 원래 파일에 덮어 쓴다!
                        using (var stream = new FileStream(sourcePngFileName + "-SmallIslandRemoved.png", FileMode.Create)) {
                            outlineImage.SaveAsPng(stream);
                            stream.Close();
                        }

                        Console.WriteLine("TOO SMALL ISLANDS DETECTED. Small Island Removed file created.");
                        Console.WriteLine("Abort...");
                        return;
                    }
                }

                var stageData = new StageData();
                foreach (var kv in islandPixelAreaByMinPoint) {
                    var p = GetP(kv.Key);
                    stageData.islandDataByMinPoint[p] = new IslandData {
                        pixelArea = islandPixelAreaByMinPoint[kv.Key],
                        rgba = GetC(islandColorByMinPoint[kv.Key]),
                        maxRect = maxRectByMinPoint[p],
                    };
                }


                var outputPath = Path.Combine(outputDir, stageName + ".bytes");
                using (var stream = File.Create(outputPath)) {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, stageData);
                    stream.Close();
                }

                Console.WriteLine($"{stageData.islandDataByMinPoint.Count} islands loaded.");
                Console.WriteLine($"Written to {outputPath}");

                
            }
        }

        private static void WriteOutlineImageFile(string sourcePngFileName, string stageName, string outputDir) {
            using (Image<Rgba32> image = Image.Load(sourcePngFileName)) {
                for (int h = 0; h < image.Height; h++) {
                    for (int w = 0; w < image.Width; w++) {
                        var pixelColor = image[w, h];
                        if (pixelColor != Rgba32.Black) {
                            image[w, h] = Rgba32.White;
                        }
                    }
                }

                var outputPath = Path.Combine(outputDir, stageName + "-Outline.png");
                using (var stream = new FileStream(outputPath, FileMode.Create)) {
                    image.SaveAsPng(stream);
                    stream.Close();
                }
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

        public static void IncreaseCountOfDictionaryValue<T>(Dictionary<T, int> pixelCountByColor, T pixel) {
            pixelCountByColor.TryGetValue(pixel, out var currentCount);
            pixelCountByColor[pixel] = currentCount + 1;
        }
    }
}