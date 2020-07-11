using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Deployment;
using TomPIT.Design;
using TomPIT.Middleware;
using TomPIT.Security;

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
				Tenant.GetService<IDesignService>().Components.Delete(i.Token, true);

			var folders = FolderModel.Create(Tenant.GetService<IComponentService>().QueryFolders(microService));

			foreach (var i in folders)
				DeleteFolder(i);

			var u = Tenant.CreateUrl("MicroServiceManagement", "Delete");
			var args = new JObject {
					 {"microService", microService }
				};

			Tenant.Post(u, args);

			u = Tenant.CreateUrl("Storage", "Clean");
			args = new JObject {
					 {"microService", microService }
				};

			Tenant.Post(u, args);

			if (Tenant.GetService<IMicroServiceService>() is IMicroServiceNotification svc)
				svc.NotifyRemoved(this, new MicroServiceEventArgs(microService));
		}

		private void DeleteFolder(FolderModel model)
		{
			foreach (var i in model.Items)
				DeleteFolder(i);

			Tenant.GetService<IDesignService>().Components.DeleteFolder(model.Folder.MicroService, model.Folder.Token);
		}

		public void Insert(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStatus status, IPackage package, string version)
		{
			var u = Tenant.CreateUrl("MicroServiceManagement", "Insert");
			var args = new JObject
				{
					 { "name",name },
					 { "microService",token },
					 {"status", status.ToString() },
					 {"resourceGroup", resourceGroup },
					 {"template", template },
					 {"meta", CreateMeta(token, package) },
					 {"version", version }
				};

			Tenant.Post(u, args);

			if (Tenant.GetService<IMicroServiceService>() is IMicroServiceNotification svc)
				svc.NotifyChanged(this, new MicroServiceEventArgs(token));
		}

		private string CreateMeta(Guid microService, IPackage package)
		{
			var meta = new JObject
				{
					 {"microService", microService },
					 {"created", DateTime.Today }
				};

			if (package != null)
			{
				meta.Add("author", package.MetaData.Account);
				meta.Add("plan", package.MetaData.Plan);
				meta.Add("service", package.MetaData.Service);
			};

			return Tenant.GetService<ICryptographyService>().Encrypt(JsonConvert.SerializeObject(meta));
		}

		private string CreateMicroServiceMeta(Guid microService)
		{
			var u = Tenant.CreateUrl("MicroServiceManagement", "CreateMicroServiceMeta")
				 .AddParameter("microService", microService);

			return Tenant.Get<string>(u);
		}

		public void Update(Guid microService, string name, MicroServiceStatus status, Guid template, Guid resourceGroup, Guid package, Guid plan, UpdateStatus updateStatus, CommitStatus commitStatus)
		{
			var u = Tenant.CreateUrl("MicroServiceManagement", "Update");
			var args = new JObject
				{
					 { "name",name },
					 { "microService",microService },
					 {"status", status.ToString() },
					 {"template", template },
					 {"resourceGroup", resourceGroup },
					 {"package", package },
					 {"plan", plan },
					 {"updateStatus", updateStatus.ToString() },
					 {"commitStatus", commitStatus.ToString() }
				};

			Tenant.Post(u, args);

			if (Tenant.GetService<IMicroServiceService>() is IMicroServiceNotification svc)
				svc.NotifyChanged(this, new MicroServiceEventArgs(microService));
		}

		public List<IMicroService> Query(Guid resourceGroup)
		{
			var u = Tenant.CreateUrl("MicroServiceManagement", "Query")
				 .AddParameter("resourceGroup", resourceGroup);

			return Tenant.Get<List<MicroService>>(u).ToList<IMicroService>();
		}

		public List<IMicroServiceString> QueryStrings(Guid microService)
		{
			var u = Tenant.CreateUrl("MicroServiceManagement", "QueryStrings")
				 .AddParameter("microService", microService);

			return Tenant.Get<List<MicroServiceString>>(u).ToList<IMicroServiceString>();
		}
	}
}
