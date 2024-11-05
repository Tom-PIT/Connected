using System;

namespace TomPIT.Proxy.Development
{
	public interface IComponentDevelopmentController
	{
		void Insert(Guid microService, Guid folder, Guid token, string nameSpace, string category, string name, string type);
		void Update(Guid component, string name, Guid folder);
		void Delete(Guid component, Guid user);

		string CreateName(Guid microService, string nameSpace, string prefix);
	}
}
