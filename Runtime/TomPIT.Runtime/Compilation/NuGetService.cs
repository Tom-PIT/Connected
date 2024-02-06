using System;
using System.Collections.Immutable;
using System.Reflection;

namespace TomPIT.Compilation;
internal class NuGetService : INuGetService
{
	private NuGetPackages _nuGet;
	public NuGetPackages Nuget => _nuGet ??= new NuGetPackages();
	public ImmutableList<Assembly> Resolve(Guid blob, bool entryOnly)
	{
		return Nuget.Resolve(blob, entryOnly);
	}

	public ImmutableList<Assembly> Resolve(string id, string version, bool entryOnly)
	{
		return Nuget.Resolve(id, version, entryOnly);
	}

	public ImmutableList<string> ResolveRuntimePaths(string id, string version)
	{
		return Nuget.ResolveRuntimePaths(id, version).ToImmutableList();
	}

}
