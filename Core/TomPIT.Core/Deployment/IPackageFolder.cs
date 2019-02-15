using System;

namespace TomPIT.Deployment
{
	public interface IPackageFolder
	{
		string Name { get; }
		Guid Token { get; }
		Guid Parent { get; }
	}
}
