using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;

namespace TomPIT.Sys.Configuration
{
	internal static class StorageProviders
	{
		private static readonly List<string> _items;
		static StorageProviders()
		{
			_items = new();

			Initialize();
		}

		public static ImmutableList<string> Items => _items.ToImmutableList();

		private static void Initialize()
		{
			if (!Shell.Configuration.RootElement.TryGetProperty("storageProviders", out JsonElement element))
				return;

			foreach (var item in element.EnumerateArray())
				_items.Add(item.GetString());
		}
	}
}
