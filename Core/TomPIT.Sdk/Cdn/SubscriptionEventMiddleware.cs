using System.Collections.Generic;
using TomPIT.Annotations;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.IoC;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Cdn
{
	public abstract class SubscriptionEventMiddleware : MiddlewareComponent, ISubscriptionEventMiddleware
	{
		private List<ISubscriptionEventDependencyInjectionMiddleware> _dependencies = null;

		[SkipValidation]
		protected internal List<ISubscriptionEventDependencyInjectionMiddleware> DependencyInjections
		{
			get
			{
				if (_dependencies == null)
				{
					var component = Context.Tenant.GetService<ICompilerService>().ResolveComponent(this);

					if (component != null)
					{
						var ms = Context.Tenant.GetService<IMicroServiceService>().Select(component.MicroService);

						_dependencies = Context.Tenant.GetService<IDependencyInjectionService>().QuerySubscriptionEventDependencies($"{ms.Name}/{component.Name}/{GetType().ShortName()}", this);
					}

					if (_dependencies == null)
						_dependencies = new List<ISubscriptionEventDependencyInjectionMiddleware>();
				}

				return _dependencies;
			}
		}

		public ISubscriptionEvent Event { get; set; }
		public List<IRecipient> Recipients { get; set; }

		public void Invoke()
		{
			Validate();

			foreach (var dependency in DependencyInjections)
				Recipients = dependency.QueryRecipients(Recipients);

			OnInvoke();

			foreach (var dependency in DependencyInjections)
				dependency.Invoke(Recipients);

			Commit();
		}

		protected virtual void OnInvoke()
		{

		}

		public void Commit()
		{
			OnCommit();
		}

		protected virtual void OnCommit()
		{

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
