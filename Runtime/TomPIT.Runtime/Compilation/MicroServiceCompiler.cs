﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Reflection;
using TomPIT.Runtime;

namespace TomPIT.Compilation;
internal static class MicroServiceCompiler
{
	static MicroServiceCompiler()
	{
		Options = CreateOptions();

		ParseOptions = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.None, SourceCodeKind.Regular);
	}

	private static CSharpCompilationOptions Options { get; }
	private static CSharpParseOptions ParseOptions { get; }
	public static void Compile()
	{
		var microServices = new CompilationSet();

		while (microServices.TryDequeue(out IMicroService microService))
			Compile(microService);
	}

	private static void Compile(IMicroService microService)
	{
		if (!CompilerService.IsStageSupported(microService.Token))
			return;

		if (!ShouldCompile(microService))
		{
			LoadExisting(microService);
			return;
		}

		var trees = LoadSyntaxTrees(microService);

		if (trees is null || !trees.Any())
			return;

		CreateBuiltInTrees(microService, trees);

		var references = CreateReferences(microService);

		var compilation = CSharpCompilation.Create(ParseAssemblyName(microService), trees, references, Options);

		Validate(microService, compilation);
		Load(microService, compilation);
	}

	private static void LoadExisting(IMicroService microService)
	{
		var path = Shell.ResolveAssemblyPath(ParseAssemblyName(microService));

		Assembly.LoadFile(Path.GetFullPath(path));
	}

	private static void Load(IMicroService microService, CSharpCompilation compilation)
	{
		var outputPath = Path.Combine("/microServices", ParseAssemblyName(microService));
		var pdbPath = Path.Combine("/microServices", ParsePdbName(microService));

		if (File.Exists(outputPath))
			File.Delete(outputPath);

		if (File.Exists(pdbPath))
			File.Delete(pdbPath);

		var result = compilation.Emit(outputPath, pdbPath);

		if (!result.Success)
			throw new Exception($"{microService.Name} - {result.Diagnostics.First(f => f.Severity == DiagnosticSeverity.Error).GetMessage()}");

		var path = Shell.ResolveAssemblyPath(ParseAssemblyName(microService));

		Assembly.LoadFile(path);
	}
	private static void Validate(IMicroService microService, CSharpCompilation compilation)
	{
		var diagnostics = compilation.GetDiagnostics();

		if (diagnostics.Any(f => f.Severity == DiagnosticSeverity.Error))
			throw new Exception($"{microService.Name} - {diagnostics.First(f => f.Severity == DiagnosticSeverity.Error).GetMessage()}");
	}

	private static string ParseAssemblyName(IMicroService microService) => $"{microService.Name}.dll";
	private static string ParsePdbName(IMicroService microService) => $"{microService.Name}.pdb";
	private static bool ShouldCompile(IMicroService microService)
	{
		if (string.IsNullOrEmpty(microService.Version))
			return true;

		var path = Shell.ResolveAssemblyPath(ParseAssemblyName(microService));

		if (string.IsNullOrEmpty(path))
			return true;

		var msVersion = Version.Parse(microService.Version);

		try
		{
			var version = AssemblyName.GetAssemblyName(path).Version;

			if (version is null)
				return true;

			return version != msVersion;
		}
		catch
		{
			return true;
		}
	}

	private static CSharpCompilationOptions CreateOptions()
	{
		var optimization = Tenant.GetService<IRuntimeService>().Optimization;
		var optimizationLevel = optimization == EnvironmentOptimization.Debug ? OptimizationLevel.Debug : OptimizationLevel.Release;

		return new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: optimizationLevel, nullableContextOptions: NullableContextOptions.Enable);
	}

	private static List<MetadataReference> CreateReferences(IMicroService microService)
	{
		var references = Tenant.GetService<IDiscoveryService>().MicroServices.References.Select(microService.Token);
		var result = new List<MetadataReference>();
		var existing = new List<string>();

		result.AddRange(FrameworkFiles.FrameworkReferences);
		result.AddRange(FrameworkFiles.AspNetCoreReferences);

		foreach (var file in FrameworkFiles.TomPITFileNames)
			AddReference(Path.Combine(FrameworkFiles.TomPITDirectory, file), existing, result);

		if (references is null)
			return result;

		foreach (var assembly in references.Assemblies)
		{
			if (string.IsNullOrWhiteSpace(assembly.AssemblyName))
				continue;

			try
			{
				var name = assembly.AssemblyName.EndsWith(".dll") ? assembly.AssemblyName : $"{assembly.AssemblyName}.dll";

				AddReference(Shell.ResolveAssemblyPath(name), existing, result);
			}
			catch { }
		}

		foreach (var package in references.Packages)
			LoadPackage(package.PackageName, package.Version, existing, result);

		return result;
	}

	private static void AddReference(string file, List<string> existing, List<MetadataReference> references)
	{
		if (string.IsNullOrWhiteSpace(file))
			return;

		if (existing.Contains(file, StringComparer.OrdinalIgnoreCase))
			return;

		existing.Add(file);

		var reference = MetadataReference.CreateFromFile(file);

		if (reference is null)
			return;

		references.Add(reference);
	}

	private static void LoadPackage(string packageName, string packageVersion, List<string> existingPaths, List<MetadataReference> references)
	{
		if (string.IsNullOrWhiteSpace(packageName) || string.IsNullOrWhiteSpace(packageVersion))
			return;

		try
		{
			var assemblies = Tenant.GetService<INuGetService>().Resolve(packageName, packageVersion, false);

			if (assemblies is null)
				return;

			foreach (var asm in assemblies)
				AddReference(asm.Location, existingPaths, references);
		}
		catch { }
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

			result.Add(CSharpSyntaxTree.ParseText(SourceText.From(sourceCode, Encoding.UTF8), ParseOptions, $"{component.Name}.cs"));
		}

		return result;
	}

	private static void CreateBuiltInTrees(IMicroService microService, List<SyntaxTree> trees)
	{
		CreateAssemblyInfo(microService, trees);
	}

	private static void CreateAssemblyInfo(IMicroService microService, List<SyntaxTree> trees)
	{
		if (string.IsNullOrEmpty(microService.Version))
			return;

		var text = new StringBuilder();

		text.AppendLine();
		text.AppendLine("using System.Reflection;");
		text.AppendLine();
		text.AppendLine($"[assembly: AssemblyVersion(\"{microService.Version}\")]");
		text.AppendLine($"[assembly: AssemblyFileVersion(\"{microService.Version}\")]");

		trees.Add(CSharpSyntaxTree.ParseText(SourceText.From(text.ToString(), Encoding.UTF8), ParseOptions, "AssemblyInfo.cs"));
	}
}