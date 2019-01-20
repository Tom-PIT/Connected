using Newtonsoft.Json;
using System.Collections.Generic;

namespace TomPIT.Services
{
	internal class PluginSet
	{
		private List<string> _plugins = null;

		[JsonProperty(PropertyName = "server")]
		public string Server { get; set; }

		[JsonProperty(PropertyName = "plugins")]
		public List<string> Plugins
		{
			get
			{
				if (_plugins == null)
					_plugins = new List<string>();

				return _plugins;
			}
		}
	}
}
