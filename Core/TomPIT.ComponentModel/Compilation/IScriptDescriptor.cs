using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Immutable;

namespace TomPIT.Compilation
{
	public interface IScriptDescriptor
	{
		ScriptRunner<object> Script { get; }
		ImmutableArray<Diagnostic> Errors { get; }
		Guid MicroService { get; }
		Guid Id { get; }
	}
}
