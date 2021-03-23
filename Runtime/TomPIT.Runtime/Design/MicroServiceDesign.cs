using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Middleware;
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
			Tenant.Post(CreateUrl("Insert"), new
			{
				microService = token,
				name,
				resourceGroup,
				template,
				status,
				meta = CreateMeta(token)
			});
		}

		public void Delete(Guid token)
		{
			var components = Tenant.GetService<IDesignService>().Components.Query(token);

			foreach (var i in components)
				Tenant.GetService<IDesignService>().Components.Delete(i.Token, true);

			var folders = FolderModel.Create(Tenant.GetService<IComponentService>().QueryFolders(token));

			foreach (var i in folders)
				DeleteFolder(i);

			Tenant.Post(CreateUrl("Delete"), new
			{
				microService = token,
			});

			Tenant.Post(Tenant.CreateUrl("Storage", "Clean"), new
			{
				microService = token
			});


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
			Tenant.Post(CreateUrl("Update"), new
			{
				microService = token,
				name,
				resourceGroup,
				template,
				status,
				updateStatus,
				commitStatus
			});

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

		private ServerUrl CreateUrl(string action)
		{
			return Tenant.CreateUrl("MicroServiceManagement", action);
		}
	}
}
