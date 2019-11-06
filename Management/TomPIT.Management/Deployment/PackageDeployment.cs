using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.Connectivity;
using TomPIT.Data.DataProviders;
using TomPIT.Deployment;
using TomPIT.Exceptions;
using TomPIT.Ide.ComponentModel;
using TomPIT.Management.ComponentModel;
using TomPIT.Management.Deployment.Packages;
using TomPIT.Middleware;
using TomPIT.Runtime;
using TomPIT.Security;
using TomPIT.Storage;

namespace TomPIT.Management.Deployment
{
	internal class PackageDeployment : TenantObject
	{
		private IPackageConfiguration _configuration = null;

		public PackageDeployment(ITenant tenant, Guid id, IPackage package) : base(tenant)
		{
			Package = package;
			Id = id;
		}

		private Guid Id { get; }
		private IPackage Package { get; }
		private IPackageConfiguration Configuration
		{
			get
			{
				if (_configuration == null)
					_configuration = Tenant.GetService<IDeploymentService>().SelectInstallerConfiguration(Id);

				return _configuration;
			}
		}

		private Dictionary<Guid, Guid> LastKnownState { get; set; }

		public void Deploy()
		{
			var success = false;

			try
			{
				Drop();

				DeployDatabases();
				DeployMicroService();

				success = true;
			}
			finally
			{
				var u = Tenant.CreateUrl("NotificationDevelopment", "MicroServiceInstalled");
				var e = new JObject
					 {
						  {"microService", Package.MicroService.Token },
						  {"success", success }
					 };

				Tenant.Post(u, e);

				if (Tenant.GetService<IMicroServiceService>() is IMicroServiceNotification n)
					n.NotifyMicroServiceInstalled(this, new MicroServiceInstallEventArgs(Package.MicroService.Token, success));
			}
		}

		private void DeployMicroService()
		{
			var m = Package.MicroService;

			Tenant.GetService<IMicroServiceManagementService>().Insert(m.Token, m.Name, Configuration.ResourceGroup, m.Template, MicroServiceStatus.Development, Package,
				 Package.MetaData.Version);

			DeployFolders();
			DeployComponents();
			DeployBlobs();
			//DeployStrings();

			Tenant.GetService<IComponentDevelopmentService>().DropRuntimeState(Package.MicroService.Token);

			var ms = Tenant.GetService<IMicroServiceService>().Select(m.Token);

			Tenant.GetService<IMicroServiceManagementService>().Update(ms.Token, ms.Name, MicroServiceStatus.Production, ms.Template, ms.ResourceGroup, ms.Package, Package.MetaData.Plan, ms.UpdateStatus, ms.CommitStatus);
		}

		private void DeployFolders()
		{
			var model = FolderModel.Create(Package.MicroService.Token, Package.Folders);

			foreach (var i in model)
				DeployFolders(i);
		}

		private void DeployFolders(FolderModel model)
		{
			Tenant.GetService<IComponentDevelopmentService>().RestoreFolder(model.Folder.MicroService, model.Folder.Token, model.Folder.Name, model.Folder.Parent);

			foreach (var i in model.Items)
				DeployFolders(i);
		}

		private void DeployComponents()
		{
			foreach (var i in Package.Components)
			{
				var configuration = Package.Blobs.FirstOrDefault(f => f.Type == BlobTypes.Configuration && f.PrimaryKey == i.Token.ToString());
				var runtimeConfiguration = Configuration.RuntimeConfigurationSupported && Configuration.RuntimeConfiguration ? Package.Blobs.FirstOrDefault(f => f.Type == BlobTypes.RuntimeConfiguration
					  && string.Compare(f.PrimaryKey, i.RuntimeConfiguration.ToString(), true) == 0) : null;

				if (!Configuration.RuntimeConfiguration)
				{
					var state = LastKnownState == null ? Guid.Empty : LastKnownState.FirstOrDefault(f => f.Key == configuration.Token).Value;
					/*
			  * if existing runtime configuration found override component's runtime configuration
			  * set to no runtime configuration otherwise
			  */
					if (state != Guid.Empty)
						((PackageComponent)i).RuntimeConfiguration = state;
					else
						((PackageComponent)i).RuntimeConfiguration = Guid.Empty;
				}

				Tenant.GetService<IComponentDevelopmentService>().Restore(Package.MicroService.Token, i, configuration, runtimeConfiguration);
			}

			var connections = Tenant.GetService<IComponentService>().QueryComponents(Package.MicroService.Token, ComponentCategories.Connection);

			foreach (var i in connections)
			{
				var db = Configuration.Databases.FirstOrDefault(f => f.Connection == i.Token);

				if (db != null)
				{
					if (Tenant.GetService<IComponentService>().SelectConfiguration(i.Token) is IConnectionConfiguration config)
					{
						var pi = config.GetType().GetProperty("Value");
						var cs = string.IsNullOrWhiteSpace(db.ConnectionString) ? string.Empty : Tenant.GetService<ICryptographyService>().Decrypt(db.ConnectionString);

						if (pi != null && pi.CanWrite)
							pi.SetValue(config, cs);

						Tenant.GetService<IComponentDevelopmentService>().Update(config, new ComponentUpdateArgs(EnvironmentMode.Design, false));
					}
				}
			}
		}

