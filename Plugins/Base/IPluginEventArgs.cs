using System;

namespace SP.Plugins
{
    public interface IPluginEventArgs
    {
        string IPAddress { get; set; }
        DateTime DateTime { get; set; }
    }
}