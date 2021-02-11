using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Security;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers
{
	public class RoleController : SysController
	{
		[HttpGet]
		public List<IRole> Query()
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
