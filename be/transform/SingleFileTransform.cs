using System;
using System.Collections.Generic;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using System.Runtime.Serialization;

namespace Black
{
    public class SingleFileTransform : BaseTransform
    {
        public string Mode { get; set; }
        public string StartFileName { get; set; }
        public int MaxColor { get; set; }

        public override void Run()
        {
            if (Mode == "otb")
            {
                ExecuteOutlineToBlack(StartFileName);
            }
            else if (Mode == "fsnb")
            {
                var otbFileName = ExecuteOutlineToBlack(StartFileName);
                ExecuteFillSmallNotBlack(otbFileName);
            }
            else if (Mode == "q")
            {
                var otbFileName = ExecuteOutlineToBlack(StartFileName);
                ExecuteQuantize(StartFileName, MaxColor);
            }
            else if (Mode == "fots")
            {
                var otbFileName = ExecuteOutlineToBlack(StartFileName);
                var fsnbFileName = ExecuteFillSmallNotBlack(otbFileName);
                var qFileName = ExecuteQuantize(StartFileName, MaxColor);
                ExecuteFlattenedOutlineToSource(qFileName, fsnbFileName);
            }
            else if (Mode == "di")
            {
                var otbFileName = ExecuteOutlineToBlack(StartFileName);
                var fsnbFileName = ExecuteFillSmallNotBlack(otbFileName);
                var qFileName = ExecuteQuantize(StartFileName, MaxColor);
                var fotsFileName = ExecuteFlattenedOutlineToSource(qFileName, fsnbFileName);
                ExecuteDetermineIsland(fotsFileName, StartFileName);
            }
            else if (Mode == "dit")
            {
                var otbFileName = ExecuteOutlineToBlack(StartFileName);
                var fsnbFileName = ExecuteFillSmallNotBlack(otbFileName);
                var qFileName = ExecuteQuantize(StartFileName, MaxColor);
                var fotsFileName = ExecuteFlattenedOutlineToSource(qFileName, fsnbFileName);
                var bytesFileName = ExecuteDetermineIsland(fotsFileName, StartFileName);
                ExecuteDetermineIslandTest(fsnbFileName, bytesFileName);
                var bbFileName = ExecuteBoxBlur(fsnbFileName, 2);
                ExecuteSdf(bbFileName);
            }
            else
            {
                Console.Out.WriteLine($"Unknown mode provided: {Mode}");
                return;
            }
            Console.Out.WriteLine("Completed.");
        }

        public static SingleFileTransform FromArgs(string[] args)
        {
            if (args.Length < 3)
            {
                throw new ArgumentException("Provide three arguments: [mode] [Input.png/jpg] [max colors] <output path replace from> <output path replace to>");
            }

            var mode = args[0];
            var startFileName = args[1];
            int.TryParse(args[2], out var maxColor);
            maxColor = Math.Clamp(maxColor, 3, 100);

            var outputPathReplaceFrom = "";
            if (args.Length >= 4)
            {
                outputPathReplaceFrom = args[3];
            }

            var outputPathReplaceTo = "";
            if (args.Length >= 5)
            {
                outputPathReplaceTo = args[4];
            }
            return new SingleFileTransform
            {
                Mode = mode,
                StartFileName = startFileName,
                MaxColor = maxColor,
                OutputPathReplaceFrom = outputPathReplaceFrom,
                OutputPathReplaceTo = outputPathReplaceTo,
            };
        }

        // 애매한 검은색을 완전한 검은색으로 바꾼다.
        private string ExecuteOutlineToBlack(string sourceFileName, int threshold = 20)
        {
            var targetFileName = AppendToFileName(sourceFileName, "-OTB");
            using (var image = Image.Load<Rgba32>(sourceFileName))
            {
                for (int h = 0; h < image.Height; h++)
                {
                    for (int w = 0; w < image.Width; w++)
                    {
                        var pixelColor = image[w, h];
                        if (pixelColor.R <= threshold && pixelColor.G <= threshold && pixelColor.B <= threshold)
                        {
                            image[w, h] = Rgba32.Black;
                        }
                        else
                        {
                            image[w, h] = Rgba32.White;
                        }
                    }
                }
                Directory.CreateDirectory(Path.GetDirectoryName(targetFileName));
                using (var stream = new FileStream(targetFileName, FileMode.Create))
                {
                    image.SaveAsPng(stream);
                    stream.Close();
                }
            }
            return targetFileName;
        }

