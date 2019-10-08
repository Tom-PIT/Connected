using System;

namespace TomPIT.Annotations.Design
{
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
	public class CompletionItemProviderAttribute : Attribute
	{
		public const string ConnectionProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.ConnectionCompletionProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string SiteMapViewProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.SiteMapViewCompletionProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string PartialProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.PartialCompletionProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string StringTableStringProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.StringTableStringProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string StringTableProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.StringTableProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string CommandTextProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.CommandTextProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string DistributedEventProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.DistributedEventProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string ApiProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.ApiProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string ApiOperationProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.ApiOperationProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string ApiOperationParameterProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.ApiOperationParameterProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string SearchCatalogProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.SearchCatalogProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string RouteKeyProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.RouteKeyProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string RouteSiteMapsProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.RouteSiteMapsProvider, " + SystemAssemblies.DevelopmentAssembly;
		//public const string IoCContainersProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.IoCContainersProvider, " + SystemAssemblies.DevelopmentAssembly;
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
