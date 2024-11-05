using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Design;

namespace TomPIT.Management.ComponentModel
{
	internal class MicroServiceManagementService : TenantObject, IMicroServiceManagementService
	{
		public MicroServiceManagementService(ITenant tenant) : base(tenant)
		{
		}

		public void Delete(Guid microService)
		{
			var components = Tenant.GetService<IDesignService>().Components.Query(microService);

			foreach (var i in components)
				Tenant.GetService<IDesignService>().Components.Delete(i.Token);

			var folders = FolderModel.Create(Tenant.GetService<IComponentService>().QueryFolders(microService));

			foreach (var i in folders)
				DeleteFolder(i);

			Instance.SysProxy.Management.MicroServices.Delete(microService);
			Instance.SysProxy.Storage.Clean(microService);

			if (Tenant.GetService<IMicroServiceService>() is IMicroServiceNotification svc)
				svc.NotifyRemoved(this, new MicroServiceEventArgs(microService));
		}

		private void DeleteFolder(FolderModel model)
		{
			foreach (var i in model.Items)
				DeleteFolder(i);

			Tenant.GetService<IDesignService>().Components.DeleteFolder(model.Folder.MicroService, model.Folder.Token, true);
		}

		public void Insert(Guid token, string name, Guid resourceGroup, Guid template, string version, string commit)
		{
			Instance.SysProxy.Management.MicroServices.Insert(token, name, resourceGroup, template, version, commit);

			if (Tenant.GetService<IMicroServiceService>() is IMicroServiceNotification svc)
				svc.NotifyChanged(this, new MicroServiceEventArgs(token));
		}

		public void Update(Guid microService, string name, Guid template, Guid resourceGroup, string version, string commit)
		{
			Instance.SysProxy.Management.MicroServices.Update(microService, name, resourceGroup, template, version, commit);

			if (Tenant.GetService<IMicroServiceService>() is IMicroServiceNotification svc)
				svc.NotifyChanged(this, new MicroServiceEventArgs(microService));
		}

		public List<IMicroService> Query(Guid resourceGroup)
		{
			return Instance.SysProxy.Management.MicroServices.Query(resourceGroup).ToList();
		}
	}
}
