using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;

namespace TomPIT.Compilers
{
	public interface IScriptDescriptor
	{
		Script Script { get; }
		ImmutableArray<Diagnostic> Errors { get; }
	}
}
