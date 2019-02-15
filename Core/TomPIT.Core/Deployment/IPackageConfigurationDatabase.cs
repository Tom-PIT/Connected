using System;

namespace TomPIT.Deployment
{
	public interface IPackageConfigurationDatabase
	{
		string ConnectionString { get; }
		Guid Connection { get; }
	}
}
