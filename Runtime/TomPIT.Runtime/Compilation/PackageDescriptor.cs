using System.Collections.Generic;

namespace TomPIT.Compilation;
internal class PackageDescriptor
{
	public PackageDescriptor()
	{
		Files = new();
		RuntimePaths = new();
	}
	public List<PackageFileDescriptor> Files { get; }
	public List<string> RuntimePaths { get; }
}