		private void DeployBlobs()
		{
			foreach (var i in Package.Blobs)
			{
				if (i.Type == BlobTypes.RuntimeConfiguration
					 || i.Type == BlobTypes.Configuration)
					continue;

				Tenant.GetService<IStorageService>().Restore(new Blob
				{
					ContentType = i.ContentType,
					FileName = i.FileName,
					MicroService = Package.MicroService.Token,
					ResourceGroup = Configuration.ResourceGroup,
					Token = i.Token,
					PrimaryKey = i.PrimaryKey,
					Topic = i.Topic,
					Type = i.Type,
					Version = i.Version
				}, Convert.FromBase64String(i.Content));
			}
		}

		private void DeployStrings()
		{
			Tenant.GetService<IMicroServiceDevelopmentService>().RestoreStrings(Package.MicroService.Token, Package.Strings);
		}

		private void DeployDatabases()
		{
			foreach (var i in Package.Databases)
			{
				var config = Configuration.Databases.FirstOrDefault(f => string.Compare(f.Name, i.Name, true) == 0);

				if (config.Enabled)
				{
					var dp = Tenant.GetService<IDataProviderService>().Select(config.DataProviderId);

					if (dp == null)
						throw new RuntimeException(string.Format("{0} ({1})", SR.ErrDataProviderNotFound, config.DataProvider));

					if (dp.SupportsDeploy)
					{
						var cs = config.ConnectionString;

						if (string.IsNullOrWhiteSpace(cs))
							throw new RuntimeException(string.Format("{0} ({1})", SR.ErrInstallConnectionStringNotSet, config.Name));

						cs = Tenant.GetService<ICryptographyService>().Decrypt(cs);

						dp.Deploy(new DatabaseDeploymentContext(Tenant, Package, cs, i));
					}
				}
			}
		}

		private void Drop()
		{
			/*
		 * save last known runtime state in case of installer failure. installer will drop all configuration and we
		 * won't be able to recover existing runtime state if image is not created at this point.
		 * ---
		 * first, load existing state. if existing state is available it means we have an invalid installation state
		 * and we won't create another state
		 */
			LastKnownState = Tenant.GetService<IComponentDevelopmentService>().SelectRuntimeState(Package.MicroService.Token);

			var existing = Tenant.GetService<IMicroServiceService>().Select(Package.MicroService.Token);
			/*
		 * it's a fresh microservice install, no need to drop anything or save any state because it doesn't exist.
		 */
			if (existing == null)
				return;

			if (existing.Status != MicroServiceStatus.Development)
			{
				Tenant.GetService<IMicroServiceManagementService>().Update(existing.Token, existing.Name, MicroServiceStatus.Development, existing.Template,
					 existing.ResourceGroup, existing.Package, existing.Plan, existing.UpdateStatus, existing.CommitStatus);
			}
			/*
		 * now, if last known state is null we must save state. this state will be merged with the installation
		 */
			if (LastKnownState == null)
			{
				Tenant.GetService<IComponentDevelopmentService>().SaveRuntimeState(existing.Token);
				LastKnownState = Tenant.GetService<IComponentDevelopmentService>().SelectRuntimeState(Package.MicroService.Token);
			}
			/*
		 * now drop the microservice and all dependent objects. note that permissions and settings are not dropped because they 
		 * are not part of microservice configuration.
		 */
			Tenant.GetService<IMicroServiceManagementService>().Delete(existing.Token);
		}
	}
}