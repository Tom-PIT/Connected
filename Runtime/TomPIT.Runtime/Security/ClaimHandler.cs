using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Middleware;

namespace TomPIT.Security
{
	public class ClaimHandler : AuthorizationHandler<ClaimRequirement>
	{
		public ClaimHandler(IHttpContextAccessor context)
		{
			Context = context;
		}

		private IHttpContextAccessor Context { get; }

		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ClaimRequirement requirement)
		{
			var ms = Context.HttpContext.Request.RouteValues["microService"] as string;

			if (string.IsNullOrWhiteSpace(ms))
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

			var connection = Shell.GetService<IConnectivityService>().SelectTenant(identity.Endpoint);
			var microService = connection.GetService<IMicroServiceService>().SelectByUrl(ms.ToString());

			if (microService == null)
			{
				context.Fail();
				return Task.CompletedTask;
			}

			var e = new AuthorizationArgs(identity.User.Token, requirement.Claim, microService.Token.ToString(), "Micro service");
			using var ctx = new MicroServiceContext(microService);
			var r = connection.GetService<TomPIT.Security.IAuthorizationService>().Authorize(ctx, e);

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
