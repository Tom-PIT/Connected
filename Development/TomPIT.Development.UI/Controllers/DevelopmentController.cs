using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TomPIT.Controllers
{
	[Authorize(Roles = "Full Control, Development")]
	public class DevelopmentController:ServerController
	{
	}
}
