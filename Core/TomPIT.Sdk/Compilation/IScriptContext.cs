using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using TomPIT.ComponentModel;

namespace TomPIT.Compilation
{
	public interface IScriptContext
	{
		string SourceCode { get; }

		ConcurrentDictionary<string, IText> SourceFiles { get; }
		Dictionary<string, ImmutableArray<PortableExecutableReference>> References { get; }
	}
}
