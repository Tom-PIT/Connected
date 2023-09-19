using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Deployment;
using TomPIT.Design;
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

        public void Insert(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStatus status, IPackage package, string version)
        {
            Instance.SysProxy.Management.MicroServices.Insert(token, name, resourceGroup, template, status, CreateMeta(token, package), version);

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

        public void Update(Guid microService, string name, MicroServiceStatus status, Guid template, Guid resourceGroup, Guid package, Guid plan, UpdateStatus updateStatus, CommitStatus commitStatus)
        {
            Instance.SysProxy.Management.MicroServices.Update(microService, name, resourceGroup, template, status, updateStatus, commitStatus, package, plan);

            if (Tenant.GetService<IMicroServiceService>() is IMicroServiceNotification svc)
                svc.NotifyChanged(this, new MicroServiceEventArgs(microService));
        }

        public List<IMicroService> Query(Guid resourceGroup)
        {
            return Instance.SysProxy.Management.MicroServices.Query(resourceGroup).ToList();
        }

        public List<IMicroServiceString> QueryStrings(Guid microService)
        {
            return new();
        }
    }
}
