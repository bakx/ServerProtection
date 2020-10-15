using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;

namespace SP.Core.Plugin
{
	public class Plugins<T> : AssemblyLoadContext, IPlugins
	{
		private readonly ILogger log;

		public Plugins(ILogger log)
		{
			this.log = log;
		}

		/// <summary>
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public Assembly LoadPlugin(string path)
		{
			// Diagnostics
			log.LogDebug("Loading plugin {0}", path);

			// Navigate up to the solution root
			string root = Assembly.GetEntryAssembly()?.Location;

			string pluginLocation =
				Path.GetFullPath(Path.Combine(root, path.Replace('\\', Path.DirectorySeparatorChar)));

			// Diagnostics
			log.LogDebug("Loading plugin dependencies for {0}", pluginLocation);

			// Load plugin
			PluginsLoadContext loadContext = new PluginsLoadContext(pluginLocation);
			return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
		}

		/// <summary>
		/// </summary>
		/// <param name="assembly"></param>
		/// <returns></returns>
		public IEnumerable<T> CreateCommands(Assembly assembly)
		{
			int count = 0;

			foreach (Type type in assembly.GetTypes())
			{
				if (!typeof(T).IsAssignableFrom(type))
				{
					continue;
				}

				// Ignore instances not related to T
				if (!(Activator.CreateInstance(type) is T result))
				{
					continue;
				}

				count++;
				yield return result;
			}

			// If types are found in plugin, continue disassemble.
			if (count != 0)
			{
				yield break;
			}

			// Invalid plug-in. Consider a non-critical error.
			string availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
			log.LogError("Invalid plug-in. No commands found in {0} . Types available in plugin: ", assembly.Location,
				availableTypes);
		}
	}
}