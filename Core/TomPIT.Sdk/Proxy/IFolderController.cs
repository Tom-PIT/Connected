using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.ComponentModel;

namespace TomPIT.Proxy
{
	public interface IFolderController
	{
		public ImmutableList<IFolder> Query();
		public ImmutableList<IFolder> Query(List<string> resourceGroups);
		public ImmutableList<IFolder> Query(Guid microService);
		public IFolder Select(Guid token);
		public IFolder Select(Guid microService, string name);
	}
}