        // 아주 작은 비검은색을 검은색으로 메운다.
        private string ExecuteFillSmallNotBlack(string sourceFileName, int threshold = 4 * 4 * 4)
        {
            var targetFileName = AppendToFileName(sourceFileName, "-FSNB");
            // Min Point 별 (섬 별) 섬 픽셀 수(면적)
            Dictionary<Vector2Int, int> islandPixelAreaByMinPoint = new Dictionary<Vector2Int, int>();
            using (var image = Image.Load<Rgba32>(sourceFileName))
            {
                // 각 픽셀에 대해서 반복한다.
                for (int h = 0; h < image.Height; h++)
                {
                    for (int w = 0; w < image.Width; w++)
                    {
                        var pixelColor = image[w, h];
                        if (pixelColor == Rgba32.Black)
                        {
                            // 경계선 색상(검은색)이면 할 일이 없다.
                        }
                        else
                        {
                            // (w, h) 좌표부터 검은색이 아닌 색을 검은색으로 채우면서 픽셀 수집한다.
                            // 수집한 모든 픽셀은 points에, points의 min point는 반환값으로 받는다.
                            var coord = new Vector2Int(w, h);
                            var fillMinPoint = FloodFill.ExecuteFillIfNotBlack(image, coord, Rgba32.Black, out var pixelArea, out var points, out var originalColors);
                            if (fillMinPoint != new Vector2Int(image.Width, image.Height))
                            {
                                islandPixelAreaByMinPoint[fillMinPoint] = pixelArea;
                            }
                            else
                            {
                                throw new Exception("Invalid fill min point!");
                            }
                        }
                    }
                }
            }

            // 메모리상 image 변수는 직전 과정에서 변경되었으므로, 다시 읽어들이자.
            // 여기서부터 본격적으로 작은 비검정색칸을 검정색칸으로 채운다.
            using (var image = Image.Load<Rgba32>(sourceFileName))
            {
                foreach (var island in islandPixelAreaByMinPoint)
                {
                    if (island.Value < threshold)
                    {
                        var fillMinPoint = FloodFill.ExecuteFillIfNotBlack(image, island.Key, Rgba32.Black, out var pixelArea, out var points, out var originalColors);
                        if (fillMinPoint != new Vector2Int(image.Width, image.Height) && pixelArea == island.Value)
                        {
                        }
                        else
                        {
                            Console.WriteLine("Logic Error!");
                        }
                    }
                }

                // 그리고 저장!
                Directory.CreateDirectory(Path.GetDirectoryName(targetFileName));
                using (var stream = new FileStream(targetFileName, FileMode.Create))
                {
                    image.SaveAsPng(stream);
                    stream.Close();
                }
            }
            return targetFileName;
        }

        // 이미지를 팔레트화시킨다.
        // 팔레트에는 반드시 검은색과 흰색은 빠지도록 한다.
        private string ExecuteQuantize(string sourceFileName, int maxColor = 30)
        {
            var targetFileName = AppendToFileName(sourceFileName, "-Q");
            var quantizer = new WuQuantizer(null, maxColor);
            var config = Configuration.Default;

            using (var image = Image.Load<Rgba32>(sourceFileName))
            using (var quantizedResult = quantizer.CreateFrameQuantizer<Rgba32>(config).QuantizeFrame(image.Frames[0]))
            {
                var quantizedPalette = new List<Color>();
                foreach (var p in quantizedResult.Palette.Span)
                {
                    var c = new Color(p);
                    // 팔레트에 흰색과 검은색은 반드시 빠지도록 한다.
                    if (c != Color.White && c != Color.Black)
                    {
                        quantizedPalette.Add(c);
                    }
                }

                using (var stream = new FileStream(targetFileName, FileMode.Create))
                {
                    var encoder = new SixLabors.ImageSharp.Formats.Png.PngEncoder
                    {
                        Quantizer = new PaletteQuantizer(quantizedPalette.ToArray(), false),
                        ColorType = SixLabors.ImageSharp.Formats.Png.PngColorType.Palette,
                    };
                    image.SaveAsPng(stream, encoder);
                    stream.Close();
                }

                return targetFileName;
            }
        }

