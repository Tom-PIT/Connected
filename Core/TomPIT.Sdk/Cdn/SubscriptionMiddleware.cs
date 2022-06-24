using System;
using System.Collections.Generic;
using TomPIT.Annotations;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.IoC;
using TomPIT.Middleware;

namespace TomPIT.Cdn
{
	public abstract class SubscriptionMiddleware : MiddlewareComponent, ISubscriptionMiddleware
	{
		private List<ISubscriptionDependencyInjectionMiddleware> _dependencies = null;

		[SkipValidation]
		protected internal List<ISubscriptionDependencyInjectionMiddleware> DependencyInjections
		{
			get
			{
				if (_dependencies == null)
				{
					var component = Context.Tenant.GetService<ICompilerService>().ResolveComponent(this);

					if (component != null)
					{
						var ms = Context.Tenant.GetService<IMicroServiceService>().Select(component.MicroService);

						_dependencies = Context.Tenant.GetService<IDependencyInjectionService>().QuerySubscriptionDependencies($"{ms.Name}/{component.Name}", this);
					}

					if (_dependencies == null)
						_dependencies = new List<ISubscriptionDependencyInjectionMiddleware>();
				}

				return _dependencies;
			}
		}

		protected ISubscription Subscription { get; private set; }
		public void Created(ISubscription subscription)
		{
			Subscription = subscription;

			OnCreated();

			foreach (var dependency in DependencyInjections)
				dependency.Created();
		}


		protected virtual void OnCreated()
		{

		}

		public List<IRecipient> Invoke(ISubscription subscription)
		{
			Validate();

			Subscription = subscription;

			var result = OnInvoke();

			foreach (var dependency in DependencyInjections)
				dependency.Invoke(result);

			return result;
		}

		protected virtual List<IRecipient> OnInvoke()
		{
			return null;
		}

		protected IRecipient CreateUserRecipient(string identifier)
		{
			return CreateUserRecipient(identifier, null);
		}

		protected IRecipient CreateUserRecipient(string identifier, List<string> tags)
		{
			return CdnUtils.CreateUserRecipient(Context, identifier, tags);
		}

		protected IRecipient CreateRoleRecipient(string roleName)
		{
			return CreateRoleRecipient(roleName, null);
		}

		protected IRecipient CreateRoleRecipient(string roleName, List<string> tags)
		{
			return CdnUtils.CreateRoleRecipient(Context, roleName, tags);
		}

		protected IRecipient CreateAlienRecipient(string email)
		{
			return CreateAlienRecipient(email, null);
		}

		protected IRecipient CreateAlienRecipient(string firstName = null, string lastName = null, string email = null, string mobile = null, string phone = null,
			Guid language = default, string timezone = null, string resourceType = null, string resourcePrimaryKey = null, List<string> tags = null)
		{
			return CdnUtils.CreateAlienRecipient(Context, firstName, lastName, email, mobile, phone, language, timezone, resourceType, resourcePrimaryKey, tags);
		}
	}
}
