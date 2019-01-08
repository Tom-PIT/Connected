using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Connectivity;

namespace TomPIT.Security
{
	internal class MembershipManagementService : IMembershipManagementService
	{
		public MembershipManagementService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

		public void Delete(Guid user, Guid role)
		{
			var u = Connection.CreateUrl("SecurityManagement", "DeleteMembership");
			var e = new JObject
			{
				{"user", user},
				{"role", role}
			};

			Connection.Post(u, e);

			if (Connection.GetService<IAuthorizationService>() is IAuthorizationNotification n)
				n.NotifyMembershipRemoved(this, new MembershipEventArgs(user, role));
		}

		public void Insert(Guid user, Guid role)
		{
			var u = Connection.CreateUrl("SecurityManagement", "InsertMembership");
			var e = new JObject
			{
				{"user", user},
				{"role", role}
			};

			Connection.Post(u, e);

			if (Connection.GetService<IAuthorizationService>() is IAuthorizationNotification n)
				n.NotifyMembershipAdded(this, new MembershipEventArgs(user, role));
		}

		public List<IMembership> Query(Guid role)
		{
			var u = Connection.CreateUrl("SecurityManagement", "QueryMembership")
				.AddParameter("role", role);

			return Connection.Get<List<Membership>>(u).ToList<IMembership>();
		}
	}
}
