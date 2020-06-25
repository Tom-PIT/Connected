using System;

namespace TomPIT.Annotations.Design
{
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
	public class CompletionItemProviderAttribute : Attribute
	{
		public const string ConnectionProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.ConnectionCompletionProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string SiteMapViewProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.SiteMapViewCompletionProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string PartialProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.PartialCompletionProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string BundleProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.BundleCompletionProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string StringTableStringProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.StringTableStringProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string StringTableProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.StringTableProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string CommandTextProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.CommandTextProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string DistributedEventProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.DistributedEventProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string DistributedEventPropertyProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.DistributedEventPropertyProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string ApiProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.ApiProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string ApiOperationProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.ApiOperationProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string ApiOperationParameterProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.ApiOperationParameterProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string SearchCatalogProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.SearchCatalogProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string RouteKeyProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.RouteKeyProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string RouteSiteMapsProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.RouteSiteMapsProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string DataHubEndpointProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.DataHubEndpointProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string ExtenderProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.ExtenderCompletionProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string QueueWorkersProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.QueueWorkersProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string IoCOperationsProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.IoCOperationsProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string IoCOperationParametersProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.IoCOperationParametersProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string MediaProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.MediaProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string HostedWorkerProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.HostedWorkerProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string ReportProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.ReportProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string SubscriptionProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.SubscriptionProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string SubscriptionEventProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.SubscriptionEventProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string BigDataPartitionProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.BigDataPartitionProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string ModelQueryOperationProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.ModelQueryOperationProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string ModelExecuteOperationProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.ModelExecuteOperationProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string ModelOperationParametersProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.ModelOperationParametersProvider, " + SystemAssemblies.DevelopmentAssembly;
		public CompletionItemProviderAttribute() { }

		public CompletionItemProviderAttribute(string type)
		{
			TypeName = type;
		}
		public CompletionItemProviderAttribute(Type type)
		{
			Type = type;
		}

		public string TypeName { get; }
		public Type Type { get; }
	}
}
