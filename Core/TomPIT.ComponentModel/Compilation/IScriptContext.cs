using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;

namespace TomPIT.ComponentModel.Compilation
{
	public interface IScriptContext
	{
		string SourceCode { get; }

		Dictionary<string, ISourceCode> SourceFiles { get; }
		Dictionary<string, ImmutableArray<PortableExecutableReference>> References { get; }
	}
}
