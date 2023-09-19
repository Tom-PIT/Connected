using System;
using System.Collections.Immutable;
using System.Reflection;

namespace TomPIT.Compilation;
public interface INuGetService
{
	ImmutableList<Assembly> Resolve(Guid blob, bool entryOnly);
	ImmutableList<Assembly> Resolve(string id, string version, bool entryOnly);
}
