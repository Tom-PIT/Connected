namespace TomPIT.ComponentModel
{
	public static class ComponentCategories
	{
		public static string ReferenceComponentName => "References";
		public static string Api => "Api";
		public static string Model => "Model";
		public static string View => "View";
		public static string Subscription => "Subscription";
		public static string Queue => "Queue";
		//public static string Event => "Event";
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
		public static string Static => "Static";
		public static string EmbeddedAssembly => "EmbeddedAssembly";
		public static string NuGetPackage => "NuGetPackage";
		public static string EmbeddedNuGetPackage => "EmbeddedNuGetPackage";
		public static string FileAssembly => "FileAssembly";
		public static string Text => "Text";
		public static string MailTemplate => "MailTemplate";
		public static string Inbox => "Inbox";
		public static string SearchCatalog => "SearchCatalog";
		public static string Installer => "Installer";
		public static string Runtime => "Runtime";
		public static string EventBinder => "EventBinder";
		public static string Management => "Management";
		public static string Settings => "Settings";
		public static string MicroServiceInfo => "MicroServiceInfo";
		public static string IoTHub => "IoTHub";
		public static string IoTSchema => "IoTSchema";
		public static string IoCContainer => "IoCContainer";
		public static string IoCEndpoint => "IoCEndpoint";
		public static string DependencyInjection => "DependencyInjection";
		public static string PermissionDescriptor => "PermissionDescriptor";
		public static string UIDependencyInjection => "UIDependencyInjection";
		public static string Report => "Report";
		public static string SmtpConnection => "SmtpConnection";
		public static string UnitTest => "UnitTest";

		public static string NameSpacePublicScript => "PublicScript";
		public static string NameSpaceInternalScript => "InternalScript";
		public static string NameSpaceView => "View";
		public static string NameSpaceData => "Data";
		public static string NameSpaceReference => "Reference";
		public static string NameSpaceResource => "Resource";
		public static string NameSpaceMiddleware => "Middleware";
		public static string NameSpaceDeployment => "Deployment";
		public static string NameSpaceQuality => "Quality";
		public static string NameSpaceNuGet=> "NuGetPackage";

		public static string[] ScriptCategories => new string[] { Script, IoCContainer };
		public static string ResolveNamespace(string category)
		{
			if (string.Compare(category, Api, true) == 0
				|| string.Compare(category, Script, true) == 0
				|| string.Compare(category, IoCContainer, true) == 0
				|| string.Compare(category, Model, true) == 0
				|| string.Compare(category, Settings, true) == 0)
				return NameSpacePublicScript;
			else if (string.Compare(category, Subscription, true) == 0
				|| string.Compare(category, Queue, true) == 0
				|| string.Compare(category, SiteMap, true) == 0
				|| string.Compare(category, BigDataPartition, true) == 0
				|| string.Compare(category, DistributedEvent, true) == 0
				|| string.Compare(category, HostedWorker, true) == 0
				|| string.Compare(category, SearchCatalog, true) == 0
				|| string.Compare(category, IoTHub, true) == 0
				|| string.Compare(category, IoTSchema, true) == 0
				|| string.Compare(category, IoCEndpoint, true) == 0)
				return NameSpaceInternalScript;
			else if (string.Compare(category, View, true) == 0
				|| string.Compare(category, MasterView, true) == 0
				|| string.Compare(category, Partial, true) == 0
				|| string.Compare(category, MailTemplate, true) == 0)
				return NameSpaceView;
			else if (string.Compare(category, Connection, true) == 0)
				return NameSpaceData;
			else if (string.Compare(category, Reference, true) == 0)
				return NameSpaceReference;
			else if (string.Compare(category, Theme, true) == 0
				|| string.Compare(category, ScriptBundle, true) == 0
				|| string.Compare(category, StringTable, true) == 0
				|| string.Compare(category, Media, true) == 0
				|| string.Compare(category, EmbeddedAssembly, true) == 0
				|| string.Compare(category, FileAssembly, true) == 0)
				return NameSpaceResource;
			else if (string.Compare(category, EventBinder, true) == 0)
				return NameSpaceMiddleware;
			else if (string.Compare(category, Installer, true) == 0)
				return NameSpaceDeployment;
			else if (string.Compare(category, UnitTest, true) == 0)
				return NameSpaceQuality;
			else if (string.Compare(category, NuGetPackage, true) == 0
				|| string.Compare(category, EmbeddedNuGetPackage, true) == 0)
				return NameSpaceNuGet;
			else
				return "Default";
		}
	}
}
