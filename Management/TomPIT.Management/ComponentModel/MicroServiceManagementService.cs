using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Connectivity;
using TomPIT.Deployment;
using TomPIT.Design;
using TomPIT.Security;

namespace TomPIT.ComponentModel
{
    internal class MicroServiceManagementService : IMicroServiceManagementService
    {
        public MicroServiceManagementService(ISysConnection connection)
        {
            Connection = connection;
        }

        private ISysConnection Connection { get; }

        public void Delete(Guid microService)
        {
            var components = Connection.GetService<IComponentDevelopmentService>().Query(microService);

            foreach (var i in components)
                Connection.GetService<IComponentDevelopmentService>().Delete(i.Token, true);

            var folders = FolderModel.Create(Connection.GetService<IComponentService>().QueryFolders(microService));

            foreach (var i in folders)
                DeleteFolder(i);

            var u = Connection.CreateUrl("MicroServiceManagement", "Delete");
            var args = new JObject {
                {"microService", microService }
            };

            Connection.Post(u, args);

            u = Connection.CreateUrl("Storage", "Clean");
            args = new JObject {
                {"microService", microService }
            };

            Connection.Post(u, args);

            if (Connection.GetService<IMicroServiceService>() is IMicroServiceNotification svc)
                svc.NotifyRemoved(this, new MicroServiceEventArgs(microService));
        }

        private void DeleteFolder(FolderModel model)
        {
            foreach (var i in model.Items)
                DeleteFolder(i);

            Connection.GetService<IComponentDevelopmentService>().DeleteFolder(model.Folder.MicroService, model.Folder.Token);
        }

        public void Insert(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStatus status, IPackage package, string version)
        {
            var u = Connection.CreateUrl("MicroServiceManagement", "Insert");
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

            Connection.Post(u, args);

            if (Connection.GetService<IMicroServiceService>() is IMicroServiceNotification svc)
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

			return Connection.GetService<ICryptographyService>().Encrypt(JsonConvert.SerializeObject(meta));
		}

        private string CreateMicroServiceMeta(Guid microService)
        {
            var u = Connection.CreateUrl("MicroServiceManagement", "CreateMicroServiceMeta")
                .AddParameter("microService", microService);

            return Connection.Get<string>(u);
        }

        public void Update(Guid microService, string name, MicroServiceStatus status, Guid template, Guid resourceGroup, Guid package, Guid plan, UpdateStatus updateStatus, CommitStatus commitStatus)
        {
            var u = Connection.CreateUrl("MicroServiceManagement", "Update");
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

            Connection.Post(u, args);

            if (Connection.GetService<IMicroServiceService>() is IMicroServiceNotification svc)
                svc.NotifyChanged(this, new MicroServiceEventArgs(microService));
        }

        public List<IMicroService> Query(Guid resourceGroup)
        {
            var u = Connection.CreateUrl("MicroServiceManagement", "Query")
                .AddParameter("resourceGroup", resourceGroup);

            return Connection.Get<List<MicroService>>(u).ToList<IMicroService>();
        }

        public List<IMicroServiceString> QueryStrings(Guid microService)
        {
            var u = Connection.CreateUrl("MicroServiceManagement", "QueryStrings")
                .AddParameter("microService", microService);

            return Connection.Get<List<MicroServiceString>>(u).ToList<IMicroServiceString>();
        }
    }
}
