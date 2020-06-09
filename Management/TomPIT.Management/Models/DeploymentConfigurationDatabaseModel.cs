using System;
using TomPIT.Data.DataProviders;
using TomPIT.Deployment;
using TomPIT.Management.Designers;
using TomPIT.Security;

namespace TomPIT.Management.Models
{
	public class DeploymentConfigurationDatabaseModel : DeploymentConfigurationModel
	{
		public DeploymentConfigurationDatabaseModel(Guid package, DeploymentDesigner designer, IPackageConfigurationDatabase database) : base(package, designer)
		{
			Database = database;
		}

		public IPackageConfigurationDatabase Database { get; }

		public string ConnectionString
		{
			get
			{
				if (string.IsNullOrWhiteSpace(Database.ConnectionString))
					return null;

				try
				{
					return Designer.Environment.Context.Tenant.GetService<ICryptographyService>().Decrypt(Database.ConnectionString);
				}
				catch { return null; }
			}
		}

		public IDataProvider DataProvider
		{
			get
			{
				if (Database.DataProviderId == Guid.Empty)
					return null;

				return Designer.Environment.Context.Tenant.GetService<IDataProviderService>().Select(Database.DataProviderId);
			}
		}

		public bool SupportsDeploy
		{
			get
			{
				return DataProvider == null
					? false
					: DataProvider is IDeployDataProvider;
			}
		}
	}
}
