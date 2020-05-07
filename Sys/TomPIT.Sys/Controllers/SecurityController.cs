using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.Security;
using TomPIT.Sys.Data;
using TomPIT.Sys.Security;

namespace TomPIT.Sys.Controllers
{
	public class SecurityController : SysController
	{
		[HttpGet]
		public List<IPermission> QueryPermissions()
		{
			return DataModel.Permissions.Query();
		}

		[HttpPost]
		public List<IPermission> QueryPermissionsForResourceGroup()
		{
			var body = FromBody().ToResults();

			if (body == null)
				return null;

			var list = new List<string>();

			foreach (JValue i in body)
				list.Add(i.Value<string>());

			return DataModel.Permissions.Query(list);
		}

		[HttpGet]
		public List<IPermission> SelectPermissions(string primaryKey)
		{
			return DataModel.Permissions.Query(primaryKey);
		}

		[HttpGet]
		public IPermission SelectPermission(string evidence, string schema, string claim, string primaryKey)
		{
			return DataModel.Permissions.Select(evidence, schema, claim, primaryKey);
		}

		[HttpGet]
		public List<IMembership> QueryMembership()
		{
			return DataModel.Membership.Query();
		}

		[HttpGet]
		public IValidationParameters SelectValidationParameters()
		{
			return new ValidationParameters
			{
				IssuerSigningKey = TomPITAuthenticationHandler.IssuerSigningKey,
				ValidAudience = TomPITAuthenticationHandler.ValidAudience,
				ValidIssuer = TomPITAuthenticationHandler.ValidIssuer
			};
		}

		[HttpPost]
		public List<IAuthenticationToken> QueryAllAuthenticationTokens()
		{
			return DataModel.AuthenticationTokens.Query();
		}

		[HttpPost]
		public List<IAuthenticationToken> QueryAuthenticationTokens()
		{
			var body = FromBody().ToResults();

			if (body == null)
				return null;

			var list = new List<string>();

			foreach (JValue i in body)
				list.Add(i.Value<string>());

			return DataModel.AuthenticationTokens.Query(list);
		}

		[HttpGet]
		public IAuthenticationToken SelectAuthenticationToken(Guid token)
		{
			return DataModel.AuthenticationTokens.Select(token);
		}
	}
}
