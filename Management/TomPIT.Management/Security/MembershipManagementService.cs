using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Net;

namespace TomPIT.Security
{
	internal class MembershipManagementService : IMembershipManagementService
	{
		public MembershipManagementService(ISysContext server)
		{
			Server = server;
		}

		private ISysContext Server { get; }

		public void Delete(Guid user, Guid role)
		{
			var u = Server.CreateUrl("SecurityManagement", "DeleteMembership");
			var e = new JObject
			{
				{"user", user},
				{"role", role}
			};

			Server.Connection.Post(u, e);

			if (Server.GetService<IAuthorizationService>() is IAuthorizationNotification n)
				n.NotifyMembershipRemoved(this, new MembershipEventArgs(user, role));
		}

		public void Insert(Guid user, Guid role)
		{
			var u = Server.CreateUrl("SecurityManagement", "InsertMembership");
			var e = new JObject
			{
				{"user", user},
				{"role", role}
			};

			Server.Connection.Post(u, e);

			if (Server.GetService<IAuthorizationService>() is IAuthorizationNotification n)
				n.NotifyMembershipAdded(this, new MembershipEventArgs(user, role));
		}

		public List<IMembership> Query(Guid role)
		{
			var u = Server.CreateUrl("SecurityManagement", "QueryMembership")
				.AddParameter("role", role);

			return Server.Connection.Get<List<Membership>>(u).ToList<IMembership>();
		}
	}
}
