namespace SP.Plugins
{
    public class PluginOptions : IPluginOptions
    {
        public bool BlockIPRange { get; set; }

        // Custom 1-4
        public string Custom1 { get; set; }
        public string Custom2 { get; set; }
        public string Custom3 { get; set; }
        public string Custom4 { get; set; }
    }
}