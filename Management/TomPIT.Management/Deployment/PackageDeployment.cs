using System;
using System.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.ComponentModel.Deployment;
using TomPIT.ComponentModel.Resources;
using TomPIT.Connectivity;
using TomPIT.Data.DataProviders;
using TomPIT.Deployment;
using TomPIT.Design;
using TomPIT.Design.Serialization;
using TomPIT.Diagnostics;
using TomPIT.Exceptions;
using TomPIT.Ide.ComponentModel;
using TomPIT.Management.ComponentModel;
using TomPIT.Management.Deployment.Packages;
using TomPIT.Middleware;
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

		//private Dictionary<Guid, Guid> LastKnownState { get; set; }

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
				Instance.SysProxy.Development.Notifications.MicroServiceInstalled(Package.MicroService.Token, success);

				if (Tenant.GetService<IMicroServiceService>() is IMicroServiceNotification n)
					n.NotifyMicroServiceInstalled(this, new MicroServiceInstallEventArgs(Package.MicroService.Token, success));
			}

			RunInstallers();
		}

		private void RunInstallers()
		{
			var installers = Tenant.GetService<IComponentService>().QueryConfigurations(Package.MicroService.Token, ComponentCategories.Installer);

			if (installers.Count == 0)
				return;

			foreach (var installer in installers)
				RunInstaller(installer as IInstallerConfiguration);
		}

		private void RunInstaller(IInstallerConfiguration configuration)
		{
			var type = Tenant.GetService<ICompilerService>().ResolveType(Package.MicroService.Token, configuration, configuration.ComponentName(), false);

			if (type == null)
				return;

			try
			{
				using var context = new MicroServiceContext(Package.MicroService.Token, Tenant.Url);
				var instance = Tenant.GetService<ICompilerService>().CreateInstance<IInstallerMiddleware>(context, type);

				instance.Invoke();
			}
			catch (Exception ex)
			{
				Tenant.GetService<ILoggingService>().Write(new LogEntry
				{
					Category = LogCategories.Deployment,
					Component = configuration.Component,
					Level = System.Diagnostics.TraceLevel.Error,
					Message = ex.Message,
					Source = ex.Source
				});
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

			//Tenant.GetService<IDesignService>().Components.DropRuntimeState(Package.MicroService.Token);

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
			Tenant.GetService<IDesignService>().Components.RestoreFolder(model.Folder.MicroService, model.Folder.Token, model.Folder.Name, model.Folder.Parent);

			foreach (var i in model.Items)
				DeployFolders(i);
		}

		private void DeployComponents()
		{
			foreach (var i in Package.Components)
			{
				var isStringTable = string.Compare(i.Category, ComponentCategories.StringTable, true) == 0;
				var configuration = Package.Blobs.FirstOrDefault(f => f.Type == BlobTypes.Configuration && f.PrimaryKey == i.Token.ToString());

				Tenant.GetService<IDesignService>().Components.Restore(Package.MicroService.Token, i, configuration);
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

						Tenant.GetService<IDesignService>().Components.Update(config, new ComponentUpdateArgs(false));
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

					if (dp is IDeployDataProvider deploy)
					{
						var cs = config.ConnectionString;

						if (string.IsNullOrWhiteSpace(cs))
							throw new RuntimeException(string.Format("{0} ({1})", SR.ErrInstallConnectionStringNotSet, config.Name));

						cs = Tenant.GetService<ICryptographyService>().Decrypt(cs);

						deploy.Deploy(new DatabaseDeploymentContext(Tenant, Package, cs, i));
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
			//LastKnownState = Tenant.GetService<IDesignService>().Components.SelectRuntimeState(Package.MicroService.Token);

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
			//if (LastKnownState == null)
			//{
			//	Tenant.GetService<IDesignService>().Components.SaveRuntimeState(existing.Token);
			//	LastKnownState = Tenant.GetService<IDesignService>().Components.SelectRuntimeState(Package.MicroService.Token);
			//}
			/*
		 * now drop the microservice and all dependent objects. note that permissions and settings are not dropped because they 
		 * are not part of microservice configuration.
		 */
			Tenant.GetService<IMicroServiceManagementService>().Delete(existing.Token);
		}
		/// <summary>
		/// This method merges string table translations. If the administrator changes the translation on the target system the Changed flag is set
		/// to true on the translation. This method checks for all those flags and leave strings as is if they were changed, they are overriden otherwise
		/// </summary>
		/// <param name="component">Deploying component.</param>
		/// <param name="runtimeConfiguration">Deploying runtime configuration.</param>
		/// <param name="state">Existing state id.</param>
		//private void MergeTranslations(IPackageComponent component, IPackageBlob runtimeConfiguration, Guid state)
		//{
		//	var type = Type.GetType(component.Type);
		//	var config = Tenant.GetService<ISerializationService>().Deserialize(Convert.FromBase64String(runtimeConfiguration.Content), type) as IStringTableConfiguration;
		//	var existingConfigBlob = state == Guid.Empty ? null : Tenant.GetService<IStorageService>().Download(state);

		//	ResetAuditFlag(config);

		//	if (existingConfigBlob == null)
		//	{
		//		OverrideStringTableConfiguration(config, runtimeConfiguration);
		//		return;
		//	}

		//	if (!(Tenant.GetService<ISerializationService>().Deserialize(existingConfigBlob.Content, type) is IStringTableConfiguration existingConfig))
		//	{
		//		OverrideStringTableConfiguration(config, runtimeConfiguration);
		//		return;
		//	}

		//	foreach (var s in config.Strings)
		//	{
		//		var existingString = existingConfig.Strings.FirstOrDefault(f => string.Compare(f.Key, s.Key, true) == 0);

		//		/*
		//		 * override changed translations
		//		 */
		//		foreach (var translation in s.Translations)
		//		{
		//			if (existingString == null)
		//				continue;

		//			var existingTranslation = existingString.Translations.FirstOrDefault(f => f.Lcid == translation.Lcid);

		//			if (existingTranslation != null && existingTranslation.Changed)
		//				s.UpdateTranslation(translation.Lcid, existingTranslation.Value, true);
		//		}
		//		/*
		//		 * add existing translations
		//		 */
		//		foreach (var existingTranslation in existingString.Translations)
		//		{
		//			var translation = s.Translations.FirstOrDefault(f => f.Lcid == existingTranslation.Lcid);

		//			if (translation == null)
		//				s.UpdateTranslation(translation.Lcid, existingTranslation.Value, true);
		//		}
		//	}

		//	OverrideStringTableConfiguration(config, runtimeConfiguration);
		//}

		//private void ResetAuditFlag(IStringTableConfiguration stringTable)
		//{
		//	foreach (var s in stringTable.Strings)
		//	{
		//		foreach (var translation in s.Translations)
		//		{
		//			if (translation.Changed)
		//				s.UpdateTranslation(translation.Lcid, translation.Value, false);
		//		}
		//	}
		//}

		private void OverrideStringTableConfiguration(IStringTableConfiguration config, IPackageBlob blob)
		{
			if (blob is PackageBlob pb)
				pb.Content = Convert.ToBase64String(Tenant.GetService<ISerializationService>().Serialize(config));
		}
	}
}