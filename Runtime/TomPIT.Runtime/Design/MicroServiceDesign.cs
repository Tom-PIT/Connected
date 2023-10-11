using System;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;

namespace TomPIT.Design
{
	internal class MicroServiceDesign : TenantObject, IMicroServiceDesign
	{
		public MicroServiceDesign(ITenant tenant) : base(tenant)
		{
		}

		public void Insert(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStages stages)
		{
			Instance.SysProxy.Management.MicroServices.Insert(token, name, resourceGroup, template, stages, null);
		}

		public void Delete(Guid token)
		{
			var components = Tenant.GetService<IDesignService>().Components.Query(token);

			foreach (var i in components)
				Tenant.GetService<IDesignService>().Components.Delete(i.Token);

			var folders = FolderModel.Create(Tenant.GetService<IComponentService>().QueryFolders(token));

			foreach (var i in folders)
				DeleteFolder(i);

			Instance.SysProxy.Management.MicroServices.Delete(token);
			Instance.SysProxy.Storage.Clean(token);

			if (Tenant.GetService<IMicroServiceService>() is IMicroServiceNotification notification)
				notification.NotifyRemoved(this, new MicroServiceEventArgs(token));
		}

		private void DeleteFolder(FolderModel model)
		{
			foreach (var i in model.Items)
				DeleteFolder(i);

			Tenant.GetService<IDesignService>().Components.DeleteFolder(model.Folder.MicroService, model.Folder.Token, true);
		}

		public void Update(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStages stages)
		{
			Instance.SysProxy.Management.MicroServices.Update(token, name, resourceGroup, template, stages);

			if (Tenant.GetService<IMicroServiceService>() is IMicroServiceNotification notification)
				notification.NotifyChanged(this, new MicroServiceEventArgs(token));
		}
	}
}
