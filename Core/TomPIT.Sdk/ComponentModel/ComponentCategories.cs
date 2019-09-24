using System.Linq;

namespace TomPIT.ComponentModel
{
	public static class ComponentCategories
	{
		public static string Api => "Api";
		public static string View => "View";
		public static string Subscription => "Subscription";
		public static string Queue => "Queue";
		public static string Event => "Event";
		public static string Connection => "Connection";
		public static string SiteMap => "SiteMap";
		public static string Script => "Script";
		public static string BigDataPartition => "BigDataPartition";
		public static string Reference => "Reference";
		public static string MasterView => "MasterView";
		public static string Partial => "Partial";
		public static string Theme => "Theme";
		public static string ScriptBundle => "ScriptBundle";
		public static string DistributedEvent => "DistributedEvent";
		public static string HostedWorker => "HostedWorker";
		public static string StringTable => "StringTable";
		public static string Media => "Media";
		public static string EmbeddedAssembly => "EmbeddedAssembly";
		public static string FileAssembly => "FileAssembly";
		public static string MailTemplate => "MailTemplate";
		public static string SearchCatalog => "SearchCatalog";
		public static string EventBinder => "EventBinder";
		public static string IoTHub => "IoTHub";
		public static string IoTSchema => "IoTSchema";
		public static string IoC => "IoC";
		public static string[] ScriptCategories => new string[] { Script };

		public static bool IsScript(string category)
		{
			return ScriptCategories.FirstOrDefault(f => string.Compare(f, category, true) == 0) != null;
		}
	}
}
