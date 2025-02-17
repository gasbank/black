#if IMAGESHARP
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using Color = SixLabors.ImageSharp.Color;
using Vector4 = System.Numerics.Vector4;
#if UNITY_2020_1_OR_NEWER
using Math = UnityEngine.Mathf;

#endif


namespace black_dev_tools
{
    internal class IslandCountException : Exception
    {
    }

    public static class Program
    {
        static string outputPathReplaceFrom = "";
        static string outputPathReplaceTo = "";
        static string outputNewFileName = null;

        static readonly Rgba32 Black = Rgba32.ParseHex("000000ff");
        static readonly Rgba32 Red = Rgba32.ParseHex("ff0000ff");
        static readonly Rgba32 White = Rgba32.ParseHex("ffffffff");
        static readonly Rgba32 AllZeros = Rgba32.ParseHex("00000000");

        public static void Main(string[] args)
        {
            if (args.Length <= 0)
            {
                Logger.WriteErrorLine("Should provide at least one param: mode");
                return;
            }

            var mode = args[0];

            if (mode == "batch")
                ProcessMultipleFiles(args);
            else if (mode == "sdf")
                ProcessSdf(args);
            else
                ProcessSingleFileAdaptiveOutlineThreshold(args);
        }

        static void ProcessSingleFileAdaptiveOutlineThreshold(string[] args)
        {
            try
            {
                ProcessSingleFile(args, int.Parse(args[6]));
            }
            catch (IslandCountException)
            {
                ProcessSingleFile(args, int.Parse(args[6]) + 10);
            }
            catch (DirectoryNotFoundException e)
            {
                Logger.WriteErrorLine($"Exception: {e.Message}");
            }
        }

        static void ProcessSdf(string[] args)
        {
            if (args.Length != 2 && args.Length != 4)
            {
                Logger.WriteErrorLine(
                    "Provide three arguments: sdf [input path] <output path replace from> <output path replace to>");
                throw new ArgumentException();
            }

            var sourceFileName = args[1];

            if (args.Length >= 3) outputPathReplaceFrom = args[2];

            if (args.Length >= 4) outputPathReplaceTo = args[3];

            var bbFileName = ExecuteBoxBlur(sourceFileName, 2);
            ExecuteSdf(bbFileName);
        }

        static void ExecuteSdf(string sourceFileName)
        {
            Logger.WriteLine($"Running {nameof(ExecuteSdf)}");

            var targetFileName = AppendToFileName(sourceFileName, "-SDF");
            using (var image = Image.Load<Rgba32>(sourceFileName))
            {
                var targetImage = new Image<Rgba32>(image.Width, image.Height);

                SDFTextureGenerator.Generate(image, targetImage, 0, 3, 0, RGBFillMode.White);

                Directory.CreateDirectory(Path.GetDirectoryName(targetFileName));
                using (var stream = new FileStream(targetFileName, FileMode.Create))
                {
                    targetImage.SaveAsPng(stream);
                    stream.Close();
                }
            }
        }

        static Rgba32 GetPixelClamped(Image<Rgba32> image, int x, int y)
        {
            return image[Math.Clamp(x, 0, image.Width - 1), Math.Clamp(y, 0, image.Height - 1)];
        }

        static void SetPixelClamped(Image<Rgba32> image, int x, int y, Rgba32 v)
        {
            image[Math.Clamp(x, 0, image.Width - 1), Math.Clamp(y, 0, image.Height - 1)] = v;
        }

        static Rgba32 Average(Image<Rgba32> image, int x, int y, int radius)
        {
            var sum = Vector4.Zero;
            for (var h = y - radius; h <= y + radius; h++)
            for (var w = x - radius; w <= x + radius; w++)
            {
                var c = GetPixelClamped(image, w, h);
                sum += c.ToVector4();
            }

            var d = (radius * 2 + 1) * (radius * 2 + 1);
            var avg = new Rgba32(sum / d);
            return avg;
        }

