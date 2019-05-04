using System;
using TomPIT.ComponentModel;
using TomPIT.Services;

namespace TomPIT.Compilation
{
	public interface ICompilerService
	{
		void Invalidate(IExecutionContext context, Guid microService, Guid component, ISourceCode sourceCode);
		object Execute<T>(Guid microService, ISourceCode sourceCode, object sender, T e);
        object Execute<T>(Guid microService, ISourceCode sourceCode, object sender, T e, out bool handled);

        bool Equals(string constant, object value);

		IScriptDescriptor GetScript<T>(Guid microService, ISourceCode sourceCode);
        IScriptDescriptor GetScript(Guid microService, ISourceCode sourceCode);
    }
}
