using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Numerics;
using System.Text.Json;
using SixLabors.ImageSharp.Formats.Png;

namespace black_dev_tools
{
    internal class IslandCountException : Exception
    {
    }

    internal static class Program
    {
        public static readonly Rgba32 Black = new(0, 0, 0, 255);
        public static readonly Rgba32 White = new(255, 255, 255, 255);
        static string outputPathReplaceFrom = "";
        static string outputPathReplaceTo = "";
        private static readonly JsonSerializerOptions SerOptions = new() { IncludeFields = true };

        // 너무 작은 칸은 검은색 경계선으로 취급한다.
        private const int IslandPixelAreaThreshold = 4 * 4 * 4;

        static void Main(string[] args)
        {
            if (args.Length <= 0)
            {
                Console.Out.WriteLine("Should provide at least one param: mode");
                return;
            }

            var mode = args[0];

            if (mode == "batch")
            {
                ProcessMultipleFiles(args);
            }
            else if (mode == "sdf")
            {
                ProcessSdf(args);
            }
            else
            {
                ProcessSingleFileAdaptiveOutlineThreshold(args);
            }
        }

        static void ProcessSingleFileAdaptiveOutlineThreshold(string[] args)
        {
            try
            {
                ProcessSingleFile(args, 20);
            }
            catch (IslandCountException)
            {
                ProcessSingleFile(args, 30);
            }
            catch (DirectoryNotFoundException e)
            {
                Console.Out.WriteLine($"Exception: {e.Message}");
            }
        }

        static void ProcessSdf(string[] args)
        {
            if (args.Length != 2 && args.Length != 4)
            {
                Console.Out.WriteLine(
                    "Provide three arguments: sdf [input path] <output path replace from> <output path replace to>");
                throw new ArgumentException();
            }

            var sourceFileName = args[1];

            if (args.Length >= 3)
            {
                outputPathReplaceFrom = args[2];
            }

            if (args.Length >= 4)
            {
                outputPathReplaceTo = args[3];
            }

            var bbFileName = ExecuteBoxBlur(sourceFileName, 2);
            ExecuteSdf(bbFileName);
        }

        static void ExecuteSdf(string sourceFileName)
        {
            Console.Out.WriteLine($"Running {nameof(ExecuteSdf)}");

            var targetFileName = AppendToFileName(sourceFileName, "-SDF");
            using var image = Image.Load<Rgba32>(sourceFileName);
            var targetImage = new Image<Rgba32>(image.Width, image.Height);

            SDFTextureGenerator.Generate(image, targetImage, 0, 3, 0, RGBFillMode.White);

            Directory.CreateDirectory(Path.GetDirectoryName(targetFileName));
            using var stream = new FileStream(targetFileName, FileMode.Create);
            targetImage.SaveAsPng(stream);
            stream.Close();
        }

        static Rgba32 GetPixelClamped(Image<Rgba32> image, int x, int y)
        {
            return image[Math.Clamp(x, 0, image.Width - 1), Math.Clamp(y, 0, image.Height - 1)];
        }

        static Rgba32 Average(Image<Rgba32> image, int x, int y, int radius)
        {
            var sum = Vector4.Zero;
            for (var h = y - radius; h <= y + radius; h++)
            {
                for (var w = x - radius; w <= x + radius; w++)
                {
                    var c = GetPixelClamped(image, w, h);
                    sum += c.ToVector4();
                }
            }

            var d = (radius * 2 + 1) * (radius * 2 + 1);
            var avg = new Rgba32(sum / d);
            return avg;
        }

        static string ExecuteBoxBlur(string sourceFileName, int radius)
        {
            Console.Out.WriteLine($"Running {nameof(ExecuteBoxBlur)}");

            var targetFileName = AppendToFileName(sourceFileName, "-BB");
            using var image = Image.Load<Rgba32>(sourceFileName);
            var targetImage = new Image<Rgba32>(image.Width, image.Height);

            for (var y = 0; y < image.Height; y++)
            {
                for (var x = 0; x < image.Width; x++)
                {
                    targetImage[x, y] = Average(image, x, y, radius);
                }
            }

            Directory.CreateDirectory(Path.GetDirectoryName(targetFileName));
            using var stream = new FileStream(targetFileName, FileMode.Create);
            targetImage.SaveAsPng(stream);
            stream.Close();

            return targetFileName;
        }

