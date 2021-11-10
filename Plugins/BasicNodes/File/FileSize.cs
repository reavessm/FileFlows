namespace FileFlow.BasicNodes.File
{
    using System.ComponentModel;
    using FileFlow.Plugin;
    using FileFlow.Plugin.Attributes;

    public class FileSize : Node
    {
        public override int Inputs => 1;
        public override int Outputs => 2;
        public override FlowElementType Type => FlowElementType.Logic;


        [NumberInt(1)]
        public int Lower { get; set; }

        [NumberInt(2)]
        public int Upper { get; set; }

        public override int Execute(NodeParameters args)
        {
            long size = new FileInfo(args.WorkingFile).Length;
            if (size < (Lower * 1024 * 1024))
                return 2;
            if (Upper > 0 && size > (Upper * 1024 * 1024))
                return 2;
            return 1;
        }
    }
}