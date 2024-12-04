using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
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

namespace TomPIT.Compilation;
internal static class MicroServiceCompiler
{
	public static List<Assembly> _compiled;
	private const string RecompileFileName = "Recompile.txt";
	private const string BackupFolder = "Backup";
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
		Console.WriteLine("Compiling Microservices...");

		if (Instance.IsShellMode)
		{
			Console.WriteLine("Instance is in shell mode. Compilation skipped.");
			return;
		}

		var microServices = new CompilationSet();

		if (Tenant.GetService<IRuntimeService>().Stage == EnvironmentStage.Development)
		{
			if (!ShouldRecompile())
			{
				Console.WriteLine($"Compilation in development occurs only if explicitly requested by creating Recompile.txt file in '{Shell.MicroServicesFolder}' folder. No such file exists. Compilation skipped.");

				while (microServices.TryDequeue(out IMicroService? microService) && microService is not null)
					Load(microService);

				return;
			}

			Backup();
			Clean(null);
		}


		while (microServices.TryDequeue(out IMicroService? microService) && microService is not null)
			await Compile(microService, !microServices.ShouldCompile(microService));

		ReferencePaths.CopyRuntimes();

		if (Tenant.GetService<IRuntimeService>().Stage == EnvironmentStage.Development)
		{
			if (File.Exists(RecompileFile()))
				File.Delete(RecompileFile());
		}
	}

	private static async Task Compile(IMicroService microService, bool skip)
	{
		Console.Write($"Compiling {microService.Name}...");

		var rt = Tenant.GetService<IRuntimeService>();

		if (!rt.IsMicroServiceSupported(microService.Token))
		{
			Console.Write($"Not supported on {rt.Stage}.{System.Environment.NewLine}");
			return;
		}

		if (skip)
		{
			Console.Write($"Up to date.{System.Environment.NewLine}");

			Load(microService);
			return;
		}

		var trees = await LoadSyntaxTrees(microService);

		if (trees is null || !trees.Any())
		{
			Console.Write($"No syntax trees found.{System.Environment.NewLine}");
			return;
		}

		CreateBuiltInTrees(microService, trees);

		var references = CreateReferences(microService);

		var compilation = CSharpCompilation.Create(ParseAssemblyName(microService), trees, references, Options);

		Validate(microService, compilation);

		try
		{
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write($"Compiled.{System.Environment.NewLine}");
			Console.ResetColor();

			Load(microService, compilation);
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine(ex.Message);
			throw;
		}
	}

	private static Assembly? Load(IMicroService microService)
	{
		var path = Shell.ResolveAssemblyPath(ParseAssemblyName(microService));

		if (string.IsNullOrWhiteSpace(path))
			return null;

		var name = AssemblyName.GetAssemblyName(Path.GetFullPath(path));

		return AssemblyLoadContext.Default.LoadFromAssemblyName(name);
	}

	private static void Load(IMicroService microService, CSharpCompilation compilation)
	{
		var outputPath = Path.Combine(Shell.MicroServicesFolder, ParseAssemblyName(microService));
		var pdbPath = Path.Combine(Shell.MicroServicesFolder, ParsePdbName(microService));

		//if (PlatformID.Unix == System.Environment.OSVersion.Platform)
		//	pdbPath = null;

		if (File.Exists(outputPath))
			File.Delete(outputPath);

		if (File.Exists(pdbPath))
			File.Delete(pdbPath);

		var resources = ResourceGenerator.Generate(microService);

		var options = new EmitOptions(false, DebugInformationFormat.PortablePdb, pdbPath);

		using (var output = File.OpenWrite(outputPath))
		{
			using (var pdbOutput = File.OpenWrite(pdbPath))
			{
				var result = compilation.Emit(output, pdbStream: pdbOutput, manifestResources: resources, options: options);

				if (!result.Success)
					throw new Exception($"{microService.Name} - {result.Diagnostics.First(f => f.Severity == DiagnosticSeverity.Error).GetMessage()}");
			}
		}

		var assembly = Load(microService);

		if (assembly is not null)
			_compiled.Add(assembly);
	}
	private static void Validate(IMicroService microService, CSharpCompilation compilation)
	{
		var diagnostics = compilation.GetDiagnostics();

		if (diagnostics.Any(f => f.Severity == DiagnosticSeverity.Error))
		{
			var firstError = diagnostics.First(f => f.Severity == DiagnosticSeverity.Error);
			var line = firstError.Location.GetLineSpan();

			throw new Exception($"{microService.Name} - {firstError.GetMessage()} {line.Path}:{line.StartLinePosition.Line + 1}");
		}
	}

	internal static string ParseAssemblyName(IMicroService microService) => $"{microService.Name}.dll";
	private static string ParsePdbName(IMicroService microService) => $"{microService.Name}.pdb";

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

			result.Add(CSharpSyntaxTree.ParseText(SourceText.From(sourceCode, Encoding.UTF8), ParseOptions, ResolveComponentPath(config)));

			if (config is IMultiFileElement multiFile)
			{
				var additionalFiles = await multiFile.QueryAdditionalFiles();

				if (additionalFiles is null)
					continue;

				foreach (var file in additionalFiles)
				{
					if (LoadAdditionalFile(microService.Token, file) is SyntaxTree additionalTree)
						result.Add(additionalTree);
				}
			}
		}

		return result;
	}

	private static string ResolveComponentPath(IText configuration)
	{
		return $"/MicroServices/{configuration.ResolvePath()}";
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
				if (LoadAdditionalFile(microService.Token, file) is SyntaxTree additionalTree)
					result.Add(additionalTree);
			}
		}

		return result;
	}

	private static SyntaxTree? LoadAdditionalFile(Guid microService, KeyValuePair<Guid, int> token)
	{
		var text = Tenant.GetService<IComponentService>().SelectText(microService, token.Key, token.Value);
		var info = Tenant.GetService<IComponentService>().SelectTextInfo(microService, token.Key, token.Value);

		if (info is null || text is null || !text.Any())
			return null;

		return CSharpSyntaxTree.ParseText(SourceText.From(text, Encoding.UTF8), ParseOptions, info.FileName);
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

		trees.Add(CSharpSyntaxTree.ParseText(SourceText.From(text.ToString(), Encoding.UTF8), ParseOptions, $"{microService.Name}/AssemblyInfo.cs"));
	}

	private static bool ShouldRecompile()
	{
		return File.Exists(Path.Combine(Shell.MicroServicesFolder, RecompileFileName));
	}

	private static void Backup()
	{
		Console.WriteLine("Performing backup...");

		var folder = Path.Combine(Shell.MicroServicesFolder, BackupFolder);

		if (Directory.Exists(folder))
			Directory.Delete(folder, true);

		Directory.CreateDirectory(folder);

		CopyDirectory(Shell.MicroServicesFolder, folder);
	}

	private static void Clean(CompilationSet? set)
	{
		Console.WriteLine("Performing cleanup...");

		if (set is null)
		{
			foreach (var file in Directory.GetFiles(Shell.MicroServicesFolder, "*.dll", SearchOption.TopDirectoryOnly))
				File.Delete(file);

			return;
		}

		foreach (var ms in set)
		{
			var outputPath = Path.Combine(Shell.MicroServicesFolder, ParseAssemblyName(ms));

			if (File.Exists(outputPath))
				File.Delete(outputPath);
		}
	}

	private static void CopyDirectory(string source, string destination)
	{
		var directory = new DirectoryInfo(source);

		CopyFiles(directory, destination);

		var children = directory.GetDirectories();

		foreach (var child in children)
		{
			if (string.Equals(child.FullName, Path.GetFullPath(destination), StringComparison.Ordinal))
				continue;

			var destinationDir = Path.Combine(destination, child.Name);
			Directory.CreateDirectory(destinationDir);

			foreach (var sub in child.GetDirectories())
			{
				var dest = Path.Combine(destination, sub.Name);

				CopyDirectory(sub.FullName, dest);
			}
		}
	}

	private static string RecompileFile() => Path.Combine(Shell.MicroServicesFolder, RecompileFileName);
	private static void CopyFiles(DirectoryInfo directory, string destination)
	{
		foreach (var file in directory.GetFiles())
		{
			if (string.Equals(file.FullName, Path.GetFullPath(RecompileFile()), StringComparison.Ordinal))
				continue;

			var targetFilePath = Path.Combine(Path.Combine(destination, file.Name));

			file.CopyTo(targetFilePath);
		}
	}
}
