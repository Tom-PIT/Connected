using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Threading;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.Reflection;
using TomPIT.Runtime;

namespace TomPIT.Compilation;
internal static class Precompilation
{
	static Precompilation()
	{
		Index = new();

		if (Enabled)
		{
			if (File.Exists(IndexFileName))
			{
				var file = JsonSerializer.Deserialize<ConcurrentDictionary<string, string>>(File.ReadAllText(IndexFileName));

				if (file is not null)
					Index = file;
			}
		}
	}

	private static bool IsPrecomiling { get; set; }
	private static ConcurrentDictionary<string, string> Index { get; }
	private static ConcurrentDictionary<string, Assembly> LoadIndex { get; } = new();
	private static bool Changed { get; set; }
	private static string Directory => Path.Combine(Shell.MicroServicesFolder, "Precompiled");
	private static string IndexFileName => Path.Combine(Directory, "Index.txt");
	private static bool Enabled => !Instance.IsShellMode && Tenant.GetService<IRuntimeService>().Stage == EnvironmentStage.Production;

	public static void Save(Guid microService, Guid script, string? path)
	{
		if (!Enabled)
			return;

		var key = ParseKey(microService, script);

		EnsureDirectory();

		if (!Index.ContainsKey(key))
			Index.TryAdd(key, path ?? string.Empty);

		Changed = true;
	}

	public static void Save(IScriptDescriptor script, Microsoft.CodeAnalysis.Compilation compilation)
	{
		if (!Enabled)
			return;

		using var ms = new MemoryStream();
		var er = compilation.Emit(ms, null, null, null, null, new EmitOptions(), null, null, null, CancellationToken.None);

		ms.Seek(0, SeekOrigin.Begin);

		var key = ParseKey(script.MicroService, script.Token);
		var path = ParseFilePath(key);

		File.WriteAllBytes(path, ms.ToArray());

		Save(script.MicroService, script.Token, path);
	}

	public static List<PrecompilationDiagnostic> Precompile()
	{
		Reset();

		if (IsPrecomiling)
			throw new InvalidOperationException("Precompiling is already in progress...");

		if (!Enabled)
			throw new InvalidOperationException("Precompilation is not enabled.");

		IsPrecomiling = true;

		try
		{
			var microServices = Tenant.GetService<IMicroServiceService>().Query();
			var discovery = Tenant.GetService<IDiscoveryService>();
			var compilerService = Tenant.GetService<ICompilerService>();
			var counter = 1;
			var result = new List<PrecompilationDiagnostic>();

			Console.WriteLine($"Precompiling {microServices.Count} microservices...");

			foreach (var microService in microServices)
			{
				var components = Tenant.GetService<IComponentService>().QueryComponents(microService.Token);
				var files = new List<IText>();

				foreach (var component in components)
				{
					if (ComponentCategories.IsAssemblyCategory(component.Category))
						continue;

					var configuration = Tenant.GetService<IComponentService>().SelectConfiguration(component.Token);

					if (configuration is null)
						continue;

					var sourceFiles = discovery.Configuration.Query<IText>(configuration);

					foreach (var sourceFile in sourceFiles)
					{
						var syntax = sourceFile.GetType().GetCustomAttribute<SyntaxAttribute>();

						if (syntax is null || string.Equals(syntax.Syntax, SyntaxAttribute.CSharp, StringComparison.Ordinal))
							files.Add(sourceFile);
					}
				}

				if (files.Count > 0)
					Console.WriteLine($"Precompiling {microService.Name} ({counter} / {microServices.Count}). {files.Count} script(s).");

				counter++;
				var fileCounter = 1;

				foreach (var file in files)
				{
					var script = compilerService.GetScript(new CompilerScriptArgs(microService.Token, file));

					if (script?.Errors is not null && script.Errors.Any(f => f.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error))
					{
						Console.Error.WriteLine($"Cannot precompile {microService.Name}/{file.FileName} because it contains errors.");

						foreach (var error in script.Errors)
						{
							if (error.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
							{
								Console.Error.WriteLine($"{error.SourcePath}, line {error.StartLine}, {error.Message}.");

								result.Add(new PrecompilationDiagnostic
								{
									FileName = error.SourcePath,
									Line = error.StartLine,
									Message = error.Message
								});
							}
						}

						Console.WriteLine($"Precompilation stopped. Please correct errors and try again.");

						return result;
					}

					fileCounter++;

					if (fileCounter % 25 == 0)
						Console.WriteLine($"Precompilation {microService.Name} ({(double)fileCounter / files.Count:p0}).");
				}
			}

			var color = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"Precompilation complete.");
			Console.ForegroundColor = color;
		}
		finally
		{
			IsPrecomiling = false;
		}

		return new();
	}

	public static void Reset()
	{
		if (System.IO.Directory.Exists(Directory))
		{
			var files = new DirectoryInfo(Directory).GetFiles();

			foreach (var file in files)
				File.Delete(file.FullName);
		}

		Index.Clear();

		Changed = true;
	}

	public static void Flush()
	{
		if (!Enabled || !Changed)
			return;

		Changed = false;

		File.WriteAllText(IndexFileName, JsonSerializer.Serialize(Index));
	}

	public static bool TryLoad(Guid microService, Guid id, out Assembly? assembly)
	{
		assembly = null;

		if (!Enabled)
			return false;

		var key = ParseKey(microService, id);

		if (LoadIndex.TryGetValue(key, out Assembly? asm))
		{
			assembly = asm;

			return true;
		}

		if (Index.TryGetValue(key, out string? path))
		{
			if (string.IsNullOrWhiteSpace(path))
				return true;

			using var ms = File.OpenRead(ParseFilePath(key));
			assembly = AssemblyLoadContext.Default.LoadFromStream(ms);

			LoadIndex.TryAdd(key, assembly);

			return true;
		}

		return false;
	}

	private static string ParseKey(Guid microService, Guid id) => $"{microService}_{id}";
	private static string ParseFilePath(string key) => Path.Combine(Directory, $"{key}.dll");

	private static void EnsureDirectory()
	{
		if (!System.IO.Directory.Exists(Directory))
			System.IO.Directory.CreateDirectory(Directory);
	}
}
