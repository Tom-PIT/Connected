using System;
using TomPIT.Proxy.Development;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local.Development
{
	internal class ComponentDevelopmentController : IComponentDevelopmentController
	{
		public string CreateName(Guid microService, string nameSpace, string prefix)
		{
			return DataModel.Components.CreateComponentName(microService, prefix, nameSpace);
		}

		public void Delete(Guid component, Guid user)
		{
			DataModel.Components.Delete(component, user);
		}

		public void Insert(Guid microService, Guid folder, Guid token, string nameSpace, string category, string name, string type)
		{
			DataModel.Components.Insert(token, microService, folder, category, nameSpace, name, type);
		}

		public void Update(Guid component, string name, Guid folder)
		{
			DataModel.Components.Update(component, name, folder);
		}
	}
}
