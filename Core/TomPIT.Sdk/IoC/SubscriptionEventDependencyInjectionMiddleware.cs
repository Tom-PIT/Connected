using System.Collections.Generic;
using TomPIT.Cdn;
using TomPIT.Middleware;

namespace TomPIT.IoC
{
	public abstract class SubscriptionEventDependencyInjectionMiddleware : MiddlewareObject, ISubscriptionEventDependencyInjectionMiddleware
	{
		public void Invoke(List<IRecipient> recipients)
		{
			OnInvoke(recipients);
		}

		protected virtual List<IRecipient> OnInvoke(List<IRecipient> recipients)
		{
			return recipients;
		}

		public List<IRecipient> QueryRecipients(List<IRecipient> recipients)
		{
			return OnQueryRecipients(recipients);
		}

		protected virtual List<IRecipient> OnQueryRecipients(List<IRecipient> recipients)
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

		public void Validate()
		{
			OnValidate();
		}

		protected virtual void OnValidate()
		{

		}

		public void Commit()
		{
			OnCommit();
		}

		protected virtual void OnCommit()
		{

		}
		public void Rollback()
		{
			
		}

		protected virtual void OnRollback()
		{

		}
	}
}
