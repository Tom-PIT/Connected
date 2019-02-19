using System;

namespace TomPIT.Deployment
{
	public interface IPackageConfigurationDatabase
	{
		string ConnectionString { get; }
		Guid Connection { get; }
		string Name { get; }
		string DataProvider { get; }
		Guid DataProviderId { get; }
		bool Enabled { get; }
	}
}
