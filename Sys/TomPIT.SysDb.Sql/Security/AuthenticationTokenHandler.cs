using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Data.Sql;
using TomPIT.Environment;
using TomPIT.Security;
using TomPIT.SysDb.Security;

namespace TomPIT.SysDb.Sql.Security
{
	internal class AuthenticationTokenHandler : IAuthenticationTokenHandler
	{
		public void Delete(IAuthenticationToken token)
		{
			using var w = new Writer("tompit.auth_token_del");

			w.CreateParameter("@id", token.GetId());

			w.Execute();
		}

		public void Insert(IResourceGroup resourceGroup, IUser user, Guid token, string name, string description, string key, AuthenticationTokenClaim claims, AuthenticationTokenStatus status,
			DateTime validFrom, DateTime validTo, TimeSpan startTime, TimeSpan endTime, string ipRestrictions)
		{
			using var w = new Writer("tompit.auth_token_ins");

			w.CreateParameter("@token", token);
			w.CreateParameter("@user", user.GetId());
			w.CreateParameter("@key", key);
			w.CreateParameter("@claims", claims);
			w.CreateParameter("@status", status);
			w.CreateParameter("@valid_from", validFrom, true);
			w.CreateParameter("@valid_to", validTo, true);
			w.CreateParameter("@start_time", startTime, true);
			w.CreateParameter("@end_time", endTime, true);
			w.CreateParameter("@ip_restrictions", ipRestrictions, true);
			w.CreateParameter("@resource_group", resourceGroup.GetId());
			w.CreateParameter("@name", name);
			w.CreateParameter("@description", description, true);

			w.Execute();
		}

		public List<IAuthenticationToken> Query()
		{
			using var r = new Reader<AuthenticationToken>("tompit.auth_token_que");

			return r.Execute().ToList<IAuthenticationToken>();
		}

		public IAuthenticationToken Select(Guid token)
		{
			using var r = new Reader<AuthenticationToken>("tompit.auth_token_sel");

			r.CreateParameter("@token", token);

			return r.ExecuteSingleRow();
		}

		public void Update(IAuthenticationToken authToken, IUser user, string name, string description, string key, AuthenticationTokenClaim claims, AuthenticationTokenStatus status, DateTime validFrom, DateTime validTo,
			TimeSpan startTime, TimeSpan endTime, string ipRestrictions)
		{
			using var w = new Writer("tompit.auth_token_upd");

			w.CreateParameter("@id", authToken.GetId());
			w.CreateParameter("@user", user.GetId());
			w.CreateParameter("@key", key);
			w.CreateParameter("@claims", claims);
			w.CreateParameter("@status", status);
			w.CreateParameter("@valid_from", validFrom, true);
			w.CreateParameter("@valid_to", validTo, true);
			w.CreateParameter("@start_time", startTime, true);
			w.CreateParameter("@end_time", endTime, true);
			w.CreateParameter("@ip_restrictions", ipRestrictions, true);
			w.CreateParameter("@name", name);
			w.CreateParameter("@description", description, true);

			w.Execute();
		}
	}
}
