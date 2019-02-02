using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;

namespace TomPIT.SysDb.Development
{
	public interface IFolderHandler
	{
		List<IFolder> Query();
		void Insert(IMicroService microService, string name, Guid token, IFolder parent);
		void Update(IFolder folder, string name, IFolder parent);
		void Delete(IFolder folder);

		IFolder Select(Guid token);
		IFolder Select(IMicroService microService, string name);
	}
}
