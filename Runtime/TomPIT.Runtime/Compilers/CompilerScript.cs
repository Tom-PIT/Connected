using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;

namespace TomPIT.Compilers
{
    internal class CompilerScript:IDisposable
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

        public void Create()
        {
            var code = Connection.GetService<IComponentService>().SelectText(MicroService, SourceCode);

            if (string.IsNullOrWhiteSpace(code))
                return;

            CompilerService.ResolveReferences(Connection, MicroService, SourceCode, code);

            Result = new ScriptDescriptor
            {
                MicroService = MicroService,
                Id = SourceCode.Id
            };

            var options = ScriptOptions.Default
                .WithImports(Usings)
                .WithReferences(References)
                .WithSourceResolver(new ReferenceResolver(Connection, MicroService))
                .WithMetadataResolver(new MetaDataResolver(Connection, MicroService))
                .WithEmitDebugInformation(true)
                .WithFilePath(SourceCode.ScriptName(Connection))
                .WithFileEncoding(Encoding.UTF8);

            using (var loader = new InteractiveAssemblyLoader())
            {
                var refs = CompilerService.ParseReferences(code);

                foreach (var i in refs)
                {
                    var asm = MetaDataResolver.LoadDependency(Connection, MicroService, i);

                    if (asm != null)
                        loader.RegisterDependency(asm);
                }

                Script = CreateScript(code, options, loader);
            }
        }

        protected virtual Script<object> CreateScript(string sourceCode, ScriptOptions options,  InteractiveAssemblyLoader loader)
        {
            return CSharpScript.Create(sourceCode, options: options, assemblyLoader: loader);
        }
        protected virtual List<Assembly> References=> new List<Assembly>
            {
                CompilerService.LoadSystemAssembly("TomPIT.Core"),
                CompilerService.LoadSystemAssembly("TomPIT.ComponentModel"),
                CompilerService.LoadSystemAssembly("Newtonsoft.Json")
            };
        protected virtual string[] Usings
        {
            get {return CompilerService.CombineUsings(null); }
        }

        public void Dispose()
        {
            Script = null;
        }

        public IScriptDescriptor Result { get; private set; }
        public Script<object> Script { get; private set; }
    }

}
