using System;
using TomPIT.ComponentModel;
using TomPIT.Runtime;

namespace TomPIT.Compilers
{
	public interface ICompilerService
	{
		void Invalidate(IApplicationContext context, Guid microService, Guid component, ISourceCode sourceCode);
		object Execute<T>(Guid microService, ISourceCode sourceCode, object sender, T e);

		bool Equals(string constant, object value);

		IScriptDescriptor GetScript<T>(Guid microService, ISourceCode sourceCode);
	}
}
