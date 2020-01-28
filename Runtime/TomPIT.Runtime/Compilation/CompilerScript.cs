using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;

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
		public void Create()
		{
			var code = Tenant.GetService<IComponentService>().SelectText(MicroService, SourceCode);

			if (string.IsNullOrWhiteSpace(code))
				return;
			var msv = Tenant.GetService<IMicroServiceService>().Select(MicroService);

			var options = ScriptOptions.Default
				 .WithImports(Usings)
				 .WithReferences(References)
				 .WithSourceResolver(new ScriptResolver(Tenant, MicroService))
				 .WithMetadataResolver(new AssemblyResolver(Tenant, MicroService))
				 .WithEmitDebugInformation(true)
				 .WithFilePath(SourceCode.ScriptName(Tenant))
				 .WithFileEncoding(Encoding.UTF8);

			using (var loader = new InteractiveAssemblyLoader())
			{
				var scriptContext = Tenant.GetService<ICompilerService>().CreateScriptContext(SourceCode);

				var refs = new List<Guid>();

				foreach (var reference in scriptContext.SourceFiles)
					refs.Add(reference.Value.Id);

				if (refs.Count > 0)
					ScriptReferences = refs;

				foreach (var reference in scriptContext.References)
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
					}
				}

				Script = CreateScript($"{code};{System.Environment.NewLine}{GenerateStaticCode()}", options, loader);
			}
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
		protected virtual string[] Usings
		{
			get { return CompilerService.CombineUsings(null); }
		}

		public void Dispose()
		{
			Script = null;
		}

		//public IScriptDescriptor Result { get; private set; }
		public Script<object> Script { get; private set; }

		private string GenerateStaticCode()
		{
			var sb = new StringBuilder();

			sb.AppendLine($"public static class {CompilerService.ScriptInfoClassName}");
			sb.AppendLine("{");
			sb.AppendLine($"public static System.Guid MicroService => new System.Guid(\"{MicroService.ToString()}\");");
			sb.AppendLine($"public static System.Guid SourceCode => new System.Guid(\"{SourceCode.Id.ToString()}\");");
			sb.AppendLine($"public static System.Guid Component => new System.Guid(\"{SourceCode.Configuration().Component.ToString()}\");");
			sb.AppendLine("}");

			return sb.ToString();
		}
	}
}