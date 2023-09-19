using System;
using System.Collections.Immutable;
using System.Reflection;

namespace TomPIT.Compilation;
internal class NuGetService : INuGetService
{
	public ImmutableList<Assembly> Resolve(Guid blob, bool entryOnly)
	{
		return ((CompilerService)Tenant.GetService<ICompilerService>()).Nuget.Resolve(blob, entryOnly);
	}

	public ImmutableList<Assembly> Resolve(string id, string version, bool entryOnly)
	{
		return ((CompilerService)Tenant.GetService<ICompilerService>()).Nuget.Resolve(id, version, entryOnly);
	}
}
