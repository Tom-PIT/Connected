using NuGet.Configuration;

namespace TomPIT.Compilation
{
	internal class LocalPackageSource : PackageSource
	{
		public LocalPackageSource() : base("localhost")
		{
		}
	}
}
