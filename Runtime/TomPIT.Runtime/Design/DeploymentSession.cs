﻿using System;
using System.Collections.Immutable;
using System.Linq;

using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.ComponentModel.Deployment;
using TomPIT.Connectivity;
using TomPIT.Data;
using TomPIT.Deployment;
using TomPIT.Diagnostics;
using TomPIT.Environment;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Runtime;

namespace TomPIT.Design
{
	internal class DeploymentSession : TenantObject
	{
		public DeploymentSession(ITenant tenant, IPullRequest request) : base(tenant)
		{
			Request = request;
		}

		private IPullRequest Request { get; }

		public void Deploy(DeployArgs e)
		{
			if (e.ResetMicroService)
			{
				foreach (var component in Request.Components)
				{
					if (component?.Files is null)
						continue;

					if (!component.Files.Any())
						continue;

					component.Verb = ComponentVerb.Add;

					foreach (var file in component.Files)
						file.Verb = ComponentVerb.Add;
				}

				DropMicroService();
			}

			SynchronizeMicroService();
			Drop();
			DeployFolders();
			DeployComponents();
			SynchronizeEntities();
			RunInstallers();
			CommitMicroService();
		}

		private void DropMicroService()
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(Request.Token);

			if (ms is null)
				return;

			if (ms.Status != MicroServiceStatus.Development)
				UpdateMicroService(ms);

			Tenant.GetService<IDesignService>().MicroServices.Delete(ms.Token);
		}

		private void CommitMicroService()
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(Request.Token);

			Tenant.GetService<IDesignService>().MicroServices.Update(Request.Token, Request.Name, ms.ResourceGroup, Request.Template, ResolveStatus(), UpdateStatus.UpToDate, CommitStatus.Synchronized);
		}

		private void SynchronizeMicroService()
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(Request.Token);

			if (ms == null)
				InsertMicroService();
			else
				UpdateMicroService(ms);
		}

		private void UpdateMicroService(IMicroService microService)
		{
			Tenant.GetService<IDesignService>().MicroServices.Update(Request.Token, Request.Name, microService.ResourceGroup, Request.Template, MicroServiceStatus.Development, microService.UpdateStatus, microService.CommitStatus);
		}

		private void InsertMicroService()
		{
			var resourceGroup = Tenant.GetService<IResourceGroupService>().Default.Token;

			Tenant.GetService<IDesignService>().MicroServices.Insert(Request.Token, Request.Name, resourceGroup, Request.Template, MicroServiceStatus.Development);
		}

		private MicroServiceStatus ResolveStatus()
		{
			var stage = Tenant.GetService<IRuntimeService>().Stage;

			switch (stage)
			{
				case EnvironmentStage.Development:
					return MicroServiceStatus.Development;
				case EnvironmentStage.Staging:
					return MicroServiceStatus.Staging;
				case EnvironmentStage.Production:
					return MicroServiceStatus.Production;
				default:
					throw new NotSupportedException();
			}
		}

		private void SynchronizeEntities()
		{
			var components = Tenant.GetService<IComponentService>().QueryComponents(Request.Token, ComponentCategories.Model);

			foreach (var component in components)
			{
				if (Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) is not IModelConfiguration config)
					continue;

				try
				{
					Tenant.GetService<IModelService>().SynchronizeEntity(config);
				}
				catch (TomPITException ex)
				{
					ex.LogError(LogCategories.Deployment);
				}
			}
		}

		private void RunInstallers()
		{
			var components = Tenant.GetService<IComponentService>().QueryComponents(Request.Token, ComponentCategories.Installer);

			foreach (var component in components)
			{
				try
				{
					if (Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) is not IInstallerConfiguration config)
						continue;

					using var ctx = new MicroServiceContext(Request.Token);
					var type = config.Middleware(ctx);

					if (type == null)
						continue;

					var middleware = ctx.CreateMiddleware<IInstallerMiddleware>(type);

					if (middleware == null)
						continue;

					middleware.Invoke();
				}
				catch (TomPITException ex)
				{
					ex.LogError(LogCategories.Deployment);
				}
			}
		}

		private void Drop()
		{
			if (Request.Components == null)
				return;

			foreach (var component in Request.Components)
			{
				if (component.Verb == ComponentVerb.Delete)
					ComponentModel.Delete(component.Token, true);
			}
		}

		private void DeployComponents()
		{
			if (Request.Components == null)
				return;

			foreach (var component in Request.Components)
			{
				/*
				 * Empty components are corrupted and we don't want to restore them.
				 */
				if (component.Files is null || !component.Files.Any())
					continue;

				ComponentModel.Restore(Request.Token, component);
			}
		}

		private void DeployFolders()
		{
			var folders = Tenant.GetService<IComponentService>().QueryFolders(Request.Token);
			/*
		 * remove
		 */
			foreach (var folder in folders)
			{
				if (Request.Folders?.FirstOrDefault(f => f.Id == folder.Token) != null)
					continue;

				ComponentModel.DeleteFolder(Request.Token, folder.Token, false);
			}
			/*
		 * add and modify
		 */
			DeployFolders(Guid.Empty, folders);
		}

		private void DeployFolders(Guid parent, ImmutableList<IFolder> existing)
		{
			if (Request.Folders == null)
				return;

			var folders = Request.Folders.Where(f => f.Parent == parent);

			foreach (var folder in folders)
			{
				var existingFolder = existing.FirstOrDefault(f => f.Token == folder.Id);
				/*
				* update
				*/
				if (existingFolder != null)
				{
					ComponentModel.UpdateFolder(Request.Token, existingFolder.Token, folder.Name, folder.Parent);
				}
				else
				{
					/*
					 * insert
					 * There is one tricky scenario. Remote folder with the same name (but different token) could be created meaning
					 * we wouldn't be able to create local folder. In this case we will create folder with indexed name instead thus
					 * enabling deployment process to succeed.
					 */
					ComponentModel.RestoreFolder(Request.Token, folder.Id, EnumerateFolder(folder, existing, 0), folder.Parent);

					existing.Add(Tenant.GetService<IComponentService>().SelectFolder(folder.Id));
				}
				/*
				* deploy children
				*/
				DeployFolders(folder.Id, existing);
			}
		}

		private string EnumerateFolder(IPullRequestFolder folder, ImmutableList<IFolder> existing, int index)
		{
			var name = index == 0 ? folder.Name : $"{folder}{index}";

			var target = existing.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase) && f.Parent == folder.Parent);

			if (target is null)
				return name;

			return EnumerateFolder(folder, existing, index + 1);
		}

		private IComponentModel ComponentModel => Tenant.GetService<IDesignService>().Components;
	}
}
