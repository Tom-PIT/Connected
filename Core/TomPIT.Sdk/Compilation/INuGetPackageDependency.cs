using System.Collections.Generic;

namespace TomPIT.Compilation
{
	public interface INuGetPackageDependency
	{
		string Id { get; }
		string Version { get; }

		List<INuGetPackageDependency> Dependencies { get; }
	}
}