        static void ProcessMultipleFiles(string[] args)
        {
            if (args.Length != 2 && args.Length != 4)
            {
                Console.Out.WriteLine(
                    "Provide three arguments: batch [input path] <output path replace from> <output path replace to>");
                return;
            }

            var inputPath = args[1];

            if (args.Length >= 3)
            {
                outputPathReplaceFrom = args[2];
            }

            if (args.Length >= 4)
            {
                outputPathReplaceTo = args[3];
            }

            var pngFiles = Directory.GetFiles(inputPath, "*.png", SearchOption.AllDirectories);
            //var jpgFiles = Directory.GetFiles(inputPath, "*.jpg", SearchOption.AllDirectories);
            var inputFiles = pngFiles; // pngFiles.Concat(jpgFiles).ToArray();

            foreach (var inputFileName in inputFiles)
            {
                // @"C:\Users\gb\Google Drive\2020_컬러뮤지엄\02. Stage\01~10\진주귀걸이를한소녀.png"
                if (Regex.IsMatch(inputFileName, @"\\[0-9][0-9]~[0-9][0-9]\\"))
                {
                    Console.Out.WriteLine(inputFileName);

                    ProcessSingleFileAdaptiveOutlineThreshold(new[]
                    {
                        "dit",
                        inputFileName,
                        30.ToString(),
                        outputPathReplaceFrom,
                        outputPathReplaceTo,
                    });
                }
            }
        }

        static void ProcessSingleFile(string[] args, int outlineThreshold)
        {
            if (args.Length < 3)
            {
                Console.Out.WriteLine(
                    "Provide three arguments: [mode] [Input.png/jpg] [max colors] <output path replace from> <output path replace to>");
            }

            var mode = args[0];
            var startFileName = args[1]
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);

            int.TryParse(args[2], out var maxColor);
            maxColor = Math.Clamp(maxColor, 3, 100);

            if (args.Length >= 4)
            {
                outputPathReplaceFrom = args[3];
            }

            if (args.Length >= 5)
            {
                outputPathReplaceTo = args[4];
            }

            if (mode == "otb")
            {
                ExecuteOutlineToBlack(startFileName, outlineThreshold);
            }
            else if (mode == "fsnb")
            {
                var otbFileName = ExecuteOutlineToBlack(startFileName, outlineThreshold);
                ExecuteFillSmallNotBlack(otbFileName);
            }
            else if (mode == "q")
            {
                ExecuteOutlineToBlack(startFileName, outlineThreshold);
                ExecuteQuantize(startFileName, maxColor);
            }
            else if (mode == "fots")
            {
                var otbFileName = ExecuteOutlineToBlack(startFileName, outlineThreshold);
                var fsnbFileName = ExecuteFillSmallNotBlack(otbFileName);
                var qFileName = ExecuteQuantize(startFileName, maxColor);
                ExecuteFlattenedOutlineToSource(qFileName, fsnbFileName);
            }
            else if (mode == "di")
            {
                var otbFileName = ExecuteOutlineToBlack(startFileName, outlineThreshold);
                var fsnbFileName = ExecuteFillSmallNotBlack(otbFileName);
                var qFileName = ExecuteQuantize(startFileName, maxColor);
                var fotsFileName = ExecuteFlattenedOutlineToSource(qFileName, fsnbFileName);
                ExecuteDetermineIsland(fotsFileName, startFileName);
            }
            else if (mode == "dit")
            {
                var qFileName = ExecuteQuantize(startFileName, maxColor);
                var otbFileName = ExecuteOutlineToBlack(qFileName, outlineThreshold);
                var fsnbFileName = ExecuteFillSmallNotBlack(otbFileName);
                var fotsFileName = ExecuteFlattenedOutlineToSource(qFileName, fsnbFileName);
                var bytesFileName = ExecuteDetermineIsland(fotsFileName, startFileName);
                ExecuteDetermineIslandTest(fsnbFileName, bytesFileName);
                var bbFileName = ExecuteBoxBlur(fsnbFileName, 1);
                ExecuteSdf(bbFileName);
            }
            else
            {
                Console.Out.WriteLine($"Unknown mode provided: {mode}");
                return;
            }

