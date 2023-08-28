using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Reflection;
using TomPIT.Runtime;

namespace TomPIT.Compilation
{
	internal class CompilerScript : TenantObject, IDisposable
	{
		public CompilerScript(ITenant tenant, Guid microService, IText sourceCode) : base(tenant)
		{
			MicroService = microService;
			SourceCode = sourceCode;
		}
		public Guid MicroService { get; }
		public IText SourceCode { get; }
		public List<Guid> ScriptReferences { get; private set; }
		private IScriptContext ScriptContext { get; set; }
		public void Create()
		{
			string code = Tenant.GetService<IComponentService>().SelectText(MicroService, SourceCode);

			if (string.IsNullOrWhiteSpace(code))
				return;

			var scriptName = ResolveScriptName();
			code = $"#load \"{scriptName}\"{System.Environment.NewLine}";

			var msv = Tenant.GetService<IMicroServiceService>().Select(MicroService);

			using var loader = new InteractiveAssemblyLoader();
			ScriptContext = Tenant.GetService<ICompilerService>().CreateScriptContext(SourceCode);

			var refs = new List<Guid>();

			foreach (var reference in ScriptContext.SourceFiles)
				refs.Add(reference.Value.Id);

			if (refs.Count > 0)
				ScriptReferences = refs;

			var options = ScriptOptions.Default
					.WithImports(Usings)
					.WithReferences(References)
					.WithSourceResolver(new ScriptResolver(Tenant, MicroService))
					.WithMetadataResolver(new AssemblyResolver(Tenant, MicroService, true))
					.WithEmitDebugInformation(Tenant.GetService<IRuntimeService>().Stage != EnvironmentStage.Production)
					.WithFilePath(SourceCode.FileName)
					.WithFileEncoding(Encoding.UTF8);

			foreach (var reference in ScriptContext.References)
			{
				if (reference.Value == ImmutableArray<PortableExecutableReference>.Empty)
					continue;

				foreach (var executable in reference.Value)
				{
					/*
		* in memory assembly
		*/
					if (string.IsNullOrEmpty(executable.FilePath))
					{
						var tokens = reference.Key.Split('/');
						var ms = MicroService;
						var name = reference.Key;

						if (tokens.Length > 1)
						{
							ms = Tenant.GetService<IMicroServiceService>().Select(tokens[0]).Token;
							name = tokens[1];
						}

						var asm = AssemblyResolver.LoadDependency(Tenant, ms, name);

						if (asm != null)
							loader.RegisterDependency(asm);
					}
					else
					{
						var name = AssemblyLoadContext.GetAssemblyName(executable.FilePath);

						if (name != null)
						{
							var asm = AssemblyLoadContext.Default.LoadFromAssemblyName(name);

							if (asm != null)
								loader.RegisterDependency(asm);
						}
					}
				}
			}

			Script = CreateScript($"{code};{System.Environment.NewLine}{GenerateStaticCode()}", options, loader);
		}

		private bool ResolveRequiresSyntaxRoot()
		{
			var component = Tenant.GetService<IComponentService>().SelectComponent(SourceCode.Configuration().Component);
			/*
			 * There is a bug in the configuration files. IoC containers have PublicScript namespace which is wrong because they
			 * can't be references thus we are handling IoC containers separately.
			 */
			return !string.Equals(component.Category, ComponentCategories.IoCContainer, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(component.NameSpace, ComponentCategories.NameSpacePublicScript, StringComparison.OrdinalIgnoreCase);
		}

		private string ResolveScriptName()
		{
			var element = Tenant.GetService<IDiscoveryService>().Configuration.Find(SourceCode.Configuration().Component, SourceCode.Id) as IText;
			var ms = Tenant.GetService<IMicroServiceService>().Select(element.Configuration().MicroService());

			if (element is IConfiguration config)
				return $"{ms.Name}/{element.Configuration().ComponentName()}.csx";
			else
				return $"{ms.Name}/{element.Configuration().ComponentName()}/{element.FileName}";
		}

		protected virtual Script<object> CreateScript(string sourceCode, ScriptOptions options, InteractiveAssemblyLoader loader)
		{
			return CSharpScript.Create(sourceCode, options: options, assemblyLoader: loader);
		}
		protected virtual List<Assembly> References => new List<Assembly>
							{
									 CompilerService.LoadSystemAssembly("TomPIT.Core"),
									 CompilerService.LoadSystemAssembly("TomPIT.ComponentModel"),
									 CompilerService.LoadSystemAssembly("TomPIT.Sdk"),
									 CompilerService.LoadSystemAssembly("TomPIT.Runtime"),
									 CompilerService.LoadSystemAssembly("Newtonsoft.Json")
							};
		protected virtual string[] Usings => Array.Empty<string>();

		public void Dispose()
		{
			Script = null;
		}

		public Script<object> Script { get; private set; }

		private string GenerateStaticCode()
		{
			var sb = new StringBuilder();

			sb.AppendLine($"public static class {CompilerService.ScriptInfoClassName}");
			sb.AppendLine("{");
			sb.AppendLine("private static readonly System.Collections.Generic.List<TomPIT.Compilation.SourceTypeDescriptor> _sourceTypes = new System.Collections.Generic.List<TomPIT.Compilation.SourceTypeDescriptor>{");

			foreach (var file in ScriptContext.SourceFiles)
			{
				var config = file.Value.Configuration();
				var component = Tenant.GetService<IComponentService>().SelectComponent(config.Component);
				var manifest = Tenant.GetService<IDiscoveryService>().Manifests.SelectScript(config.MicroService(), component.Token, file.Value.Id);

				if (manifest is not null)
				{
					foreach (var manifestType in manifest.DeclaredTypes)
					{
						sb.AppendLine("new TomPIT.Compilation.SourceTypeDescriptor{");
						sb.AppendLine($"Component = new System.Guid(\"{component.Token}\"),");
						sb.AppendLine($"ContainingType = \"{manifestType.ContainingType}\",");
						sb.AppendLine($"TypeName = \"{manifestType.Name}\",");
						sb.AppendLine($"Script = new System.Guid(\"{file.Value.Id}\")");
						sb.AppendLine("},");
					}
				}
			}

			sb.AppendLine("};");
			sb.AppendLine($"public static System.Guid MicroService => new System.Guid(\"{MicroService}\");");
			sb.AppendLine($"public static System.Guid SourceCode => new System.Guid(\"{SourceCode.Id}\");");
			sb.AppendLine($"public static System.Guid Component => new System.Guid(\"{SourceCode.Configuration().Component}\");");
			sb.AppendLine($"public static System.Collections.Generic.List<TomPIT.Compilation.SourceTypeDescriptor> SourceTypes => _sourceTypes;");
			sb.AppendLine("}");

			return sb.ToString();
		}
	}
}