using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TomPIT.Sys.Controllers
{
	[AllowAnonymous]
	public class TestConnectionController : SysController
	{
		[HttpGet]
		public string Test()
		{
			return DateTime.UtcNow.ToString("G");
		}
	}
}
