using System.Reflection;

namespace SP.Core
{
    public interface IPlugins
    {
        Assembly LoadPlugin(string path);
    }
}