        static string ExecuteBoxBlur(string sourceFileName, int radius)
        {
            Logger.WriteLine($"Running {nameof(ExecuteBoxBlur)}");

            var targetFileName = AppendToFileName(sourceFileName, "-BB");
            using (var image = Image.Load<Rgba32>(sourceFileName))
            {
                var targetImage = new Image<Rgba32>(image.Width, image.Height);

                for (var y = 0; y < image.Height; y++)
                for (var x = 0; x < image.Width; x++)
                    targetImage[x, y] = image[x, y] == Black ? Black : Average(image, x, y, radius);

                Directory.CreateDirectory(Path.GetDirectoryName(targetFileName));
                using (var stream = new FileStream(targetFileName, FileMode.Create))
                {
                    targetImage.SaveAsPng(stream);
                    stream.Close();
                }
            }

            return targetFileName;
        }

        static void ProcessMultipleFiles(string[] args)
        {
            if (args.Length != 2 && args.Length != 4)
            {
                Logger.WriteErrorLine(
                    "Provide three arguments: batch [input path] <output path replace from> <output path replace to>");
                return;
            }

            var inputPath = args[1];

            if (args.Length >= 3) outputPathReplaceFrom = args[2];

            if (args.Length >= 4) outputPathReplaceTo = args[3];

            var pngFiles = Directory.GetFiles(inputPath, "*.png", SearchOption.AllDirectories);
            //var jpgFiles = Directory.GetFiles(inputPath, "*.jpg", SearchOption.AllDirectories);
            var inputFiles = pngFiles; // pngFiles.Concat(jpgFiles).ToArray();

            foreach (var inputFileName in inputFiles)
                // @"C:\Users\gb\Google Drive\2020_컬러뮤지엄\02. Stage\01~10\진주귀걸이를한소녀.png"
                if (Regex.IsMatch(inputFileName, @"\\[0-9][0-9]~[0-9][0-9]\\"))
                {
                    Logger.WriteLine(inputFileName);

                    ProcessSingleFileAdaptiveOutlineThreshold(new[]
                    {
                        "dit",
                        inputFileName,
                        30.ToString(),
                        outputPathReplaceFrom,
                        outputPathReplaceTo
                    });
                }
        }

        static void ProcessSingleFile(string[] args, int outlineThreshold)
        {
            if (args.Length < 3)
                Logger.WriteErrorLine(
                    "Provide three arguments: [mode] [Input.png/jpg] [max colors] <output path replace from> <output path replace to>");

            var mode = args[0];
            var startFileName = args[1]
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);

            int.TryParse(args[2], out var maxColor);
            maxColor = Math.Clamp(maxColor, 3, 100);

            if (args.Length >= 4) outputPathReplaceFrom = args[3];

            if (args.Length >= 5) outputPathReplaceTo = args[4];

            outputNewFileName = args.Length >= 6 ? args[5] : null;

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
                var rasapFileName = ExecuteResizeAndSaveAsPng(startFileName, 2048);
                var otbFileName = ExecuteOutlineToBlack(rasapFileName, outlineThreshold);
                var fsnbFileName = ExecuteFillSmallNotBlack(otbFileName, 120);
                var qFileName = ExecuteQuantize(rasapFileName, maxColor);
                var fotsFileName = ExecuteFlattenedOutlineToSource(qFileName, fsnbFileName);
                var bytesFileName = ExecuteDetermineIsland(fotsFileName, rasapFileName);

                // 색칠 테스트. 첫 데이터를 얻기 위한 것으로 오류 메시지는 경고로 무시해도 된다.
                var ditFileName = ExecuteDetermineIslandTest(fsnbFileName, bytesFileName, true, false);

                // DIT 파일이 최종 컬러링 완성 시의 이미지이다. 이걸로 최종적으로 데이터를 다시 뽑는다.
                bytesFileName = ExecuteDetermineIsland(ditFileName, rasapFileName);

                //두 번째 테스트. 여기서 오류가 나면 뭔가 이상한거다.
                ExecuteDetermineIslandTest(fsnbFileName, bytesFileName, false, true);

                var bbFileName = ExecuteBoxBlur(fsnbFileName, 1);
                ExecuteSdf(bbFileName);

                // 필요없는 파일은 삭제한다.
                // 디버그가 필요한 경우 삭제하지 않고 살펴보면 된다.
//                if (rasapFileName != startFileName)
//                {
//                    File.Delete(rasapFileName);
//                }

