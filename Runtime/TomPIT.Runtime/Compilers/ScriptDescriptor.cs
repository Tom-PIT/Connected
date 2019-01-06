using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;

namespace TomPIT.Compilers
{
	internal class ScriptDescriptor : IScriptDescriptor
	{
		public Script Script { get; set; }
		public ImmutableArray<Diagnostic> Errors { get; set; }
	}
}
