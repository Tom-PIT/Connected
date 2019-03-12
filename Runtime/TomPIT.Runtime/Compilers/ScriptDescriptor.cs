using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Immutable;
using TomPIT.Compilation;

namespace TomPIT.Compilers
{
	internal class ScriptDescriptor : IScriptDescriptor
	{
		public ScriptRunner<object> Script { get; set; }
		public ImmutableArray<Diagnostic> Errors { get; set; }
		public Guid MicroService { get; set; }
		public Guid Id { get; set; }
	}
}