            Console.Out.WriteLine("Completed.");
        }

        // 섬 데이터와 외곽선 데이터를 이용해 색칠을 자동으로 해 본다.
        // 색칠 후 이미지에 문제가 없는지 확인하기 위한 테스트 과정이다.
        static void ExecuteDetermineIslandTest(string sourceFileName, string bytesFileName)
        {
            Console.Out.WriteLine($"Running {nameof(ExecuteDetermineIslandTest)}");

            var targetFileName = AppendToFileName(sourceFileName, "-DIT");
            StageData stageData;
            using (var bytesFileStream = new FileStream(bytesFileName, FileMode.Open))
            {
                //var formatter = new BinaryFormatter();
                try
                {
                    // Deserialize the hashtable from the file and 
                    // assign the reference to the local variable.
                    //stageData = (StageData)formatter.Deserialize(bytesFileStream);
                    stageData = JsonSerializer.Deserialize<StageData>(bytesFileStream, SerOptions) ??
                                throw new NullReferenceException("StageData null");
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
                    var minPoint = UInt32ToVector2Int(island.Key);
                    var targetColor = UInt32ToRgba32(island.Value.rgba);
                    var fillMinPoint = FloodFill.ExecuteFillIf(image, minPoint, White, targetColor, out var pixelArea,
                        out _, out _);
                    if (fillMinPoint != new Vector2Int(image.Width, image.Height) &&
                        pixelArea == island.Value.pixelArea)
                    {
                    }
                    else
                    {
                        Console.WriteLine("Logic error in ExecuteDetermineIslandTest()!");
                    }
                }

                using (var stream = new FileStream(targetFileName, FileMode.Create))
                {
                    image.SaveAsPng(stream);
                    stream.Close();
                }
            }
        }

