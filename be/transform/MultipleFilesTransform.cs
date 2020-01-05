using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Black
{
    public class MultipleFilesTransform : BaseTransform
    {
        public string InputPath { get; set; }

        public override void Run()
        {
            var pngFiles = Directory.GetFiles(InputPath, "*.png", SearchOption.AllDirectories);
            //var jpgFiles = Directory.GetFiles(inputPath, "*.jpg", SearchOption.AllDirectories);
            var inputFiles = pngFiles;// pngFiles.Concat(jpgFiles).ToArray();

            foreach (var inputFileName in inputFiles)
            {
                // @"C:\Users\gb\Google Drive\2020_컬러뮤지엄\02. Stage\01~10\진주귀걸이를한소녀.png"
                if (Regex.IsMatch(inputFileName, @"\\[0-9][0-9]~[0-9][0-9]\\"))
                {
                    Console.Out.WriteLine(inputFileName);
                    new SingleFileTransform
                    {
                        Mode = "dit",
                        StartFileName = inputFileName,
                        MaxColor = 30,
                        OutputPathReplaceFrom = OutputPathReplaceFrom,
                        OutputPathReplaceTo = OutputPathReplaceTo,
                    }.Run();
                }
            }
        }

        public static MultipleFilesTransform FromArgs(string[] args)
        {
            if (args.Length != 2 && args.Length != 4)
            {
                throw new ArgumentException("Provide three arguments: batch [input path] <output path replace from> <output path replace to>");
            }

            var inputPath = args[1];
            var outputPathReplaceFrom = "";
            if (args.Length >= 3)
            {
                outputPathReplaceFrom = args[2];
            }
            var outputPathReplaceTo = "";
            if (args.Length >= 4)
            {
                outputPathReplaceTo = args[3];
            }
            return new MultipleFilesTransform
            {
                InputPath = inputPath,
                OutputPathReplaceFrom = outputPathReplaceFrom,
                OutputPathReplaceTo = outputPathReplaceTo
            };
        }

    }
}