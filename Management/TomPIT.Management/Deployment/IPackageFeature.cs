using System;

namespace TomPIT.Deployment
{
	public interface IPackageFeature
	{
		string Name { get; }
		Guid Token { get; }
	}
}
