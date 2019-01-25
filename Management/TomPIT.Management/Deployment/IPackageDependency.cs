using System;

namespace TomPIT.Deployment
{
	public interface IPackageDependency
	{
		Guid Token { get; }
		string Name { get; }
	}
}
