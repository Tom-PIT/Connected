﻿namespace TomPIT.ComponentModel
{
	public static class ComponentCategories
	{
		public static string ReferenceComponentName => "References";
		public static string Api => "Api";
		public static string Model => "Model";
		public static string Code => "Code";
		public static string AssemblyResource => "AssemblyResource";
		public static string View => "View";
		public static string Subscription => "Subscription";
		public static string Queue => "Queue";
		//public static string Event => "Event";
		public static string Connection => "Connection";
		public static string Entity => "Entity";
		public static string SiteMap => "SiteMap";
		public static string Script => "Script";
		public static string Middleware => "Middleware";
		public static string BigDataPartition => "BigDataPartition";
		public static string Reference => "Reference";
		public static string MasterView => "MasterView";
		public static string Partial => "Partial";
		public static string Theme => "Theme";
		public static string ScriptBundle => "ScriptBundle";
		public static string DistributedEvent => "DistributedEvent";
		public static string HostedWorker => "HostedWorker";
		public static string HostedService => "HostedService";
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

		public static string NameSpaceCode => "Code";
		public static string NameSpaceAssemblyResources => "AssemblyResources";
		public static string NameSpacePublicScript => "PublicScript";
		public static string NameSpaceInternalScript => "InternalScript";
		public static string NameSpaceView => "View";
		public static string NameSpaceData => "Data";
		public static string NameSpaceReference => "Reference";
		public static string NameSpaceResource => "Resource";
		public static string NameSpaceMiddleware => "Middleware";
		public static string NameSpaceDeployment => "Deployment";
		public static string NameSpaceQuality => "Quality";
		public static string NameSpaceNuGet => "NuGetPackage";

		public static string[] ScriptCategories => new string[] { Script, IoCContainer };

		public static string[] NamespaceCategories(string nameSpace)
		{
			if (string.Equals(nameSpace, NameSpacePublicScript, System.StringComparison.OrdinalIgnoreCase))
				return new string[] { Api, Script, Model, Settings, Entity };
			else if (string.Equals(nameSpace, NameSpaceInternalScript, System.StringComparison.OrdinalIgnoreCase))
				return new string[] { Subscription, IoCContainer, Queue, SiteMap, BigDataPartition, DistributedEvent, HostedWorker, HostedService, SearchCatalog, IoTHub, IoTSchema, IoCEndpoint, Middleware };
			else if (string.Equals(nameSpace, NameSpaceView, System.StringComparison.OrdinalIgnoreCase))
				return new string[] { View, MasterView, Partial, MailTemplate };
			else if (string.Equals(nameSpace, NameSpaceData, System.StringComparison.OrdinalIgnoreCase))
				return new string[] { Connection };
			else if (string.Equals(nameSpace, NameSpaceReference, System.StringComparison.OrdinalIgnoreCase))
				return new string[] { Reference };
			else if (string.Equals(nameSpace, NameSpaceResource, System.StringComparison.OrdinalIgnoreCase))
				return new string[] { Theme, ScriptBundle, StringTable, Media, EmbeddedAssembly, FileAssembly };
			else if (string.Equals(nameSpace, NameSpaceMiddleware, System.StringComparison.OrdinalIgnoreCase))
				return new string[] { EventBinder };
			else if (string.Equals(nameSpace, NameSpaceDeployment, System.StringComparison.OrdinalIgnoreCase))
				return new string[] { Installer };
			else if (string.Equals(nameSpace, NameSpaceQuality, System.StringComparison.OrdinalIgnoreCase))
				return new string[] { UnitTest };
			else if (string.Equals(nameSpace, NameSpaceNuGet, System.StringComparison.OrdinalIgnoreCase))
				return new string[] { NuGetPackage, EmbeddedNuGetPackage };
			else if (string.Equals(nameSpace, NameSpaceCode, System.StringComparison.OrdinalIgnoreCase))
				return new string[] { Code };
			else if (string.Equals(nameSpace, NameSpaceAssemblyResources, System.StringComparison.OrdinalIgnoreCase))
				return new string[] { AssemblyResource };
			else
				return new string[0];
		}
		public static string ResolveNamespace(string category)
		{
			if (string.Compare(category, Api, true) == 0
				 || string.Compare(category, Script, true) == 0
				 || string.Compare(category, Model, true) == 0
				 || string.Compare(category, Settings, true) == 0
				|| string.Compare(category, Entity, true) == 0)
				return NameSpacePublicScript;
			else if (string.Compare(category, Subscription, true) == 0
				 || string.Compare(category, IoCContainer, true) == 0
				 || string.Compare(category, Queue, true) == 0
				 || string.Compare(category, SiteMap, true) == 0
				 || string.Compare(category, BigDataPartition, true) == 0
				 || string.Compare(category, DistributedEvent, true) == 0
				 || string.Compare(category, HostedWorker, true) == 0
				 || string.Compare(category, HostedService, true) == 0
				 || string.Compare(category, SearchCatalog, true) == 0
				 || string.Compare(category, IoTHub, true) == 0
				 || string.Compare(category, IoTSchema, true) == 0
				 || string.Compare(category, IoCEndpoint, true) == 0
				 || string.Compare(category, Middleware, true) == 0)
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
			else if (string.Compare(category, Code, true) == 0)
				return NameSpaceCode;
			else if (string.Compare(category, AssemblyResource, true) == 0)
				return NameSpaceAssemblyResources;
			else
				return "Default";
		}

		public static bool IsAssemblyCategory(string category)
		{
			return string.Equals(category, Code, System.StringComparison.OrdinalIgnoreCase)
				|| string.Equals(category, AssemblyResource, System.StringComparison.OrdinalIgnoreCase);
		}
	}
}
