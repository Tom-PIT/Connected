﻿using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Scripting;
using TomPIT.Middleware;

namespace TomPIT.Compilation
{
	public enum CompilerLanguage
	{
		CSharp = 1,
		Razor = 2
	}

	public interface ICompilerService
	{
		event EventHandler<Guid> Invalidated;
		void Invalidate(IMicroServiceContext context, Guid microService, Guid component, IText sourceCode);
		IScriptDescriptor GetScript(CompilerScriptArgs e);

		IMicroService ResolveMicroService(Type type);
		IMicroService ResolveMicroService(object instance);
		IComponent ResolveComponent(object instance);
		IComponent ResolveComponent(Type type);

		Type ResolveType(Guid microService, IText sourceCode, string typeName);
		Type ResolveType(Guid microService, IText sourceCode, string typeName, bool throwException);

		IScriptContext CreateScriptContext(IText sourceCode);
		List<string> QuerySubClasses(IScriptConfiguration script);

		T CreateInstance<T>(IMicroServiceContext context, Type scriptType) where T : class;
		T CreateInstance<T>(IMicroServiceContext context, Type scriptType, string arguments) where T : class;
		T CreateInstance<T>(IMicroServiceContext context, Type scriptType, object arguments) where T : class;

		T CreateInstance<T>(IText sourceCode) where T : class;
		T CreateInstance<T>(IText sourceCode, string arguments, string typeName) where T : class;
		T CreateInstance<T>(IText sourceCode, string arguments) where T : class;

		T CreateInstance<T>(IText sourceCode, object arguments, string typeName) where T : class;
		T CreateInstance<T>(IText sourceCode, object arguments) where T : class;

		T CreateInstance<T>(IMicroServiceContext context, IText sourceCode) where T : class;
		T CreateInstance<T>(IMicroServiceContext context, IText sourceCode, string arguments, string typeName) where T : class;
		T CreateInstance<T>(IMicroServiceContext context, IText sourceCode, string arguments) where T : class;

		T CreateInstance<T>(IMicroServiceContext context, IText sourceCode, object arguments, string typeName) where T : class;
		T CreateInstance<T>(IMicroServiceContext context, IText sourceCode, object arguments) where T : class;

		Microsoft.CodeAnalysis.Compilation GetCompilation(IText sourceCode);

		public IText ResolveText(Guid microService, string path);
		public string Rewrite(string sourceText);
		public string ResolveReference(Guid microService, string path);
	}
}