using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Black
{
    public abstract class BaseTransform
    {
        public string OutputPathReplaceFrom { get; set; }
        public string OutputPathReplaceTo { get; set; }

        public abstract void Run();

        protected string AppendToFileName(string fileName, string append)
        {
            var r = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + append + Path.GetExtension(fileName));
            if (string.IsNullOrEmpty(OutputPathReplaceFrom) == false)
            {
                r = r.Replace(OutputPathReplaceFrom, OutputPathReplaceTo);
            }
            return r;
        }

        protected string ExecuteSdf(string sourceFileName)
        {
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
            return targetFileName;
        }

        protected string ExecuteBoxBlur(string sourceFileName, int radius)
        {
            var targetFileName = AppendToFileName(sourceFileName, "-BB");
            using (var image = Image.Load<Rgba32>(sourceFileName))
            {

                var targetImage = new Image<Rgba32>(image.Width, image.Height);

                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        targetImage[x, y] = Utils.Average(image, x, y, radius);
                    }
                }

                var p = targetImage[0, 0];

                Directory.CreateDirectory(Path.GetDirectoryName(targetFileName));
                using (var stream = new FileStream(targetFileName, FileMode.Create))
                {
                    targetImage.SaveAsPng(stream);
                    stream.Close();
                }
            }
            return targetFileName;
        }
    }
}