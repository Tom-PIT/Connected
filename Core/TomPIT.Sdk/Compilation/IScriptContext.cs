using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using TomPIT.ComponentModel;

namespace TomPIT.Compilation
{
	public interface IScriptContext
	{
		ConcurrentDictionary<string, IText> SourceFiles { get; }
		ConcurrentDictionary<string, ImmutableArray<PortableExecutableReference>> References { get; }
	}
}
