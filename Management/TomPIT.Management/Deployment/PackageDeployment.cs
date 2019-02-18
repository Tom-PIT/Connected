using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Deployment;
using TomPIT.Design;
using TomPIT.Storage;

namespace TomPIT.Management.Deployment
{
	internal class PackageDeployment
	{
		private IPackageConfiguration _configuration = null;

		public PackageDeployment(ISysConnection connection, IPackage package)
		{
			Package = package;
			Connection = connection;
		}

		private IPackage Package { get; }
		private ISysConnection Connection { get; }
		private IPackageConfiguration Configuration
		{
			get
			{
				if (_configuration == null)
					_configuration = Connection.GetService<IDeploymentService>().SelectInstallerConfiguration(Package.MicroService.Token);

				return _configuration;
			}
		}

		private Dictionary<Guid, Guid> LastKnownState { get; set; }

		public bool Deploy()
		{
			var success = true;

			try
			{
				Drop();

				DeployDatabase();
				DeployMicroService();
			}
			catch (Exception ex)
			{
				success = false;
				Connection.LogError(null, "Installer", ex.Source, ex.Message);
			}

			var u = Connection.CreateUrl("NotificationDevelopment", "MicroServiceInstalled");
			var e = new JObject
			{
				{"microService", Package.MicroService.Token },
				{"success", success }
			};

			Connection.Post(u, e);

			if (Connection.GetService<IMicroServiceService>() is IMicroServiceNotification n)
				n.NotifyMicroServiceInstalled(this, new MicroServiceInstallEventArgs(Package.MicroService.Token, success));

			return success;
		}

		private void DeployMicroService()
		{
			var m = Package.MicroService;

			Connection.GetService<IMicroServiceManagementService>().Insert(m.Token, m.Name, Configuration.ResourceGroup, m.Template, MicroServiceStatus.Production);
			//TODO: save metadata
			DeployFolders();
			DeployComponents();
			DeployBlobs();
			//DeployStrings();

			Connection.GetService<IComponentDevelopmentService>().DropRuntimeState(Package.MicroService.Token);
		}

		private void DeployFolders()
		{
			var model = FolderModel.Create(Package.MicroService.Token, Package.Folders);

			foreach (var i in model)
				DeployFolders(i);
		}

		private void DeployFolders(FolderModel model)
		{
			Connection.GetService<IComponentDevelopmentService>().RestoreFolder(model.Folder.MicroService, model.Folder.Token, model.Folder.Name, model.Folder.Parent);

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

					if (state != Guid.Empty)
						((PackageComponent)i).RuntimeConfiguration = state;
				}

				Connection.GetService<IComponentDevelopmentService>().Restore(Package.MicroService.Token, i, configuration, runtimeConfiguration);
			}
		}

		private void DeployBlobs()
		{
			foreach (var i in Package.Blobs)
			{
				if (i.Type == BlobTypes.RuntimeConfiguration
					|| i.Type == BlobTypes.Configuration)
					continue;

				Connection.GetService<IStorageService>().Restore(new Blob
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
			Connection.GetService<IMicroServiceDevelopmentService>().RestoreStrings(Package.MicroService.Token, Package.Strings);
		}

		private void DeployDatabase()
		{

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
			LastKnownState = Connection.GetService<IComponentDevelopmentService>().SelectRuntimeState(Package.MicroService.Token);

			var existing = Connection.GetService<IMicroServiceService>().Select(Package.MicroService.Token);
			/*
			 * it's a fresh microservice install, no need to drop anything or save any state because it doesn't exist.
			 */
			if (existing == null)
				return;
			/*
			 * now, if last known state is null we must save state. this state will be merged with the installation
			 */
			if (LastKnownState == null)
			{
				Connection.GetService<IComponentDevelopmentService>().SaveRuntimeState(existing.Token);
				LastKnownState = Connection.GetService<IComponentDevelopmentService>().SelectRuntimeState(Package.MicroService.Token);
			}
			/*
			 * now drop the microservice and all dependent objects. note that permissions and settings are not dropped because they 
			 * are not part of microservice configuration.
			 */
			Connection.GetService<IMicroServiceManagementService>().Delete(existing.Token);
		}
	}
}
