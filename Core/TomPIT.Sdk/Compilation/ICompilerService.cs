using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Scripting;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Compilation
{
	public interface ICompilerService
	{
		void Invalidate(IMicroServiceContext context, Guid microService, Guid component, ISourceCode sourceCode);
		object Execute<T>(Guid microService, ISourceCode sourceCode, object sender, T e);
		object Execute<T>(Guid microService, ISourceCode sourceCode, object sender, T e, out bool handled);

		IScriptDescriptor GetScript<T>(Guid microService, ISourceCode sourceCode);
		IScriptDescriptor GetScript(Guid microService, ISourceCode sourceCode);

		string CompileView(ITenant tenant, ISourceCode sourceCode);

		Type ResolveType(Guid microService, ISourceCode sourceCode, string typeName);
		Type ResolveType(Guid microService, ISourceCode sourceCode, string typeName, bool throwException);

		IScriptContext CreateScriptContext(ISourceCode sourceCode);
		List<string> QuerySubClasses(IScriptConfiguration script);

		T CreateInstance<T>(IMicroServiceContext context, Type scriptType) where T : class;
		T CreateInstance<T>(IMicroServiceContext context, Type scriptType, string arguments) where T : class;

		T CreateInstance<T>(ISourceCode sourceCode) where T : class;
		T CreateInstance<T>(ISourceCode sourceCode, string arguments, string typeName) where T : class;
		T CreateInstance<T>(ISourceCode sourceCode, string arguments) where T : class;

		T CreateInstance<T>(IMicroServiceContext context, ISourceCode sourceCode) where T : class;
		T CreateInstance<T>(IMicroServiceContext context, ISourceCode sourceCode, string arguments, string typeName) where T : class;
		T CreateInstance<T>(IMicroServiceContext context, ISourceCode sourceCode, string arguments) where T : class;
	}
}