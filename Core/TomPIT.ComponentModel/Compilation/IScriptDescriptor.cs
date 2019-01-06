using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;

namespace TomPIT.Compilation
{
	public interface IScriptDescriptor
	{
		Script Script { get; }
		ImmutableArray<Diagnostic> Errors { get; }
	}
}
