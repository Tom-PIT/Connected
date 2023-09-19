using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.ComponentModel;

namespace TomPIT.Proxy.Remote
{
	internal class FolderController : IFolderController
	{
		private const string Controller = "Folder";

		public ImmutableList<IFolder> Query()
		{
			return Connection.Get<List<Folder>>(Connection.CreateUrl(Controller, "Query")).ToImmutableList<IFolder>();
		}

		public ImmutableList<IFolder> Query(List<string> resourceGroups)
		{
			return Connection.Post<List<Folder>>(Connection.CreateUrl(Controller, "QueryForResourceGroups"), resourceGroups).ToImmutableList<IFolder>();
		}

		public ImmutableList<IFolder> Query(Guid microService)
		{
			return Connection.Post<List<Folder>>(Connection.CreateUrl(Controller, "QueryForMicroService"), new
			{
				microService
			}).ToImmutableList<IFolder>();
		}

		public IFolder Select(Guid token)
		{
			var u = Connection.CreateUrl(Controller, "SelectByToken")
				.AddParameter("token", token);

			return Connection.Get<Folder>(u);
		}

		public IFolder Select(Guid microService, string name)
		{
			var u = Connection.CreateUrl(Controller, "Select")
				.AddParameter("microService", microService)
				.AddParameter("name", name);

			return Connection.Get<Folder>(u);
		}
	}
}
