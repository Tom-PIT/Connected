using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;
using TomPIT.Middleware;
using TomPIT.Security;

namespace TomPIT.Management.Security
{
	internal class MembershipManagementService : TenantObject, IMembershipManagementService
	{
		public MembershipManagementService(ITenant tenant) : base(tenant)
		{

		}

		public void Delete(Guid user, Guid role)
		{
			var u = Tenant.CreateUrl("SecurityManagement", "DeleteMembership");
			var e = new JObject
			{
				{"user", user},
				{"role", role}
			};

			Tenant.Post(u, e);

			if (Tenant.GetService<IAuthorizationService>() is IAuthorizationNotification n)
				n.NotifyMembershipRemoved(this, new MembershipEventArgs(user, role));
		}

		public void Insert(Guid user, Guid role)
		{
			var u = Tenant.CreateUrl("SecurityManagement", "InsertMembership");
			var e = new JObject
			{
				{"user", user},
				{"role", role}
			};

			Tenant.Post(u, e);

			if (Tenant.GetService<IAuthorizationService>() is IAuthorizationNotification n)
				n.NotifyMembershipAdded(this, new MembershipEventArgs(user, role));
		}

		public List<IMembership> Query(Guid role)
		{
			var u = Tenant.CreateUrl("SecurityManagement", "QueryMembership")
				.AddParameter("role", role);

			return Tenant.Get<List<Membership>>(u).ToList<IMembership>();
		}
	}
}
