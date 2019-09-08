using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TomPIT.ComponentModel
{
	public static class ComponentCategories
	{
		public static string View => "View";
		public static string Subscription => "Subscription";
		public static string Queue => "Queue";
		public static string Event => "Event";
		public static string Connection => "Connection";
		public static string SiteMap => "SiteMap";
		public static string Script => "Script";

		public static string[] ScriptCategories => new string[] { Script};

		public static bool IsScript(string category)
		{
			return ScriptCategories.FirstOrDefault(f => string.Compare(f, category, true) == 0) != null;
		}
	}
}
