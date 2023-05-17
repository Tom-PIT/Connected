using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Collections;
using TomPIT.Security;

namespace TomPIT.Proxy.Remote
{
	internal class SecurityController : ISecurityController
	{
		private const string Controller = "Security";
		public ImmutableList<IAuthenticationToken> QueryAuthenticationTokens()
		{
			return Connection.Post<List<AuthenticationToken>>(Connection.CreateUrl(Controller, "QueryAllAuthenticationTokens")).ToImmutableList<IAuthenticationToken>();
		}

		public ImmutableList<IAuthenticationToken> QueryAuthenticationTokens(List<string> resourceGroups)
		{
			return Connection.Post<List<AuthenticationToken>>(Connection.CreateUrl(Controller, "QueryAuthenticationTokens"), new
			{
				data = resourceGroups
			}).ToImmutableList<IAuthenticationToken>();
		}

		public ImmutableList<IMembership> QueryMembership()
		{
			return Connection.Get<List<Membership>>(Connection.CreateUrl(Controller, "QueryMembership")).ToImmutableList<IMembership>();
		}

		public ImmutableList<IPermission> QueryPermissions()
		{
			return Connection.Get<List<Permission>>(Connection.CreateUrl(Controller, "QueryPermissions")).ToImmutableList<IPermission>();
		}

		public ImmutableList<IPermission> QueryPermissions(List<string> resourceGroups)
		{
			return Connection.Post<List<Permission>>(Connection.CreateUrl(Controller, "QueryPermissionsForResourceGroup"), new
			{
				Data = resourceGroups
			}).ToImmutableList<IPermission>();
		}

		public IAuthenticationToken SelectAuthenticationToken(Guid token)
		{
			var u = Connection.CreateUrl(Controller, "SelectAuthenticationToken")
				.AddParameter("token", token);

			return Connection.Get<AuthenticationToken>(u);
		}

		public IPermission SelectPermission(string evidence, string schema, string claim, string primaryKey, string descriptor)
		{
			var u = Connection.CreateUrl(Controller, "SelectPermission")
				.AddParameter("evidence", evidence)
				.AddParameter("schema", schema)
				.AddParameter("claim", claim)
				.AddParameter("primaryKey", primaryKey)
				.AddParameter("descriptor", descriptor);

			return Connection.Get<Permission>(u);
		}

		public ImmutableList<IPermission> SelectPermissions(string primaryKey)
		{
			var u = Connection.CreateUrl(Controller, "SelectPermissions")
				.AddParameter("primaryKey", primaryKey);

			return Connection.Get<List<Permission>>(u).ToImmutableList<IPermission>();
		}

		public IValidationParameters SelectValidationParameters()
		{
			return Connection.Get<ValidationParameters>(Connection.CreateUrl(Controller, "SelectValidationParameters"));
		}
	}
}
