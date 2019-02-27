using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Threading.Tasks;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Security;
using TomPIT.Services;

namespace TomPIT.Runtime.Security
{
	public class ClaimHandler : AuthorizationHandler<ClaimRequirement>
	{
		public ClaimHandler(IActionContextAccessor accessor)
		{
			Accessor = accessor;
		}

		private IActionContextAccessor Accessor { get; }

		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ClaimRequirement requirement)
		{
			var ms = Accessor.ActionContext.RouteData.Values["microService"];

			if (string.IsNullOrWhiteSpace(ms.ToString()))
			{
				context.Fail();
				return Task.CompletedTask;
			}

			if (!(context.User.Identity is Identity identity) || !identity.IsAuthenticated)
			{
				context.Fail();
				return Task.CompletedTask;
			}

			if (context.User.IsInRole("Full control"))
			{
				context.Succeed(requirement);
				return Task.CompletedTask;
			}

			var connection = Shell.GetService<IConnectivityService>().Select(identity.Endpoint);
			var microService = connection.GetService<IMicroServiceService>().SelectByUrl(ms.ToString());

			if (microService == null)
			{
				context.Fail();
				return Task.CompletedTask;
			}

			var e = new AuthorizationArgs(identity.User.Token, requirement.Claim, microService.Token.ToString());
			var r = connection.GetService<TomPIT.Security.IAuthorizationService>().Authorize(new ExecutionContext(microService), e);

			if (!r.Success)
			{
				context.Fail();

				return Task.CompletedTask;
			}

			context.Succeed(requirement);

			return Task.CompletedTask;
		}
	}
}
