using System;
using TomPIT.Proxy.Development;

namespace TomPIT.Proxy.Remote.Development
{
	internal class FolderDevelopmentController : IFolderDevelopmentController
	{
		private const string Controller = "FolderDevelopment";

		public void Delete(Guid microService, Guid folder)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Delete"), new
			{
				microService,
				token = folder
			});
		}

		public Guid Insert(Guid microService, string name, Guid parent)
		{
			return Connection.Post<Guid>(Connection.CreateUrl(Controller, "Insert"), new
			{
				microService,
				name,
				parent
			});
		}

		public void Restore(Guid microService, Guid token, string name, Guid parent)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Restore"), new
			{
				microService,
				token,
				name,
				parent
			});
		}

		public void Update(Guid microService, Guid folder, string name, Guid parent)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Update"), new
			{
				microService,
				token = folder,
				name,
				parent
			});
		}
	}
}