        // sourceFileName 위에 outlineFileName을 얹는다. 얹을 때 완전 검은 색깔만 얹는다.
        private string ExecuteFlattenedOutlineToSource(string sourceFileName, string outlineFileName)
        {
            var targetFileName = AppendToFileName(outlineFileName, "-FOTS");
            using (var sourceImage = Image.Load<Rgba32>(sourceFileName))
            using (var outlineImage = Image.Load<Rgba32>(outlineFileName))
            {
                if (sourceImage.Size() != outlineImage.Size())
                {
                    throw new Exception("Image dimension differs.");
                }

                for (int h = 0; h < sourceImage.Height; h++)
                {
                    for (int w = 0; w < sourceImage.Width; w++)
                    {
                        if (outlineImage[w, h] == Rgba32.Black)
                        {
                            sourceImage[w, h] = Rgba32.Black;
                        }
                    }
                }

                using (var stream = new FileStream(targetFileName, FileMode.Create))
                {
                    sourceImage.SaveAsPng(stream);
                    stream.Close();
                }
            }
            return targetFileName;
        }

        // 입력 이미지로 섬 데이터를 만든다.
        // 섬 데이터는 유니티에서 사용하게 된다.
        private string ExecuteDetermineIsland(string sourceFileName, string startFileName)
        {
            // 이미지 파일을 열어봅시다~
            using (var image = Image.Load<Rgba32>(sourceFileName))
            {
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
                for (int h = 0; h < image.Height; h++)
                {
                    for (int w = 0; w < image.Width; w++)
                    {
                        var pixelColor = image[w, h];

                        if (pixelColor == Rgba32.Black)
                        {
                            // 경계선 색상(검은색)이면 할 일이 없다.
                        }
                        else
                        {
                            // (w, h) 좌표부터 검은색이 아닌 색을 검은색으로 채우면서 픽셀 수집한다.
                            // 수집한 모든 픽셀은 points에, points의 min point는 반환값으로 받는다.
                            var coord = new Vector2Int(w, h);
                            var fillMinPoint = FloodFill.ExecuteFillIfNotBlack(image, coord, Rgba32.Black, out var pixelArea, out var points, out var originalColors);
                            if (fillMinPoint != new Vector2Int(image.Width, image.Height))
                            {
                                if (originalColors.Count > 1)
                                {
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
                                if (originalColors.Count == 0)
                                {
                                    throw new Exception($"Island color is empty. Is this possible?");
                                }

                                if (pixelColor == Rgba32.White)
                                {
                                    throw new Exception($"Island color is WHITE?! Fix it!");
                                }

                                Utils.IncreaseCountOfDictionaryValue(pixelCountByColor, pixelColor);

                                islandColorByMinPoint[fillMinPoint] = pixelColor;
                                islandPixelAreaByMinPoint[fillMinPoint] = pixelArea;
                                Utils.IncreaseCountOfDictionaryValue(islandCountByPixelArea, pixelArea);
                                Utils.IncreaseCountOfDictionaryValue(islandCountByColor, pixelColor);
                                var xMax = points.Max(e => e.x);
                                var xMin = points.Min(e => e.x);
                                var yMax = points.Max(e => e.y);
                                var yMin = points.Min(e => e.y);
                                var subRectW = xMax - xMin + 1;
                                var subRectH = yMax - yMin + 1;
                                var A = Enumerable.Range(0, subRectH).Select(e => new int[subRectW]).ToArray();
                                foreach (var point in points)
                                {
                                    A[point.y - yMin][point.x - xMin] = 1;
                                }
                                var area = MaxSubRect.MaxRectangle(subRectH, subRectW, A, out var beginIndexR, out var endIndexR, out var beginIndexC, out var endIndexC);
                                Console.WriteLine($"Sub Rect: area:{area} [({yMin + beginIndexR},{xMin + beginIndexC})-({yMin + endIndexR},{xMin + endIndexC})]");
                                maxRectByMinPoint[Utils.Vector2IntToUInt32(fillMinPoint)] = Utils.GetRectRange(
                                    xMin + beginIndexC,
                                    yMin + beginIndexR,
                                    xMin + endIndexC,
                                    yMin + endIndexR);
                            }
                            else
                            {
                                throw new Exception("Invalid fill min point!");
                            }
                        }
                    }
                }

                Console.WriteLine($"Total Pixel Count: {image.Width * image.Height}");
                pixelCountByColor.TryGetValue(Rgba32.White, out var whiteCount);
                Console.WriteLine($"White Count: {whiteCount}");

                var islandIndex = 0;
                foreach (var kv in islandColorByMinPoint)
                {
                    Console.WriteLine($"Island #{islandIndex} fillMinPoint={kv.Key}, color={kv.Value}, area={islandPixelAreaByMinPoint[kv.Key]}");
                    islandIndex++;
                }

                var colorCountIndex = 0;
                foreach (var kv in pixelCountByColor)
                {
                    islandCountByColor.TryGetValue(kv.Key, out var islandCount);
                    Console.WriteLine($"Color #{colorCountIndex} {kv.Key}: pixelCount={kv.Value}, islandCount={islandCount}");
                    colorCountIndex++;
                    if (kv.Key == Rgba32.White)
                    {
                        throw new Exception("Palette color should not be white!");
                    }
                }

                var pixelAreaCountIndex = 0;
                foreach (var kv in islandCountByPixelArea.OrderByDescending(kv => kv.Key))
                {
                    Console.WriteLine($"Pixel Area #{pixelAreaCountIndex} {kv.Key}: islandCount={kv.Value}");
                    pixelAreaCountIndex++;
                }

                Dictionary<uint, uint> islandColorByMinPointPrimitive = new Dictionary<uint, uint>();
                foreach (var kv in islandColorByMinPoint)
                {
                    var p = Utils.Vector2IntToUInt32(kv.Key);
                    var c = Utils.Rgba32ToUInt32(kv.Value);
                    islandColorByMinPointPrimitive[p] = c;
                }

                var stageData = new StageData();
                foreach (var kv in islandPixelAreaByMinPoint)
                {
                    var p = Utils.Vector2IntToUInt32(kv.Key);
                    stageData.islandDataByMinPoint[p] = new IslandData
                    {
                        pixelArea = islandPixelAreaByMinPoint[kv.Key],
                        rgba = Utils.Rgba32ToUInt32(islandColorByMinPoint[kv.Key]),
                        maxRect = maxRectByMinPoint[p],
                    };
                }

                var outputPath = Path.ChangeExtension(startFileName, "bytes").Replace(OutputPathReplaceFrom, OutputPathReplaceTo);
                using (var stream = File.Create(outputPath))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, stageData);
                    stream.Close();
                }

                Console.WriteLine($"{stageData.islandDataByMinPoint.Count} islands loaded.");
                Console.WriteLine($"Written to {outputPath}");
                return outputPath;
            }
        }

