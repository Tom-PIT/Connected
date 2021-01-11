using System.Collections.Generic;
using System.Security.Claims;
using TomPIT.Middleware;

namespace TomPIT.Security
{
	public class Identity : ClaimsIdentity
	{
		private bool _isAuthenticated = true;
		private List<Claim> _claims = null;

		public Identity(IUser user) : this(user, null, null)
		{
		}

		public Identity(IUser user, string jwToken, string endpoint)
		{
			User = user;
			Token = jwToken;
			Endpoint = endpoint;
			Name = user.AuthenticationToken.ToString();
		}

		public override string AuthenticationType => "Tom PIT";
		public override bool IsAuthenticated { get { return _isAuthenticated; } }
		public override string Name { get; }
		public string Token { get; }

		public string Endpoint { get; }
		public IUser User { get; }

		public static Identity NotAuthenticated()
		{
			return new Identity(null, null, null)
			{
				_isAuthenticated = false
			};
		}

		public override IEnumerable<Claim> Claims
		{
			get
			{
				return _claims ??= CreateClaims();
			}
		}

		public override bool HasClaim(string type, string value)
		{
			return base.HasClaim(type, value);
		}

		public override bool HasClaim(System.Predicate<Claim> match)
		{
			return base.HasClaim(match);
		}

		private List<Claim> CreateClaims()
		{
			using var ctx = new MiddlewareContext(string.IsNullOrWhiteSpace(Endpoint) ? MiddlewareDescriptor.Current.Tenant.Url : Endpoint);
			var service = ctx.Tenant.GetService<IAuthorizationService>();
			var isAdmin = User != null ? service.IsInRole(User.Token, "Full control") : false;

			if (isAdmin)
			{
				return new List<Claim>()
				{
					new Claim(TomPIT.Claims.ImplementMicroservice, true.ToString(), null, AuthenticationType)
				};
			}
			else
			{
				var result = new List<Claim>();

				if (User == null)
					return result;

				var args = new AuthorizationArgs(User.Token, TomPIT.Claims.ImplementMicroservice, null, null);

				args.Schema.Level = AuthorizationLevel.Pessimistic;
				args.Schema.Empty = EmptyBehavior.Deny;

				if (service.Authorize(ctx, args).Success)
					result.Add(new Claim(TomPIT.Claims.ImplementMicroservice, true.ToString(), null, AuthenticationType));

				return result;
			}
		}
	}
}
