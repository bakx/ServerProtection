using System.Reflection;

namespace SP.Core.Plugin
{
    public interface IPlugins
    {
        Assembly LoadPlugin(string path);
    }
}