        // 입력 이미지로 섬 데이터를 만든다.
        // 섬 데이터는 유니티에서 사용하게 된다.
        static string ExecuteDetermineIsland(string sourceFileName, string startFileName)
        {
            Console.Out.WriteLine($"Running {nameof(ExecuteDetermineIsland)}");

            // 이미지 파일을 열어봅시다~
            using var image = Image.Load<Rgba32>(sourceFileName);
            // 색상 별 픽셀 수
            var pixelCountByColor = new Dictionary<Rgba32, int>();
            // Min Point 별 (섬 별) 섬 색상
            var islandColorByMinPoint = new Dictionary<Vector2Int, Rgba32>();
            // Min Point 별 (섬 별) 섬 픽셀 수(면적)
            var islandPixelAreaByMinPoint = new Dictionary<Vector2Int, int>();
            // 색상 별 섬 수
            var islandCountByColor = new Dictionary<Rgba32, int>();
            // 픽셀 수(면적) 별 섬 수
            var islandCountByPixelArea = new Dictionary<int, int>();
            // Min Point 별 (섬 별) Max Rect
            var maxRectByMinPoint = new Dictionary<uint, ulong>();

            // 각 픽셀에 대해서 반복한다.
            for (var h = 0; h < image.Height; h++)
            {
                for (var w = 0; w < image.Width; w++)
                {
                    var pixelColor = image[w, h];

                    if (pixelColor == Black)
                    {
                        // 경계선 색상(검은색)이면 할 일이 없다.
                    }
                    else
                    {
                        // (w, h) 좌표부터 검은색이 아닌 색을 검은색으로 채우면서 픽셀 수집한다.
                        // 수집한 모든 픽셀은 points에, points의 min point는 반환값으로 받는다.
                        var coord = new Vector2Int(w, h);
                        var fillMinPoint = FloodFill.ExecuteFillIfNotBlack(image, coord, Black, out var pixelArea,
                            out var points, out var originalColors);
                        if (fillMinPoint != new Vector2Int(image.Width, image.Height))
                        {
                            if (pixelArea < IslandPixelAreaThreshold)
                            {
                                Console.WriteLine("Logic error. Too small island.");
                            }

                            if (originalColors.Count > 1)
                            {
                                // 한 섬에 색상이 여러 가지라면 가장 많은 색상이 최종 색깔이 되도록 하자.
                                // 주로 경계선 주변에서 경계선과 섬 색깔이 블렌딩되면서 다른 색깔이 되는 패턴이다.
                                //var prominentColor = originalColors.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
                                var prominentColor = originalColors.OrderByDescending(e => e.Value)
                                    .First(e => e.Key != White).Key;

                                pixelColor = prominentColor;

                                // foreach (var originalColor in originalColors) {
                                //     Console.WriteLine($"{originalColor.Key} = {originalColor.Value}");
                                // }
                                // throw new Exception($"Island color is not uniform! It has {originalColors.Count} colors in it! coord={coord}");
                            }

                            if (originalColors.Count == 0)
                            {
                                throw new Exception("Island color is empty. Is this possible?");
                            }

                            if (pixelColor == White)
                            {
                                throw new Exception("Island color is WHITE?! Fix it!");
                            }

                            IncreaseCountOfDictionaryValue(pixelCountByColor, pixelColor, pixelArea);

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
                            foreach (var point in points)
                            {
                                A[point.y - yMin][point.x - xMin] = 1;
                            }

                            var area = MaxSubRect.MaxRectangle(subRectH, subRectW, A, out var beginIndexR,
                                out var endIndexR, out var beginIndexC, out var endIndexC);
                            Console.WriteLine(
                                $"Sub Rect: area:{area} [({yMin + beginIndexR},{xMin + beginIndexC})-({yMin + endIndexR},{xMin + endIndexC})]");
                            maxRectByMinPoint[Vector2IntToUInt32(fillMinPoint)] = GetRectRange(
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
            pixelCountByColor.TryGetValue(White, out var whiteCount);
            Console.WriteLine($"White Count: {whiteCount}");

            if (islandColorByMinPoint.Count < 2)
            {
                throw new IslandCountException();
            }

            var islandIndex = 0;
            foreach (var kv in islandColorByMinPoint)
            {
                Console.WriteLine(
                    $"Island #{islandIndex} fillMinPoint={kv.Key}, color={kv.Value}, area={islandPixelAreaByMinPoint[kv.Key]}");
                islandIndex++;
            }

            var colorCountIndex = 0;
            foreach (var kv in pixelCountByColor)
            {
                islandCountByColor.TryGetValue(kv.Key, out var islandCount);
                Console.WriteLine(
                    $"Color #{colorCountIndex} {kv.Key}: pixelCount={kv.Value}, islandCount={islandCount}");
                colorCountIndex++;
                if (kv.Key == White)
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

            var stageData = new StageData();
            foreach (var kv in islandPixelAreaByMinPoint)
            {
                var p = Vector2IntToUInt32(kv.Key);
                stageData.islandDataByMinPoint[p] = new IslandData
                {
                    pixelArea = islandPixelAreaByMinPoint[kv.Key],
                    rgba = Rgba32ToUInt32(islandColorByMinPoint[kv.Key]),
                    maxRect = maxRectByMinPoint[p],
                };
            }

            var outputPath = Path.ChangeExtension(startFileName, "json");
            if (string.IsNullOrEmpty(outputPathReplaceFrom) == false
                && string.IsNullOrEmpty(outputPathReplaceTo) == false)
            {
                outputPath = outputPath.Replace(outputPathReplaceFrom, outputPathReplaceTo);
            }

            using (var stream = File.Create(outputPath))
            {
                //var formatter = new BinaryFormatter();
                //formatter.Serialize(stream, stageData);
                JsonSerializer.Serialize(stream, stageData, SerOptions);
                stream.Close();
            }

            Console.WriteLine($"{stageData.islandDataByMinPoint.Count} islands loaded.");
            Console.WriteLine($"Written to {outputPath}");
            return outputPath;
        }

        // 이미지를 팔레트화시킨다.
        // 팔레트에는 흰색은 빠지도록 하고, 검은색은 반드시 포함되도록 한다.
        // (검은색은 경계선을 의미)
        // (흰색은 칠해지지 않은 칸을 의미)
        static string ExecuteQuantize(string sourceFileName, int maxColor = 30)
        {
            Console.Out.WriteLine($"Running {nameof(ExecuteQuantize)}");

            var targetFileName = AppendToFileName(sourceFileName, "-Q");
            var quantizerOptions = new QuantizerOptions { MaxColors = maxColor, Dither = null };
            var quantizer = new WuQuantizer(quantizerOptions);
            var config = Configuration.Default;

            using var image = Image.Load<Rgba32>(sourceFileName);

            var q = quantizer.CreatePixelSpecificQuantizer<Rgba32>(config);
            q.BuildPalette(new ExtensivePixelSamplingStrategy(), image);

            using var quantizedResult = q.QuantizeFrame(image.Frames[0], image.Frames[0].Bounds());
            var quantizedPalette = new List<Color>();
            foreach (var p in quantizedResult.Palette.Span)
            {
                if (p.A != 255)
                {
                    throw new InvalidOperationException("Should be opaque");
                }

                var c = new Color(p);
                // 팔레트에 흰색과 검은색은 반드시 빠지도록 한다.
                if (c != Color.White && c != Color.Black)
                {
                    quantizedPalette.Add(c);
                }
            }

            // 검은색은 경계선으로서 반드시 포함되어야 한다.
            quantizedPalette.Add(Black);

            using var stream = new FileStream(targetFileName, FileMode.Create);
            var encoder = new SixLabors.ImageSharp.Formats.Png.PngEncoder
            {
                Quantizer = new PaletteQuantizer(quantizedPalette.ToArray(), quantizerOptions),
                ColorType = SixLabors.ImageSharp.Formats.Png.PngColorType.Palette,
            };
            image.SaveAsPng(stream, encoder);
            stream.Close();

            return targetFileName;
        }

        // sourceFileName 위에 outlineFileName을 얹는다. 얹을 때 완전 검은 색깔만 얹는다.
        static string ExecuteFlattenedOutlineToSource(string sourceFileName, string outlineFileName)
        {
            Console.Out.WriteLine($"Running {nameof(ExecuteFlattenedOutlineToSource)}");

            var targetFileName = AppendToFileName(outlineFileName, "-FOTS");
            using var sourceImage = Image.Load<Rgba32>(sourceFileName);
            using var outlineImage = Image.Load<Rgba32>(outlineFileName);
            if (sourceImage.Size != outlineImage.Size)
            {
                throw new Exception("Image dimension differs.");
            }

            for (var h = 0; h < sourceImage.Height; h++)
            {
                for (var w = 0; w < sourceImage.Width; w++)
                {
                    if (outlineImage[w, h] == Black)
                    {
                        sourceImage[w, h] = Black;
                    }
                }
            }

            using var stream = new FileStream(targetFileName, FileMode.Create);
            sourceImage.SaveAsPng(stream);
            stream.Close();

            return targetFileName;
        }

        // 애매한 검은색을 완전한 검은색으로 바꾼다.
        static string ExecuteOutlineToBlack(string sourceFileName, int threshold)
        {
            Console.Out.WriteLine($"Running {nameof(ExecuteOutlineToBlack)}");

            var targetFileName = AppendToFileName(sourceFileName, "-OTB");
            using var image = Image.Load<Rgba32>(sourceFileName);
            for (var h = 0; h < image.Height; h++)
            {
                for (var w = 0; w < image.Width; w++)
                {
                    var pixelColor = image[w, h];
                    if (pixelColor.R <= threshold && pixelColor.G <= threshold && pixelColor.B <= threshold)
                    {
                        image[w, h] = Black;
                    }
                    else
                    {
                        image[w, h] = White;
                    }
                }
            }

            Directory.CreateDirectory(Path.GetDirectoryName(targetFileName));
            using var stream = new FileStream(targetFileName, FileMode.Create);
            image.Save(stream, new PngEncoder{ColorType = PngColorType.Rgb});
            stream.Close();
            
            using var checkImage = Image.Load<Rgba32>(targetFileName);
            for (var h = 0; h < checkImage.Height; h++)
            {
                for (var w = 0; w < checkImage.Width; w++)
                {
                    var pixelColor = checkImage[w, h];
                    if (pixelColor != Black && pixelColor != White)
                    {
                        Console.WriteLine("Logic error! Should be black or white pixel");
                        goto BreakLoop;
                    }
                }
            }
            BreakLoop:
            return targetFileName;
        }

        static string AppendToFileName(string fileName, string append)
        {
            var r = Path.Combine(Path.GetDirectoryName(fileName),
                Path.GetFileNameWithoutExtension(fileName) + append + Path.GetExtension(fileName));
            if (string.IsNullOrEmpty(outputPathReplaceFrom) == false)
            {
                r = r.Replace(outputPathReplaceFrom, outputPathReplaceTo);
            }

            return r;
        }

        // 아주 작은 비검은색(색칠할) 칸을 검은색으로 메운다.
        // 메운 영역이 매우 작은 경우는 색칠하기 아주 작은 공간일 것이다.
        // ('아주 작은'의 기준은 threshold 인자로 결정한다.)
        static string ExecuteFillSmallNotBlack(string sourceFileName)
        {
            Console.Out.WriteLine($"Running {nameof(ExecuteFillSmallNotBlack)}");

            var targetFileName = AppendToFileName(sourceFileName, "-FSNB");
            // Min Point 별 (섬 별) 섬 픽셀 수(면적)
            var islandPixelAreaByMinPoint = new Dictionary<Vector2Int, int>();
            using (var image = Image.Load<Rgba32>(sourceFileName))
            {
                // 각 픽셀에 대해서 반복한다.
                for (var h = 0; h < image.Height; h++)
                {
                    for (var w = 0; w < image.Width; w++)
                    {
                        var pixelColor = image[w, h];
                        if (pixelColor == Black)
                        {
                            // 경계선 색상(검은색)이면 할 일이 없다.
                        }
                        else
                        {
                            // (w, h) 좌표부터 검은색이 아닌 색을 검은색으로 채우면서 픽셀 수집한다.
                            // 수집한 모든 픽셀은 points에, points의 min point는 반환값으로 받는다.
                            var coord = new Vector2Int(w, h);
                            var fillMinPoint = FloodFill.ExecuteFillIfNotBlack(image, coord, Black, out var pixelArea,
                                out _, out _);
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
                    if (island.Value < IslandPixelAreaThreshold)
                    {
                        var fillMinPoint = FloodFill.ExecuteFillIfNotBlack(image, island.Key, Black, out var pixelArea,
                            out _, out _);
                        if (fillMinPoint != new Vector2Int(image.Width, image.Height) && pixelArea == island.Value)
                        {
                        }
                        else
                        {
                            Console.WriteLine("Logic error in ExecuteFillSmallNotBlack()!");
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

                // 저장된 파일을 열어서 모든 하얀색 칸이 threshold 이상으로 큰지 재차 확인한다.
                using (var resultImage = Image.Load<Rgba32>(targetFileName))
                {
                    var islandIndex = 0;
                    for (var h = 0; h < resultImage.Height; h++)
                    {
                        for (var w = 0; w < resultImage.Width; w++)
                        {
                            var pixelColor = resultImage[w, h];
                            if (pixelColor == White)
                            {
                                var coord = new Vector2Int(w, h);
                                FloodFill.ExecuteFillIfNotBlack(resultImage, coord, Black, out var pixelArea, out _,
                                    out _);
                                Console.WriteLine($"Check island #{islandIndex}: pixel area {pixelArea}");
                                if (pixelArea < IslandPixelAreaThreshold)
                                {
                                    Console.WriteLine("Logic error in ExecuteFillSmallNotBlack()!");
                                }

                                islandIndex++;
                            }
                        }
                    }
                }
            }

            return targetFileName;
        }

        static ulong GetRectRange(int v1, int v2, int v3, int v4)
        {
            return (ulong)v1 + ((ulong)v2 << 16) + ((ulong)v3 << 32) + ((ulong)v4 << 48);
        }

        static uint Rgba32ToUInt32(Rgba32 v)
        {
            return v.PackedValue;
            //return ((uint)v.A << 24) + ((uint)v.B << 16) + ((uint)v.G << 8) + (uint)v.R;
        }

        static Rgba32 UInt32ToRgba32(uint v)
        {
            return new Rgba32(v);
        }

        static uint Vector2IntToUInt32(Vector2Int k)
        {
            return ((uint)k.y << 16) + (uint)k.x;
        }

        static Vector2Int UInt32ToVector2Int(uint v)
        {
            return new Vector2Int((int)(v & 0xffff), (int)(v >> 16));
        }

        public static void IncreaseCountOfDictionaryValue<T>(Dictionary<T, int> pixelCountByColor, T pixel,
            int amount = 1)
        {
            pixelCountByColor.TryGetValue(pixel, out var currentCount);
            pixelCountByColor[pixel] = currentCount + amount;
        }
    }
}