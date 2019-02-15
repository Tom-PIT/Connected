using System;

namespace TomPIT.Deployment
{
	public interface IPackageDependency
	{
		string Name { get; }
		Guid Token { get; }
	}
}
