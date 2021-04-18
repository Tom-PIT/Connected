using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;

namespace TomPIT.Compilation
{
	internal class CompilerGenericScript<T> : CompilerScript
	{
		public CompilerGenericScript(ITenant tenant, Guid microService, IText sourceCode) : base(tenant, microService, sourceCode)
		{
		}

		protected override string[] Usings => new string[] { typeof(T).Namespace };
		protected override List<Assembly> References => new List<Assembly>
				{
					 typeof(T).Assembly,
					 CompilerService.LoadSystemAssembly("TomPIT.Core"),
					 CompilerService.LoadSystemAssembly("TomPIT.ComponentModel"),
					 CompilerService.LoadSystemAssembly("TomPIT.Sdk"),
					 CompilerService.LoadSystemAssembly("TomPIT.Runtime"),
					 CompilerService.LoadSystemAssembly("Newtonsoft.Json")
				};

		protected override Script<object> CreateScript(string sourceCode, ScriptOptions options, InteractiveAssemblyLoader loader)
		{
			return CSharpScript.Create(sourceCode, options: options, globalsType: typeof(ScriptGlobals<T>), assemblyLoader: loader);
		}
	}
}
