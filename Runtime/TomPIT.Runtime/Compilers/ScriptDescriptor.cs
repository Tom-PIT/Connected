using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections.Immutable;
using TomPIT.Compilation;

namespace TomPIT.Compilers
{
	internal class ScriptDescriptor : IScriptDescriptor
	{
		public Script Script { get; set; }
		public ImmutableArray<Diagnostic> Errors { get; set; }
	}
}
