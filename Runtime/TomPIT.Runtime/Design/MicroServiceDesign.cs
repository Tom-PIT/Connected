using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Security;

namespace TomPIT.Design
{
	internal class MicroServiceDesign : TenantObject, IMicroServiceDesign
	{
		public MicroServiceDesign(ITenant tenant) : base(tenant)
		{
		}

		public void Insert(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStatus status)
		{
			Instance.SysProxy.Management.MicroServices.Insert(token, name, resourceGroup, template, status, CreateMeta(token), null);
		}

		public void Delete(Guid token)
		{
			var components = Tenant.GetService<IDesignService>().Components.Query(token);

			foreach (var i in components)
				Tenant.GetService<IDesignService>().Components.Delete(i.Token, true);

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

		public void Update(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStatus status, UpdateStatus updateStatus, CommitStatus commitStatus)
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(token);

			Instance.SysProxy.Management.MicroServices.Update(token, name, resourceGroup, template, status, updateStatus, commitStatus, ms.Package, ms.Plan);

			if (Tenant.GetService<IMicroServiceService>() is IMicroServiceNotification notification)
				notification.NotifyChanged(this, new MicroServiceEventArgs(token));
		}

		/*
	  * Microservice meta is obsolete so this is a temporary fix.
	  */
		private string CreateMeta(Guid microService)
		{
			var meta = new JObject
					 {
							{"microService", microService },
							{"created", DateTime.Today }
					 };

			return Tenant.GetService<ICryptographyService>().Encrypt(JsonConvert.SerializeObject(meta));
		}
	}
}
