using System.Collections.Generic;
using System.Linq;
using TomPIT.Connectivity;
using TomPIT.Development;
using TomPIT.Middleware;

namespace TomPIT.Design
{
	internal class Repositories : TenantObject, IRepositories
	{
		private const string Controller = "VersionControl";
		public Repositories(ITenant tenant) : base(tenant)
		{
		}

		public void Delete(string name)
		{
			Tenant.Post(CreateUrl("DeleteRepository"), new
			{
				Name = name
			});
		}

		public IRepositoriesEndpoint Select(string name)
		{
			return Tenant.Post<RepositoriesEndpoint>(CreateUrl("SelectRepository"), new
			{
				Name = name
			});
		}

		public void Insert(string name, string url, string userName, string password)
		{
			Tenant.Post(CreateUrl("InsertRepository"), new
			{
				Name = name,
				Url = url,
				UserName = userName,
				Password = password
			});
		}

		public List<IRepositoriesEndpoint> Query()
		{
			return Tenant.Get<List<RepositoriesEndpoint>>(CreateUrl("QueryRepositories")).ToList<IRepositoriesEndpoint>();
		}

		public void Update(string existingName, string name, string url, string userName, string password)
		{
			Tenant.Post(CreateUrl("UpdateRepository"), new
			{
				ExistingName = existingName,
				Name = name,
				Url = url,
				UserName = userName,
				Password = password
			});
		}

		private ServerUrl CreateUrl(string action)
		{
			return Tenant.CreateUrl(Controller, action);
		}
	}
}
