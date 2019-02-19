using System;
using TomPIT.Deployment.Database;

namespace TomPIT.Deployment
{
	public interface IPackageDatabase : IDatabase
	{
		Guid Connection { get; }
		Guid DataProviderId { get; }
		string DataProvider { get; }
		string Name { get; }
	}
}
