using Microsoft.Extensions.Configuration;

using System.Collections.Generic;

namespace TomPIT.Runtime.Configuration;

public static class Plugins
{
	private static readonly PluginBindings _binder = new();

	static Plugins()
	{
		Initialize();
	}

	public static string? Location => _binder.Location;
	public static bool ShadowCopy => _binder.ShadowCopy;
	public static List<string>? Items => _binder.Items;

	private static void Initialize()
	{
		Shell.Configuration.Bind("plugins", _binder);
	}
	private class PluginBindings
	{
		public string? Location { get; set; }
		public bool ShadowCopy { get; set; }
		public List<string>? Items { get; set; }
	}
}
