using System;

namespace TomPIT.Deployment
{
	public interface IPackageDependency
	{
		string Title { get; }
		Guid MicroService { get; }
		Guid Plan { get; }
	}
}
