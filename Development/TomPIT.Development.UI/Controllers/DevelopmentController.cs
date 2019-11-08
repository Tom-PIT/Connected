using Microsoft.AspNetCore.Authorization;
using TomPIT.Controllers;

namespace TomPIT.Development.Controllers
{
	[Authorize(Roles = "Full Control, Development")]
	public class DevelopmentController : ServerController
	{
	}
}
