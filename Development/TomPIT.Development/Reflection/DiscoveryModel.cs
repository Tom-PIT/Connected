using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Middleware;
using TomPIT.Security;

namespace TomPIT.Development.Reflection
{
	public class DiscoveryModel : MiddlewareObject
	{
		private List<IMicroService> _services = null;

		public DiscoveryModel(IMiddlewareContext context) : base(context)
		{

		}

		public List<IMicroService> Services
		{
			get
			{
				if (_services == null)
				{
					_services = new List<IMicroService>();
					var ms = Context.Tenant.GetService<IMicroServiceService>().Query();

					foreach (var i in ms)
					{
						if (!Authorize(i))
							continue;

						_services.Add(i);
					}
				}

				return _services;
			}
		}

		private bool Authorize(IMicroService microService)
		{
			var e = new AuthorizationArgs(Context.Services.Identity.IsAuthenticated ? Context.Services.Identity.User.Token : Guid.Empty,
				Claims.ImplementMicroservice, microService.Token.ToString(), "Micro service");

			e.Schema.Empty = EmptyBehavior.Deny;
			e.Schema.Level = AuthorizationLevel.Pessimistic;

			return Context.Tenant.GetService<IAuthorizationService>().Authorize(Context, e).Success;
		}
	}
}
