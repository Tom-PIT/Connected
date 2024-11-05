using System;
using TomPIT.Proxy.Development;

namespace TomPIT.Proxy.Remote.Development
{
	internal class ComponentDevelopmentController : IComponentDevelopmentController
	{
		private const string Controller = "ComponentDevelopment";

		public string CreateName(Guid microService, string nameSpace, string prefix)
		{
			var u = Connection.CreateUrl(Controller, "CreateName")
				.AddParameter("microService", microService)
				.AddParameter("nameSpace", nameSpace)
				.AddParameter("prefix", prefix);

			return Connection.Get<string>(u);
		}

		public void Delete(Guid component, Guid user)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Delete"), new
			{
				component,
				user
			});
		}

		public void Insert(Guid microService, Guid folder, Guid token, string nameSpace, string category, string name, string type)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Insert"), new
			{
				microService,
				folder,
				name,
				category,
				type,
				component = token,
				nameSpace
			});
		}

		public void Update(Guid component, string name, Guid folder)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Update"), new
			{
				name,
				component,
				folder
			});
		}
	}
}
