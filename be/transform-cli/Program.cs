using System;

namespace Black
{
    class TransformMain
    {
        static void Main(string[] args)
        {
            if (args.Length <= 0)
            {
                throw new ArgumentException("Should provide at least one param: mode");
            }

            var mode = args[0];

            if (mode == "batch")
            {
                MultipleFilesTransform.FromArgs(args).Run();
            }
            else if (mode == "sdf")
            {
                SdfTransform.FromArgs(args).Run();
            }
            else
            {
                SingleFileTransform.FromArgs(args).Run();
            }
        }
    }
}