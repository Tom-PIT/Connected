using System;

namespace TomPIT.Annotations.Design
{
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
	public sealed class AnalyzerAttribute : Attribute
	{
		public const string StringTableAnalyzer = "TomPIT.Development.TextEditor.CSharp.Services.Analyzers.StringTableAnalyzer, " + SystemAssemblies.DevelopmentAssembly;
		public const string StringAnalyzer = "TomPIT.Development.TextEditor.CSharp.Services.Analyzers.StringAnalyzer, " + SystemAssemblies.DevelopmentAssembly;
		public const string EventAnalyzer = "TomPIT.Development.TextEditor.CSharp.Services.Analyzers.EventAnalyzer, " + SystemAssemblies.DevelopmentAssembly;
		public const string ViewAnalyzer = "TomPIT.Development.TextEditor.CSharp.Services.Analyzers.ViewAnalyzer, " + SystemAssemblies.DevelopmentAssembly;
		public const string NavigationContextAnalyzer = "TomPIT.Development.TextEditor.CSharp.Services.Analyzers.NavigationContextAnalyzer, " + SystemAssemblies.DevelopmentAssembly;
		public const string RouteKeyAnalyzer = "TomPIT.Development.TextEditor.CSharp.Services.Analyzers.RouteKeyAnalyzer, " + SystemAssemblies.DevelopmentAssembly;
		public const string ApiOperationAnalyzer = "TomPIT.Development.TextEditor.CSharp.Services.Analyzers.ApiOperationAnalyzer, " + SystemAssemblies.DevelopmentAssembly;
		public const string BundleAnalyzer = "TomPIT.Development.TextEditor.CSharp.Services.Analyzers.BundleAnalyzer, " + SystemAssemblies.DevelopmentAssembly;
		public AnalyzerAttribute() { }

		public AnalyzerAttribute(string type)
		{
			TypeName = type;
		}
		public AnalyzerAttribute(Type type)
		{
			Type = type;
		}

		public string TypeName { get; }
		public Type Type { get; }
	}
}
