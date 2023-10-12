using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Runtime;

namespace TomPIT.Compilation;
internal static class MicroServiceCompiler
{
	static MicroServiceCompiler()
	{
		Options = CreateOptions();
	}

	private static CSharpCompilationOptions Options { get; }
	public static void Compile()
	{
		var microServices = Tenant.GetService<IMicroServiceService>().Query();

		foreach (var microService in microServices)
			Compile(microService);
	}

	private static void Compile(IMicroService microService)
	{
		if (!CompilerService.IsStageSupported(microService.Token))
			return;

		var trees = LoadSyntaxTrees(microService);

		if (trees is null || !trees.Any())
			return;

		var references = CreateReferences(microService);

		var compilation = CSharpCompilation.Create($"{microService.Name}.dll", trees, references, Options);
		var diagnostics = compilation.GetDiagnostics();

		if (diagnostics.Any(f => f.Severity == DiagnosticSeverity.Error))
			throw new Exception("Ne gre.");
	}

	private static CSharpCompilationOptions CreateOptions()
	{
		var optimization = Tenant.GetService<IRuntimeService>().Optimization;
		var optimizationLevel = optimization == EnvironmentOptimization.Debug ? OptimizationLevel.Debug : OptimizationLevel.Release;

		return new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: optimizationLevel, nullableContextOptions: NullableContextOptions.Enable);
	}

	private static List<MetadataReference> CreateReferences(IMicroService microService)
	{
		return new List<MetadataReference>();
	}
	private static List<SyntaxTree> LoadSyntaxTrees(IMicroService microService)
	{
		var components = Tenant.GetService<IComponentService>().QueryComponents(microService.Token, ComponentCategories.Code);

		if (!components.Any())
			return null;

		var result = new List<SyntaxTree>();

		foreach (var component in components)
		{
			if (Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) is not ICodeConfiguration config)
				continue;

			var sourceCode = Tenant.GetService<IComponentService>().SelectText(microService.Token, config);

			if (string.IsNullOrEmpty(sourceCode))
				continue;

			result.Add(CSharpSyntaxTree.ParseText(SourceText.From(sourceCode)));
		}

		return result;
	}
}
