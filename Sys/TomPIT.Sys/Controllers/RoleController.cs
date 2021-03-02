using System;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Security;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers
{
	public class RoleController : SysController
	{
		[HttpGet]
		public ImmutableList<IRole> Query()
		{
			return DataModel.Roles.Query();
		}

		[HttpGet]
		public IRole Select(Guid token)
		{
			return DataModel.Roles.Select(token);
		}
	}
}
