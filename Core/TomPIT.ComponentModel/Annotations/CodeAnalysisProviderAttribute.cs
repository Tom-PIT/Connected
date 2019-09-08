using System;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
	public class CodeAnalysisProviderAttribute : Attribute
	{
		private const string DesignAssembly = "TomPIT.Design";
		private const string DevelopmentAssembly = "TomPIT.Development";

		public const string NavigationUrlProvider = "TomPIT.Design.CodeAnalysis.Providers.NavigationUrlProvider, " + DesignAssembly;
		public const string NavigationViewUrlProvider = "TomPIT.Design.CodeAnalysis.Providers.NavigationViewUrlProvider, " + DesignAssembly;
		public const string RouteKeysProvider = "TomPIT.Development.CodeAnalysis.Providers.RouteKeysProvider, " + DevelopmentAssembly;
		public const string RouteSiteMapsProvider = "TomPIT.Development.CodeAnalysis.Providers.RouteSiteMapsProvider, " + DevelopmentAssembly;
		public const string MicroservicesProvider = "TomPIT.Development.CodeAnalysis.Providers.MicroServicesProvider, " + DevelopmentAssembly;
		public const string SubscriptionProvider = "TomPIT.Design.CodeAnalysis.Providers.SubscriptionProvider, " + DesignAssembly;
		public const string SubscriptionEventProvider = "TomPIT.Design.CodeAnalysis.Providers.SubscriptionEventProvider, " + DesignAssembly;
		public const string EventProvider = "TomPIT.Design.CodeAnalysis.Providers.EventProvider, " + DesignAssembly;
		public const string ConnectionProvider = "TomPIT.Design.CodeAnalysis.Providers.ConnectionProvider, " + DesignAssembly;
		public const string CommandTextProvider = "TomPIT.Design.CodeAnalysis.Providers.CommandTextProvider, " + DesignAssembly;

		public CodeAnalysisProviderAttribute() { }

		public CodeAnalysisProviderAttribute(string type)
		{
			TypeName = type;
		}
		public CodeAnalysisProviderAttribute(Type type)
		{
			Type = type;
		}

		public string TypeName { get; }
		public Type Type { get; }
	}
}