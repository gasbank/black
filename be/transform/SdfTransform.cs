using System;

namespace Black
{
    public class SdfTransform : BaseTransform
    {
        public string sourceFileName { get; set; }

        public override void Run()
        {
            var bbFileName = ExecuteBoxBlur(sourceFileName, 2);
            var resultFile = ExecuteSdf(bbFileName);
            Console.WriteLine(resultFile);
        }

        public static SdfTransform FromArgs(string[] args)
        {
            if (args.Length != 2 && args.Length != 4)
            {
                throw new ArgumentException("Provide three arguments: sdf [input path] <output path replace from> <output path replace to>");
            }

            var sourceFileName = args[1];
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
            return new SdfTransform
            {
                sourceFileName = sourceFileName,
                OutputPathReplaceFrom = outputPathReplaceFrom,
                OutputPathReplaceTo = outputPathReplaceTo
            };
        }
    }
}