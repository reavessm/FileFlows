namespace FileFlow.VideoNodes
{
    using System.ComponentModel.DataAnnotations;
    using FileFlow.Plugin.Attributes;

    public class Plugin : FileFlow.Plugin.IPlugin
    {
        public string Name => "Video Nodes";

        [Required]
        [File(1, "exe")]
        public string HandBrakeCli { get; set; }

        [Required]
        [File(2, "exe")]
        public string FFProbeExe { get; set; }
    }
}