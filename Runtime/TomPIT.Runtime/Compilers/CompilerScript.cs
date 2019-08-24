using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Compilation;
using TomPIT.Connectivity;

namespace TomPIT.Compilers
{
	internal class CompilerScript : IDisposable
	{
		public CompilerScript(ISysConnection connection, Guid microService, ISourceCode sourceCode)
		{
			MicroService = microService;
			SourceCode = sourceCode;
			Connection = connection;
		}

		public Guid MicroService { get; }
		public ISourceCode SourceCode { get; }
		protected ISysConnection Connection { get; }

		public List<Guid> ScriptReferences { get; private set; }
		public void Create()
		{
			var code = Connection.GetService<IComponentService>().SelectText(MicroService, SourceCode);

			if (string.IsNullOrWhiteSpace(code))
				return;

			var options = ScriptOptions.Default
				 .WithImports(Usings)
				 .WithReferences(References)
				 .WithSourceResolver(new ScriptResolver(Connection, MicroService))
				 .WithMetadataResolver(new AssemblyResolver(Connection, MicroService))
				 .WithEmitDebugInformation(true)
				 .WithFilePath(SourceCode.ScriptName(Connection))
				 .WithFileEncoding(Encoding.UTF8);

			using (var loader = new InteractiveAssemblyLoader())
			{
				var scriptContext = Connection.GetService<ICompilerService>().CreateScriptContext(SourceCode);

				var refs = new List<Guid>();

				foreach (var reference in scriptContext.SourceFiles)
					refs.Add(reference.Value.Id);

				if (refs.Count > 0)
					ScriptReferences = refs;

				foreach (var reference in scriptContext.References)
				{
					if (reference.Value == ImmutableArray<PortableExecutableReference>.Empty)
						continue;

					foreach(var executable in reference.Value)
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
								ms = Connection.GetService<IMicroServiceService>().Select(tokens[0]).Token;
								name = tokens[1];
							}

							var asm = AssemblyResolver.LoadDependency(Connection, ms, name);

							if (asm != null)
								loader.RegisterDependency(asm);
						}
					}
				}

				Script = CreateScript(code, options, loader);
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
	}
}