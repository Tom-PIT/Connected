using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Features;

namespace TomPIT.SysDb.Development
{
	public interface IComponentHandler
	{
		List<IComponent> Query();
		void Insert(IMicroService microService, DateTime modified, IFeature feature, string category, string name, Guid token, string type, Guid runtimeConfiguration);
		void Update(IComponent component, DateTime modified, string name, Guid runtimeConfiguration);
		void Delete(IComponent component);

		IComponent Select(Guid component);
		IComponent Select(IMicroService microService, string category, string name);
		List<IComponent> Query(string category, string name);
	}
}