        // 섬 데이터와 외곽선 데이터를 이용해 색칠을 자동으로 해 본다.
        // 색칠 후 이미지에 문제가 없는지 확인하기 위한 테스트 과정이다.
        private string ExecuteDetermineIslandTest(string sourceFileName, string bytesFileName)
        {
            var targetFileName = AppendToFileName(sourceFileName, "-DIT");
            StageData stageData = null;
            using (var bytesFileStream = new FileStream(bytesFileName, FileMode.Open))
            {
                var formatter = new BinaryFormatter();
                try
                {
                    // Deserialize the hashtable from the file and 
                    // assign the reference to the local variable.
                    stageData = (StageData)formatter.Deserialize(bytesFileStream);
                }
                catch (SerializationException e)
                {
                    Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                    throw;
                }
            }
            using (var image = Image.Load<Rgba32>(sourceFileName))
            {
                foreach (var island in stageData.islandDataByMinPoint)
                {
                    var minPoint = Utils.UInt32ToVector2Int(island.Key);
                    var targetColor = Utils.UInt32ToRgba32(island.Value.rgba);
                    var fillMinPoint = FloodFill.ExecuteFillIf(image, minPoint, Rgba32.White, targetColor, out var pixelArea, out var points, out var originalColors);
                    if (fillMinPoint != new Vector2Int(image.Width, image.Height) && pixelArea == island.Value.pixelArea)
                    {
                    }
                    else
                    {
                        Console.WriteLine("Logic Error!");
                    }
                }

                using (var stream = new FileStream(targetFileName, FileMode.Create))
                {
                    image.SaveAsPng(stream);
                    stream.Close();
                }
            }
            return targetFileName;
        }

    }
}