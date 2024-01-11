using Microsoft.Extensions.Configuration;

using System;
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
			var storageProviders = Shell.Configuration.GetSection("storageProviders").Get<string[]>();

			_items.AddRange(storageProviders);
		}
	}
}
