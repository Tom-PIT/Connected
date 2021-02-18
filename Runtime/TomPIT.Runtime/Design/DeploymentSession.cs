using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.ComponentModel.Deployment;
using TomPIT.Connectivity;
using TomPIT.Data;
using TomPIT.Deployment;
using TomPIT.Middleware;

namespace TomPIT.Design
{
	internal class DeploymentSession : TenantObject
	{
		public DeploymentSession(ITenant tenant, IPullRequest request) : base(tenant)
		{
			Request = request;
		}

		private IPullRequest Request { get; }

		public void Deploy()
		{
			Drop();
			DeployFolders();
			DeployComponents();

			SynchronizeEntities();
			RunInstallers();
		}

		private void SynchronizeEntities()
		{
			var components = Tenant.GetService<IComponentService>().QueryComponents(Request.Token, ComponentCategories.Model);

			foreach (var component in components)
			{
				if (Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) is not IModelConfiguration config)
					continue;

				Tenant.GetService<IModelService>().SynchronizeEntity(config);
			}
		}

		private void RunInstallers()
		{
			var components = Tenant.GetService<IComponentService>().QueryComponents(Request.Token, ComponentCategories.Installer);

			foreach (var component in components)
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
		}

		private void Drop()
		{
			if (Request.Components == null)
				return;

			foreach(var component in Request.Components)
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
				if (component.Verb == ComponentVerb.Delete || component.Verb == ComponentVerb.NotModified)
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
			foreach(var folder in folders)
			{
				if (Request.Folders?.FirstOrDefault(f => f.Id == folder.Token) != null)
					continue;
				/*
				 * if no components are in the folder we can remove it
				 */
				var components = Tenant.GetService<IComponentService>().QueryComponents(Request.Token, folder.Token);

				if (components.Count == 0)
					ComponentModel.DeleteFolder(Request.Token, folder.Token);
			}
			/*
			 * add and modify
			 */
			DeployFolders(Guid.Empty, folders);
		}

		private void DeployFolders(Guid parent, List<IFolder> existing)
		{
			if (Request.Folders == null)
				return;

			var folders = Request.Folders.Where(f => f.Parent == parent);

			foreach(var folder in folders)
			{
				var existingFolder = existing.FirstOrDefault(f => f.Token == folder.Id);
				/*
				 * update
				 */
				if(existingFolder != null)
				{
					ComponentModel.UpdateFolder(Request.Token, existingFolder.Token, folder.Name, folder.Parent);
				}
				else
				{
					/*
					 * insert
					 */
					ComponentModel.InsertFolder(Request.Token, folder.Name, folder.Parent);

					existing.Add(Tenant.GetService<IComponentService>().SelectFolder(folder.Id));
				}
				/*
				 * deploy children
				 */
				DeployFolders(folder.Id, existing);
			}
		}

		private IComponentModel ComponentModel => Tenant.GetService<IDesignService>().Components;
	}
}
