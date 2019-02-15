using System;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Deployment;

namespace TomPIT.Management.Deployment
{
	internal class PackageDeployment
	{
		public PackageDeployment(ISysConnection connection, IPackage package)
		{
			Package = package;
			Connection = connection;
		}

		private IPackage Package { get; }
		private ISysConnection Connection { get; }

		public void Deploy()
		{
			Drop();

			DeployDatabase();
			DeployMicroService();
		}

		private void DeployMicroService()
		{
			var m = Package.MicroService;

			Connection.GetService<IMicroServiceManagementService>().Insert(m.Name, Guid.Empty, m.Template, MicroServiceStatus.Production);
			//Connection.GetService<IMicroServiceManagementService>().up

			//foreach(var i in Package.Folders)
		}

		private void DeployDatabase()
		{

		}

		private void Drop()
		{
			Connection.GetService<IMicroServiceManagementService>().Delete(Package.MicroService.Token);
		}
	}
}
