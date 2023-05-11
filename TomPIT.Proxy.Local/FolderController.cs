using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.ComponentModel;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local
{
	internal class FolderController : IFolderController
	{
		public ImmutableList<IFolder> Query()
		{
			return DataModel.Folders.Query();
		}

		public ImmutableList<IFolder> Query(List<string> resourceGroups)
		{
			return DataModel.Folders.Query(resourceGroups);
		}

		public ImmutableList<IFolder> Query(Guid microService)
		{
			return DataModel.Folders.Query(microService);
		}

		public IFolder Select(Guid token)
		{
			return DataModel.Folders.Select(token);
		}

		public IFolder Select(Guid microService, string name)
		{
			return DataModel.Folders.Select(microService, name);
		}
	}
}
