using System.Collections.Generic;
using TomPIT.Cdn;
using TomPIT.Middleware;

namespace TomPIT.IoC
{
	public abstract class SubscriptionDependencyInjectionMiddleware : MiddlewareObject, ISubscriptionDependencyInjectionMiddleware
	{
		public void Created()
		{
			OnCreated();
		}

		protected virtual void OnCreated()
		{

		}

		public List<IRecipient> Invoke(List<IRecipient> recipients)
		{
			return OnInvoke(recipients);
		}

		protected virtual List<IRecipient> OnInvoke(List<IRecipient> recipients)
		{
			return recipients;
		}

		protected IRecipient CreateUserRecipient(string identifier)
		{
			return CdnUtils.CreateUserRecipient(Context, identifier);
		}

		protected IRecipient CreateRoleRecipient(string roleName)
		{
			return CdnUtils.CreateRoleRecipient(Context, roleName);
		}

		protected IRecipient CreateAlienRecipient(string email)
		{
			return CdnUtils.CreateAlienRecipient(Context, email);
		}
	}
}
