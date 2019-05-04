using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;

namespace TomPIT.Compilers
{
    internal class CompilerGenericScript<T> : CompilerScript
    {
        public CompilerGenericScript(ISysConnection connection, Guid microService, ISourceCode sourceCode) : base(connection, microService, sourceCode)
        {
        }

        protected override string[] Usings => CompilerService.CombineUsings(new List<string> { typeof(T).Namespace });
        protected override List<Assembly> References => new List<Assembly>
            {
                typeof(T).Assembly,
                CompilerService.LoadSystemAssembly("TomPIT.Core"),
                CompilerService.LoadSystemAssembly("TomPIT.ComponentModel"),
                CompilerService.LoadSystemAssembly("Newtonsoft.Json")
            };

        protected override Script<object> CreateScript(string sourceCode, ScriptOptions options, InteractiveAssemblyLoader loader)
        {
            return CSharpScript.Create(sourceCode, options: options, globalsType: typeof(ScriptGlobals<T>), assemblyLoader: loader);
        }
    }
}