                var artifactsDirName = Path.GetDirectoryName(ChangeToArtifactsPath(bbFileName));
                if (string.IsNullOrEmpty(artifactsDirName))
                {
                    throw  new ArgumentNullException();
                }
                
                try
                {
                    Directory.Delete(artifactsDirName, true);
                }
                catch (DirectoryNotFoundException)
                {
                }

                MoveOrOverwriteToArtifactsDirectory(bbFileName);
                MoveOrOverwriteToArtifactsDirectory(ditFileName);
                MoveOrOverwriteToArtifactsDirectory(fotsFileName);
                MoveOrOverwriteToArtifactsDirectory(fsnbFileName);
                MoveOrOverwriteToArtifactsDirectory(otbFileName);
                MoveOrOverwriteToArtifactsDirectory(qFileName);
                MoveOrOverwriteToArtifactsDirectory(rasapFileName);
            }
            else
            {
                Logger.WriteErrorLine($"Unknown mode provided: {mode}");
                return;
            }

            Logger.WriteLine("Completed.");
        }

        static void MoveOrOverwriteToArtifactsDirectory(string path)
        {
            var artifactsPath = ChangeToArtifactsPath(path);
            var artifactsDirName = Path.GetDirectoryName(artifactsPath);
            if (string.IsNullOrEmpty(artifactsDirName))
            {
                throw new ArgumentNullException();
            }
            
            Directory.CreateDirectory(artifactsDirName);
            File.Delete(artifactsPath);
            File.Move(path, artifactsPath);
        }

        static string ChangeToArtifactsPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException();
            }
            
            var dirName = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(dirName))
            {
                throw new ArgumentNullException();
            }

            return Path.Combine(dirName, "Artifacts~", Path.GetFileName(path));
        }

        // 섬 데이터와 외곽선 데이터를 이용해 색칠을 자동으로 해 본다.
        // 색칠 후 이미지에 문제가 없는지 확인하기 위한 테스트 과정이다.
        static string ExecuteDetermineIslandTest(string sourceFileName, string bytesFileName, bool errorAsWarning, bool writeA1A2Tex)
        {
            Logger.WriteLine($"Running {nameof(ExecuteDetermineIslandTest)}");

            var targetFileName = AppendToFileName(sourceFileName, "-DIT");
            var a1TexFileName = AppendToFileName(targetFileName, "-A1");
            var a2TexFileName = AppendToFileName(targetFileName, "-A2");
            StageData stageData;
            using (var bytesFileStream = new FileStream(bytesFileName, FileMode.Open))
            {
                var formatter = new BinaryFormatter();
                try
                {
                    // Deserialize the hashtable from the file and 
                    // assign the reference to the local variable.
                    stageData = (StageData) formatter.Deserialize(bytesFileStream);
                }
                catch (SerializationException e)
                {
                    Logger.WriteErrorLine("Failed to deserialize. Reason: " + e.Message);
                    throw;
                }
            }

            var colorUintArray = stageData.CreateColorUintArray();
            var colorUintDict = new Dictionary<uint, int>();
            for (var i = 0; i < colorUintArray.Length; i++)
            {
                colorUintDict[colorUintArray[i]] = i + 1; // Palette Index 0은 외곽선 용으로 예비한다.
            }

            using (var image = Image.Load<Rgba32>(sourceFileName))
            {
                var a1Tex = new Image<Rgba32>(image.Width, image.Height, AllZeros);
                var a2Tex = new Image<Rgba32>(image.Width, image.Height, AllZeros);

                var islandIndex = 1; // Island Index 0은 외곽선 용으로 예비한다.
                
                // dictionary의 이터레이션 순서에 의존하면 안되고, island data에서 지정한 index 순서로 하자.
                foreach (var island in stageData.islandDataByMinPoint.OrderBy(e => e.Value.index))
                {
                    var minPoint = UInt32ToVector2Int(island.Key);
                    var targetColor = UInt32ToRgba32(island.Value.rgba);
                    var paletteIndex = colorUintDict[island.Value.rgba];
                    var fillMinPoint = FloodFill.ExecuteFillIf(image, minPoint, White, targetColor, out var pixelArea,
                        out _, out _, islandIndex, (islandIndexCallback, fx, fy) =>
                        {
                            GetAlpha8Pair(islandIndexCallback, paletteIndex, out var a1, out var a2);
                            a1Tex[fx, fy] = new Rgba32 {A = a1};
                            a2Tex[fx, fy] = new Rgba32 {A = a2};
                        });

                    if (fillMinPoint == new Vector2Int(image.Width, image.Height))
                    {
                        if (errorAsWarning)
                        {
                            // 이번엔 오류로 안친다.
                            Logger.WriteLine("Logic error in ExecuteDetermineIslandTest()! Invalid fillMinPoint");
                        }
                        else
                        {
                            Logger.WriteErrorLine("Logic error in ExecuteDetermineIslandTest()! Invalid fillMinPoint");
                        }
                    }

                    if (pixelArea != island.Value.pixelArea)
                    {
                        if (errorAsWarning)
                        {
                            // 이번엔 오류로 안친다.
                            Logger.WriteLine(
                                $"Logic error in ExecuteDetermineIslandTest()! Pixel area {pixelArea} expected to be {island.Value.pixelArea}");
                        }
                        else
                        {
                            Logger.WriteErrorLine(
                                $"Logic error in ExecuteDetermineIslandTest()! Pixel area {pixelArea} expected to be {island.Value.pixelArea}");
                        }
                    }

                    islandIndex++;
                }

                using (var stream = new FileStream(targetFileName, FileMode.Create))
                {
                    image.SaveAsPng(stream);
                    stream.Close();
                }

                if (writeA1A2Tex)
                {
                    using (var stream = new FileStream(a1TexFileName, FileMode.Create))
                    {
                        a1Tex.SaveAsPng(stream);
                        stream.Close();
                    }

                    using (var stream = new FileStream(a2TexFileName, FileMode.Create))
                    {
                        a2Tex.SaveAsPng(stream);
                        stream.Close();
                    }
                }
            }

            return targetFileName;
        }

        // paletteIndex에 6-bit -> 최대 64개 팔레트
        // islandIndex에 10-bit -> 최대 1024개 섬
        static void GetAlpha8Pair(int islandIndex, int paletteIndex, out byte a1, out byte a2)
        {
            if (islandIndex <= 0 || islandIndex >= (2 << 9))
            {
                throw new ArgumentOutOfRangeException(nameof(islandIndex));
            }
            
            if (paletteIndex <= 0 || paletteIndex >= (2 << 5))
            {
                throw new ArgumentOutOfRangeException(nameof(paletteIndex));
            }

            a1 = (byte) (paletteIndex | ((islandIndex & ((1 << 2) - 1)) << 6));
            a2 = (byte) (islandIndex >> 2);
        }

        // 입력 이미지로 섬 데이터를 만든다.
        // 섬 데이터는 유니티에서 사용하게 된다.
        static string ExecuteDetermineIsland(string sourceFileName, string startFileName)
        {
            Logger.WriteLine($"Running {nameof(ExecuteDetermineIsland)}");

            // 이미지 파일을 열어봅시다~
            using (var image = Image.Load<Rgba32>(sourceFileName))
            {
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
                            if (originalColors.Count > 1)
                            {
                                // 한 섬에 색상이 여러 가지라면 가장 많은 색상이 최종 색깔이 되도록 하자.
                                // 주로 경계선 주변에서 경계선과 섬 색깔이 블렌딩되면서 다른 색깔이 되는 패턴이다.
                                //var prominentColor = originalColors.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
                                var prominentColor = originalColors.OrderByDescending(e => e.Value)
                                    .First(e => e.Key != White).Key;

                                pixelColor = prominentColor;

                                // foreach (var originalColor in originalColors) {
                                //     Logger.WriteLine($"{originalColor.Key} = {originalColor.Value}");
                                // }
                                // throw new Exception($"Island color is not uniform! It has {originalColors.Count} colors in it! coord={coord}");
                            }

                            if (originalColors.Count == 0)
                                throw new Exception("Island color is empty. Is this possible?");

                            if (pixelColor == White) throw new Exception("Island color is WHITE?! Fix it!");

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
                            foreach (var point in points) A[point.y - yMin][point.x - xMin] = 1;

                            var area = MaxSubRect.MaxRectangle(subRectH, subRectW, A, out var beginIndexR,
                                out var endIndexR, out var beginIndexC, out var endIndexC);
                            Logger.WriteLine(
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

                Logger.WriteLine($"Total Pixel Count: {image.Width * image.Height}");
                pixelCountByColor.TryGetValue(White, out var whiteCount);
                Logger.WriteLine($"White Count: {whiteCount}");

                if (islandColorByMinPoint.Count < 1) throw new IslandCountException();

                var islandIndex = 1; // 0번째 island는 외곽선을 위해 예비한 값이다.
                foreach (var kv in islandColorByMinPoint.OrderBy(kv => Vector2IntToUInt32(kv.Key)))
                {
                    Logger.WriteLine(
                        $"Island #{islandIndex} fillMinPoint={kv.Key}, color={kv.Value}, area={islandPixelAreaByMinPoint[kv.Key]}");
                    islandIndex++;
                }

                var colorCountIndex = 1; // 0 번째 컬러는 외곽선을 위해 예비한 블랙 값이다.
                foreach (var kv in pixelCountByColor)
                {
                    islandCountByColor.TryGetValue(kv.Key, out var islandCount);
                    Logger.WriteLine(
                        $"Color #{colorCountIndex} {kv.Key}: pixelCount={kv.Value}, islandCount={islandCount}");
                    colorCountIndex++;

                    if (kv.Key == White) throw new Exception("Palette color should not be white!");
                }

                var pixelAreaCountIndex = 0;
                foreach (var kv in islandCountByPixelArea.OrderByDescending(kv => kv.Key))
                {
                    Logger.WriteLine($"Pixel Area #{pixelAreaCountIndex} {kv.Key}: islandCount={kv.Value}");
                    pixelAreaCountIndex++;
                }

                var stageData = new StageData();
                var islandIndex2 = 1;
                foreach (var kv in islandPixelAreaByMinPoint.OrderBy(kv => Vector2IntToUInt32(kv.Key)))
                {
                    var p = Vector2IntToUInt32(kv.Key);
                    stageData.islandDataByMinPoint[p] = new IslandData
                    {
                        index = islandIndex2,
                        pixelArea = islandPixelAreaByMinPoint[kv.Key],
                        rgba = Rgba32ToUInt32(islandColorByMinPoint[kv.Key]),
                        maxRect = maxRectByMinPoint[p]
                    };
                    islandIndex2++;
                }

                var outputPath = Path.ChangeExtension(startFileName, "bytes");
                if (string.IsNullOrEmpty(outputPathReplaceFrom) == false
                    && string.IsNullOrEmpty(outputPathReplaceTo) == false)
                    outputPath = outputPath.Replace(outputPathReplaceFrom, outputPathReplaceTo);

                using (var stream = File.Create(outputPath))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, stageData);
                    stream.Close();
                }

                Logger.WriteLine($"{stageData.islandDataByMinPoint.Count} islands loaded.");
                Logger.WriteLine($"Written to {outputPath}");
                return outputPath;
            }
        }

        // 이미지를 팔레트화시킨다.
        // 팔레트에는 반드시 검은색과 흰색은 빠지도록 한다.
        static string ExecuteQuantize(string sourceFileName, int maxColor = 30)
        {
            Logger.WriteLine($"Running {nameof(ExecuteQuantize)}");

            var targetFileName = AppendToFileName(sourceFileName, "-Q");

            var quOpts = new QuantizerOptions {MaxColors = maxColor};
            //var quantizer = new OctreeQuantizer(quOpts);
            var quantizer = new WuQuantizer(quOpts);
            var config = Configuration.Default;
            var pixelQuantizer = quantizer.CreatePixelSpecificQuantizer<Rgba32>(config);

            using (var image = Image.Load<Rgba32>(sourceFileName))
            using (var quantizedResult =
                pixelQuantizer.BuildPaletteAndQuantizeFrame(image.Frames[0], image.Frames[0].Bounds()))
            {
                var quantizedPalette = new List<Color>();
                foreach (var p in quantizedResult.Palette.Span)
                {
                    var c = new Color(p);
                    // 팔레트에 흰색과 검은색은 반드시 빠지도록 한다.
                    if (c != Color.White && c != Color.Black) quantizedPalette.Add(c);
                }

                using (var stream = new FileStream(targetFileName, FileMode.Create))
                {
                    var encoder = new PngEncoder
                    {
                        Quantizer = new PaletteQuantizer(quantizedPalette.ToArray()),
                        ColorType = PngColorType.Palette
                    };
                    image.SaveAsPng(stream, encoder);
                    stream.Close();
                }

                return targetFileName;
            }
        }

        // sourceFileName 위에 outlineFileName을 얹는다. 얹을 때 완전 검은 색깔만 얹는다.
        static string ExecuteFlattenedOutlineToSource(string sourceFileName, string outlineFileName)
        {
            Logger.WriteLine($"Running {nameof(ExecuteFlattenedOutlineToSource)}");

            var targetFileName = AppendToFileName(outlineFileName, "-FOTS");
            using (var sourceImage = Image.Load<Rgba32>(sourceFileName))
            using (var outlineImage = Image.Load<Rgba32>(outlineFileName))
            {
                if (sourceImage.Size() != outlineImage.Size()) throw new Exception("Image dimension differs.");

                for (var h = 0; h < sourceImage.Height; h++)
                for (var w = 0; w < sourceImage.Width; w++)
                {
                    if (outlineImage[w, h] == Black)
                        sourceImage[w, h] = Black;
                }


                using (var stream = new FileStream(targetFileName, FileMode.Create))
                {
                    sourceImage.SaveAsPng(stream);
                    stream.Close();
                }
            }

            return targetFileName;
        }

        // 너무 큰 이미지를 작은 이미지로 줄이면서 PNG 파일이 아니면 PNG로 바꾼다.
        static string ExecuteResizeAndSaveAsPng(string sourceFileName, int threshold)
        {
            Logger.WriteLine($"Running {nameof(ExecuteResizeAndSaveAsPng)}");

            using var image = Image.Load<Rgba32>(sourceFileName);

            // 정사각형이 아니라면 우선 큰 변 기준으로 정사각형으로 만든다. (여백 추가)
            if (image.Width != image.Height)
            {
                var maxSide = Math.Max(image.Width, image.Height);

                var options = new ResizeOptions
                {
                    Size = new Size(maxSide, maxSide),
                    Mode = ResizeMode.BoxPad,
                    Sampler = new NearestNeighborResampler()
                };
                image.Mutate(x => x.Resize(options));
            }

            if (image.Width > threshold)
            {
                var options = new ResizeOptions
                {
                    Size = new Size(threshold, threshold),
                    Mode = ResizeMode.BoxPad
                };
                image.Mutate(x => x.Resize(options));
            }

            var targetFileName = AppendToFileName(sourceFileName, "", ".png", outputNewFileName);

            image.Save(targetFileName, new PngEncoder());

            return targetFileName;
        }

        // 애매한 검은색을 완전한 검은색으로 바꾼다.
        static string ExecuteOutlineToBlack(string sourceFileName, int threshold)
        {
            Logger.WriteLine($"Running {nameof(ExecuteOutlineToBlack)}");

            var targetFileName = AppendToFileName(sourceFileName, "-OTB");
            using (var image = Image.Load<Rgba32>(sourceFileName))
            {
                var newImage = image.Clone();

                for (var h = 0; h < image.Height; h++)
                for (var w = 0; w < image.Width; w++)
                {
                    var pixelColor = image[w, h];
                    var r = pixelColor.R / 255.0f;
                    var g = pixelColor.G / 255.0f;
                    var b = pixelColor.B / 255.0f;
                    var y = 0.2126f * r + 0.7152f * g + 0.0722f * b;
                    if (y < threshold / 255.0f)
                    {
                        var ss = 0;
                        // 7x7 윈도우로 모두 검은색 만들기
                        for (var hh = -ss; hh <= ss; hh++)
                        for (var ww = -ss; ww <= ss; ww++)
                        {
                            SetPixelClamped(newImage, w + ww, h + hh, Black);
                        }
                    }
                    else
                    {
                        newImage[w, h] = White;
                    }
                }

                var targetDirName = Path.GetDirectoryName(targetFileName);
                if (string.IsNullOrEmpty(targetDirName) == false)
                {
                    Directory.CreateDirectory(targetDirName);
                    using (var stream = new FileStream(targetFileName, FileMode.Create))
                    {
                        newImage.SaveAsPng(stream);
                        stream.Close();
                    }

                    // Check only white & black
                    using var targetImage = Image.Load<Rgba32>(targetFileName);
                    for (var h = 0; h < targetImage.Height; h++)
                    for (var w = 0; w < targetImage.Width; w++)
                    {
                        if (targetImage[w, h] != Black && targetImage[w, h] != White)
                        {
                            Logger.WriteErrorLine(
                                $"Logic error. Image pixel at ({w},{h}) is not black or white. It is {targetImage[w, h]}");
                        }
                    }
                }
            }

            return targetFileName;
        }

        static string AppendToFileName(string fileName, string append, string newExt = null, string newFileName = null)
        {
            var fileDirName = Path.GetDirectoryName(fileName);
            if (string.IsNullOrEmpty(fileDirName)) return string.Empty;

            var r = Path.Combine(fileDirName,
                (newFileName ?? Path.GetFileNameWithoutExtension(fileName)) + append +
                (newExt ?? Path.GetExtension(fileName)));
            if (string.IsNullOrEmpty(outputPathReplaceFrom) == false)
                r = r.Replace(outputPathReplaceFrom, outputPathReplaceTo);

            return r;
        }

        // 아주 작은 비검은색을 검은색으로 메운다.
        static string ExecuteFillSmallNotBlack(string sourceFileName, int threshold = 4 * 4 * 4)
        {
            Logger.WriteLine($"Running {nameof(ExecuteFillSmallNotBlack)}");

            var targetFileName = AppendToFileName(sourceFileName, "-FSNB");
            // Min Point 별 (섬 별) 섬 픽셀 수(면적)
            var islandPixelAreaByMinPoint = new Dictionary<Vector2Int, int>();
            using (var image = Image.Load<Rgba32>(sourceFileName))
            {
                // 각 픽셀에 대해서 반복한다.
                for (var h = 0; h < image.Height; h++)
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
                            islandPixelAreaByMinPoint[fillMinPoint] = pixelArea;
                        else
                            throw new Exception("Invalid fill min point!");
                    }
                }
            }

            // 메모리상 image 변수는 직전 과정에서 변경되었으므로, 다시 읽어들이자.
            // 여기서부터 본격적으로 작은 비검정색칸을 검정색칸으로 채운다.
            using (var image = Image.Load<Rgba32>(sourceFileName))
            {
                foreach (var island in islandPixelAreaByMinPoint)
                    if (island.Value < threshold)
                    {
                        var fillMinPoint = FloodFill.ExecuteFillIfNotBlack(image, island.Key, Black, out var pixelArea,
                            out _, out _);
                        if (fillMinPoint != new Vector2Int(image.Width, image.Height) && pixelArea == island.Value)
                        {
                        }
                        else
                        {
                            Logger.WriteErrorLine("Logic error in ExecuteFillSmallNotBlack()!");
                        }
                    }

                // 그리고 저장!
                var targetDirName = Path.GetDirectoryName(targetFileName);
                if (string.IsNullOrEmpty(targetDirName)) return string.Empty;

                Directory.CreateDirectory(targetDirName);
                using (var stream = new FileStream(targetFileName, FileMode.Create))
                {
                    image.SaveAsPng(stream);
                    stream.Close();
                }
            }

            return targetFileName;
        }

        static ulong GetRectRange(int v1, int v2, int v3, int v4)
        {
            return (ulong) v1 + ((ulong) v2 << 16) + ((ulong) v3 << 32) + ((ulong) v4 << 48);
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
            return ((uint) k.y << 16) + (uint) k.x;
        }

        static Vector2Int UInt32ToVector2Int(uint v)
        {
            return new Vector2Int((int) (v & 0xffff), (int) (v >> 16));
        }

        public static void IncreaseCountOfDictionaryValue<T>(Dictionary<T, int> pixelCountByColor, T pixel)
        {
            pixelCountByColor.TryGetValue(pixel, out var currentCount);
            pixelCountByColor[pixel] = currentCount + 1;
        }
    }
}
#endif