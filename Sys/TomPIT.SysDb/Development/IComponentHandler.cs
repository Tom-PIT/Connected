using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;

namespace TomPIT.SysDb.Development
{
	public interface IComponentHandler
	{
		List<IComponent> Query();
		void Insert(IMicroService microService, DateTime modified, IFolder folder, string category, string nameSpace, string name, Guid token, string type);
		void Update(IComponent component, DateTime modified, string name, IFolder folder);
		void Delete(IComponent component);

		IComponent Select(Guid component);
		IComponent Select(IMicroService microService, string category, string name);
		List<IComponent> Query(string category, string name);
	}
}
