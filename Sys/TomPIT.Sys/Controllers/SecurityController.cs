using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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

		[HttpGet]
		public List<IPermission> SelectPermissions(string primaryKey)
		{
			return DataModel.Permissions.Query(primaryKey);
		}

		[HttpGet]
		public IPermission SelectPermission(Guid evidence, string schema, string claim, string primaryKey)
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
	}
}
