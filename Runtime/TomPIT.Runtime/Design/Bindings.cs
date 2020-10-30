using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Connectivity;
using TomPIT.Development;
using TomPIT.Middleware;

namespace TomPIT.Design
{
	internal class Bindings : TenantObject, IServiceBindings
	{
		private const string Controller = "VersionControl";
		public Bindings(ITenant tenant) : base(tenant)
		{
		}

		public void Delete(Guid service, string repository)
		{
			Tenant.Post(CreateUrl("DeleteBinding"), new
			{
				Service = service,
				Repository = repository
			});
		}

		public List<IServiceBinding> QueryActive()
		{
			return Tenant.Get<List<ServiceBinding>>(CreateUrl("QueryActiveBindings")).ToList<IServiceBinding>();
		}

		public IServiceBinding Select(Guid service, string repository)
		{
			return Tenant.Post<ServiceBinding>(CreateUrl("SelectBinding"), new
			{
				Service = service,
				Repository = repository
			});
		}

		public void Update(Guid service, string repository, long commit, DateTime date, bool active)
		{
			Tenant.Post(CreateUrl("UpdateBinding"), new
			{
				Service = service,
				Repository = repository,
				Commit = commit,
				Date = date,
				Active = active,
			});
		}

		private ServerUrl CreateUrl(string action)
		{
			return Tenant.CreateUrl(Controller, action);
		}
	}
}
