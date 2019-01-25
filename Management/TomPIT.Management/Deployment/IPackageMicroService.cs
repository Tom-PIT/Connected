using System;

namespace TomPIT.Deployment
{
	public interface IPackageMicroService
	{
		string Name { get; }
		Guid Token { get; }
		Guid Template { get; }
		string MetaData { get; }
	}
}
