using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Reflection;
using TomPIT.Runtime;
using TomPIT.Storage;

namespace TomPIT.Compilation;
internal static class MicroServiceCompiler
{
	public static List<Assembly> _compiled;
	static MicroServiceCompiler()
	{
		_compiled = new();

		Options = CreateOptions();

		ParseOptions = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.None, SourceCodeKind.Regular);
	}

	private static CSharpCompilationOptions Options { get; }
	private static CSharpParseOptions ParseOptions { get; }
	public static ImmutableArray<Assembly> Compiled => _compiled.ToImmutableArray();
	public static async Task Compile()
	{
		var microServices = new CompilationSet();

		while (microServices.TryDequeue(out IMicroService microService))
			await Compile(microService);
	}

	private static async Task Compile(IMicroService microService)
	{
		if (!Tenant.GetService<IRuntimeService>().IsMicroServiceSupported(microService.Token))
			return;

		if (!ShouldCompile(microService))
		{
			Load(microService);
			return;
		}

		var trees = await LoadSyntaxTrees(microService);

		if (trees is null || !trees.Any())
			return;

		CreateBuiltInTrees(microService, trees);

		var references = CreateReferences(microService);

		var compilation = CSharpCompilation.Create(ParseAssemblyName(microService), trees, references, Options);

		Validate(microService, compilation);

		try
		{
			Load(microService, compilation);
		}
		catch (Exception ex)
		{
			//TODO log to standard logging channel
		}
	}

	private static Assembly Load(IMicroService microService)
	{
		var path = Shell.ResolveAssemblyPath(ParseAssemblyName(microService));

		if (string.IsNullOrWhiteSpace(path))
			return;

		var name = AssemblyName.GetAssemblyName(Path.GetFullPath(path));

		return AssemblyLoadContext.Default.LoadFromAssemblyName(name);
	}

	private static void Load(IMicroService microService, CSharpCompilation compilation)
	{
		var outputPath = Path.Combine(Shell.MicroServicesFolder, ParseAssemblyName(microService));
		var pdbPath = Path.Combine(Shell.MicroServicesFolder, ParsePdbName(microService));

		if (File.Exists(outputPath))
			File.Delete(outputPath);

		if (File.Exists(pdbPath))
			File.Delete(pdbPath);

		var resources = ResourceGenerator.Generate(microService);
		var result = compilation.Emit(outputPath, pdbPath, manifestResources: resources);

		if (!result.Success)
			throw new Exception($"{microService.Name} - {result.Diagnostics.First(f => f.Severity == DiagnosticSeverity.Error).GetMessage()}");

		_compiled.Add(Load(microService));
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
#if NORECOMPILE
		return false;
#endif

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

			if (version != msVersion)
				return true;

			var references = Tenant.GetService<IDiscoveryService>().MicroServices.References.References(microService.Token, false);

			foreach (var reference in references)
			{
				var assembly = $"{reference.Name}.dll";

				if (_compiled.Any(f => string.Equals(f.GetName().Name, assembly, StringComparison.Ordinal)))
					return true;
			}
		}
		catch
		{
			return true;
		}

		return false;
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

		foreach (var ms in references.MicroServices)
		{
			if (string.IsNullOrWhiteSpace(ms.MicroService))
				continue;

			AddReference(Shell.ResolveAssemblyPath($"{ms.MicroService}.dll"), existing, result);
		}

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

		var packageAssemblies = new List<Assembly>();
		var unmanagedPaths = new List<string>();

		foreach (var package in references.Packages)
		{
			var prepared = PreparePackage(package.PackageName, package.Version, existing, result);

			if (prepared.Item1 is not null && prepared.Item1.Any())
				packageAssemblies.AddRange(prepared.Item1);

			if (prepared.Item2 is not null && prepared.Item2.Any())
				unmanagedPaths.AddRange(prepared.Item2);
		}

		packageAssemblies = ConsolidateAssemblies(packageAssemblies);

		foreach (var assembly in packageAssemblies)
			AddReference(assembly.Location, existing, result);

		ReferencePaths.Update(packageAssemblies);
		ReferencePaths.Update(unmanagedPaths);

		return result;
	}

	private static List<Assembly> ConsolidateAssemblies(List<Assembly> assemblies)
	{
		var result = new List<Assembly>();

		foreach (var assembly in assemblies)
		{
			var proposed = AssemblyName.GetAssemblyName(assembly.Location);
			var add = true;

			for (var i = 0; i < result.Count; i++)
			{
				var existing = AssemblyName.GetAssemblyName(result[i].Location);

				if (!string.Equals(existing.Name, proposed.Name, StringComparison.Ordinal))
					continue;

				add = false;

				if (proposed.Version > existing.Version)
				{
					result[i] = assembly;
					break;
				}
			}

			if (add)
				result.Add(assembly);
		}

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

	private static (ImmutableList<Assembly>?, ImmutableList<string>?) PreparePackage(string packageName, string packageVersion, List<string> existingPaths, List<MetadataReference> references)
	{
		if (string.IsNullOrWhiteSpace(packageName) || string.IsNullOrWhiteSpace(packageVersion))
			return (null, null);

		try
		{
			var assemblies = Tenant.GetService<INuGetService>().Resolve(packageName, packageVersion, false);
			var paths = Tenant.GetService<INuGetService>().ResolveRuntimePaths(packageName, packageVersion);

			return (assemblies, paths);
		}
		catch
		{
			return (null, null);
		}
	}

	private static async Task<List<SyntaxTree>?> LoadSyntaxTrees(IMicroService microService)
	{
		var result = new List<SyntaxTree>();

		if (await LoadComponents(microService) is List<SyntaxTree> trees)
			result.AddRange(trees);

		if (await LoadResources(microService) is List<SyntaxTree> resourcesTrees)
			result.AddRange(resourcesTrees);

		return result;
	}

	private static async Task<List<SyntaxTree>?> LoadComponents(IMicroService microService)
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

			if (config is IMultiFileElement multiFile)
			{
				var additionalFiles = await multiFile.QueryAdditionalFiles();

				if (additionalFiles is null)
					continue;

				foreach (var file in additionalFiles)
				{
					if (LoadAdditionalFile(file) is SyntaxTree additionalTree)
						result.Add(additionalTree);
				}
			}
		}

		return result;
	}

	private static async Task<List<SyntaxTree>?> LoadResources(IMicroService microService)
	{
		var resources = Tenant.GetService<IComponentService>().QueryConfigurations(microService.Token, ComponentCategories.AssemblyResource);

		if (!resources.Any())
			return null;

		var result = new List<SyntaxTree>();

		foreach (var configuration in resources)
		{
			if (configuration is not IAssemblyResourceConfiguration config)
				continue;

			var additionalFiles = await config.QueryAdditionalFiles();

			if (additionalFiles is null)
				continue;

			foreach (var file in additionalFiles)
			{
				if (LoadAdditionalFile(file) is SyntaxTree additionalTree)
					result.Add(additionalTree);
			}
		}

		return result;
	}

	private static SyntaxTree? LoadAdditionalFile(Guid token)
	{
		var blob = Tenant.GetService<IStorageService>().Select(token);
		var text = Tenant.GetService<IStorageService>().Download(token);

		if (text is null || text.Content is null || !text.Content.Any())
			return null;

		var sourceCode = Encoding.UTF8.GetString(text.Content);

		return CSharpSyntaxTree.ParseText(SourceText.From(sourceCode, Encoding.UTF8), ParseOptions, blob.FileName);
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
