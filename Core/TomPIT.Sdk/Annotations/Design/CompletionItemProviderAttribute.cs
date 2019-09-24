using System;

namespace TomPIT.Annotations.Design
{
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
	public class CompletionItemProviderAttribute : Attribute
	{
		public const string ConnectionProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.ConnectionCompletionProvider, " + SystemAssemblies.DevelopmentAssembly;
		public const string SiteMapViewUrlProvider = "TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders.SiteMapViewUrlCompletionProvider, " + SystemAssemblies.DevelopmentAssembly;

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
