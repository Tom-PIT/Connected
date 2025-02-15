﻿using System;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Security;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers
{
	public class UserController : SysController
	{
		[HttpGet]
		public ImmutableList<IUser> Query()
		{
			return DataModel.Users.Query();
		}

		[HttpGet]
		public IUser Select(string qualifier)
		{
			if (string.IsNullOrWhiteSpace(qualifier))
				return null;

			return DataModel.Users.Resolve(qualifier);
		}

		[HttpGet]
		public IUser SelectByAuthenticationToken(Guid token)
		{
			return DataModel.Users.SelectByAuthenticationToken(token);
		}

		[HttpPost]
		public IUser SelectBySecurityCode()
		{
			var body = FromBody();
			var code = body.Required<string>("securityCode");

			return DataModel.Users.SelectBySecurityCode(code);
		}

		//[HttpPost]
		//public void SignOut()
		//{
			//DataModel.Users.SelectByAuthenticationToken()
		//}

		[HttpGet]
		public ImmutableList<IMembership> QueryMembership(Guid user)
		{
			return DataModel.Membership.Query(user);
		}

		[HttpGet]
		public IMembership SelectMembership(Guid user, Guid role)
		{
			return DataModel.Membership.Select(string.Format("{0}.{1}", user, role));
		}
	}
}
