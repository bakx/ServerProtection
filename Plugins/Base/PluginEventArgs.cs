using System;

namespace SP.Plugins
{
    public class PluginEventArgs : EventArgs, IPluginEventArgs
    {
        public string Details { get; set; }
        public string IPAddress { get; set; }
        public DateTime DateTime { get; set; }
    }
}