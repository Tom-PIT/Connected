﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.Security;
using TomPIT.Sys.Model;
using TomPIT.Sys.Security;

namespace TomPIT.Sys.Controllers
{
	public class SecurityController : SysController
	{
		[HttpGet]
		public ImmutableList<IPermission> QueryPermissions()
		{
			return DataModel.Permissions.Query();
		}

		[HttpPost]
		public ImmutableList<IPermission> QueryPermissionsForResourceGroup()
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
		public ImmutableList<IPermission> SelectPermissions(string primaryKey)
		{
			return DataModel.Permissions.Query(primaryKey);
		}

		[HttpGet]
		public IPermission SelectPermission(string evidence, string schema, string claim, string primaryKey, string descriptor)
		{
			return DataModel.Permissions.Select(evidence, schema, claim, primaryKey, descriptor);
		}

		[HttpGet]
		public ImmutableList<IMembership> QueryMembership()
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
		public ImmutableList<IAuthenticationToken> QueryAllAuthenticationTokens()
		{
			return DataModel.AuthenticationTokens.Query();
		}

		[HttpPost]
		public ImmutableList<IAuthenticationToken> QueryAuthenticationTokens()
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
