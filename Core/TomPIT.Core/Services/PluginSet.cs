using Newtonsoft.Json;
using System.Collections.Generic;

namespace TomPIT.Services
{
	public class PluginSet
	{
		private List<string> _plugins = null;

		[JsonProperty(PropertyName = "location")]
		public string Location { get; set; }

		[JsonProperty(PropertyName = "items")]
		public List<string> Items
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
