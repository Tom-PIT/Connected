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

		public void Insert(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStages stages, string version, string commit)
		{
			Instance.SysProxy.Management.MicroServices.Insert(token, name, resourceGroup, template, stages, version, commit);
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

		public void Update(Guid token, string name, Guid resourceGroup, Guid template, MicroServiceStages stages, string version, string commit)
		{
			Instance.SysProxy.Management.MicroServices.Update(token, name, resourceGroup, template, stages, version, commit);

			if (Tenant.GetService<IMicroServiceService>() is IMicroServiceNotification notification)
				notification.NotifyChanged(this, new MicroServiceEventArgs(token));
		}

		public void IncrementVersion(Guid token)
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(token);

			if (ms is null)
				return;

			var version = ms.Version;
			var major = DateTime.UtcNow.ToString("yy");
			var minor = DateTime.UtcNow.ToString("MM");
			var build = DateTime.UtcNow.Day.ToString().PadLeft(2, '0');
			var revision = "0";

			if (!string.IsNullOrEmpty(version))
			{
				string currentRevision = version.Split('.')[^1];

				if (!string.IsNullOrEmpty(currentRevision) && Types.TryConvert(currentRevision, out int rev))
					revision = (++rev).ToString();
			}

			version = $"{major}.{minor}.{build}.{revision}";

			Update(token, ms.Name, ms.ResourceGroup, ms.Template, ms.SupportedStages, version, ms.Commit);
		}
	}
}