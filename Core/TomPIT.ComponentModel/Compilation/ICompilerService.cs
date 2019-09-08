using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Compilation;
using TomPIT.ComponentModel.Resources;
using TomPIT.Connectivity;
using TomPIT.Services;

namespace TomPIT.Compilation
{
	public interface ICompilerService
	{
		void Invalidate(IExecutionContext context, Guid microService, Guid component, ISourceCode sourceCode);
		object Execute<T>(Guid microService, ISourceCode sourceCode, object sender, T e);
		object Execute<T>(Guid microService, ISourceCode sourceCode, object sender, T e, out bool handled);

		IScriptDescriptor GetScript<T>(Guid microService, ISourceCode sourceCode);
		IScriptDescriptor GetScript(Guid microService, ISourceCode sourceCode);

		string CompileView(ISysConnection connection, ISourceCode sourceCode);

		Type ResolveType(Guid microService, ISourceCode sourceCode, string typeName);
		Type ResolveType(Guid microService, ISourceCode sourceCode, string typeName, bool throwException);

		IScriptContext CreateScriptContext(ISourceCode sourceCode);
		List<string> QuerySubClasses(IScript script);

		T CreateInstance<T>(IDataModelContext context, Type scriptType) where T : class;
		T CreateInstance<T>(IDataModelContext context, Type scriptType, string arguments) where T : class;

		T CreateInstance<T>(ISourceCode sourceCode) where T : class;
		T CreateInstance<T>(ISourceCode sourceCode, string arguments, string typeName) where T : class;
		T CreateInstance<T>(ISourceCode sourceCode, string arguments) where T : class;

		T CreateInstance<T>(IDataModelContext context, ISourceCode sourceCode) where T : class;
		T CreateInstance<T>(IDataModelContext context, ISourceCode sourceCode, string arguments, string typeName) where T : class;
		T CreateInstance<T>(IDataModelContext context, ISourceCode sourceCode, string arguments) where T : class;
	}
}