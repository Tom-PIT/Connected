using System;
using TomPIT.Proxy.Development;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local.Development
{
	internal class FolderDevelopmentController : IFolderDevelopmentController
	{
		public void Delete(Guid microService, Guid folder)
		{
			DataModel.Folders.Delete(microService, folder);
		}

		public Guid Insert(Guid microService, string name, Guid parent)
		{
			return DataModel.Folders.Insert(microService, name, parent);
		}

		public void Restore(Guid microService, Guid token, string name, Guid parent)
		{
			DataModel.Folders.Restore(microService, token, name, parent);
		}

		public void Update(Guid microService, Guid folder, string name, Guid parent)
		{
			DataModel.Folders.Update(microService, folder, name, parent);
		}
	}
}
