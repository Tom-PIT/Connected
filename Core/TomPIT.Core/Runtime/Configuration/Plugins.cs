using System.Collections.Generic;
using System.Text.Json;

namespace TomPIT.Runtime.Configuration
{
	public static class Plugins
	{
		static Plugins()
		{
			Items = new();

			Initialize();
		}

		public static string Location { get; private set; }
		public static bool ShadowCopy { get; private set; }
		public static List<string> Items { get; }

		private static void Initialize()
		{
			if (!Shell.Configuration.RootElement.TryGetProperty("plugins", out JsonElement element))
				return;

			if (element.TryGetProperty("location", out JsonElement location))
				Location = location.GetString();

			if (element.TryGetProperty("shadowCopy", out JsonElement shadowCopy))
				ShadowCopy = shadowCopy.GetBoolean();

			if (element.TryGetProperty("items", out JsonElement items))
			{
				foreach (var item in items.EnumerateArray())
					Items.Add(item.GetString());
			}
		}
	}
}
