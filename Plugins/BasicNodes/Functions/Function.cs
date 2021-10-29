namespace FileFlow.BasicNodes.Functions
{
    using System.ComponentModel;
    using FileFlow.Plugin;
    using FileFlow.Plugin.Attributes;
    using Jint.Runtime;
    using Jint.Native.Object;
    using Jint;
    using System.Text;

    public class Function : Node, IConfigurableOutputNode
    {
        public override int Inputs => 1;

        [DefaultValue(1)]
        [NumberIntAttribute(1)]
        public new int Outputs { get; set; }

        [DefaultValue("// VideoFile object contains info about the video file\n\n// return true to continue processing this flow\n// return false to stop it\nreturn true;")]
        [Code(2)]
        public string Code { get; set; }

        delegate void LogDelegate(params object[] values);
        public override int Execute(NodeParameters args)
        {
            args.Logger.DLog("Code: ", Environment.NewLine + new string('=', 40) + Environment.NewLine + Code + Environment.NewLine + new string('=', 40));
            if (string.IsNullOrEmpty(Code))
                return base.Execute(args); // no code, means will run fine... i think... maybe...  depends what i do

            var sb = new StringBuilder();
            var log = new
            {
                ILog = new LogDelegate(args.Logger.ILog),
                DLog = new LogDelegate(args.Logger.DLog),
                WLog = new LogDelegate(args.Logger.WLog),
                ELog = new LogDelegate(args.Logger.ELog),
            };
            var engine = new Engine(options =>
            {
                options.LimitMemory(4_000_000);
                options.MaxStatements(100);
            })
            .SetValue("Logger", args.Logger)
            //.SetValue("ILog", log.ILog)
            ;

            var result = engine.Evaluate(Code).ToObject();
            if (result as bool? != true)
                args.Result = NodeResult.Failure;

            return base.Execute(args);
        }
    }
}