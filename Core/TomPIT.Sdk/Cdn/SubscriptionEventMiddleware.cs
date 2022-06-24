using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Exceptions;
using TomPIT.IoC;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Security;

namespace TomPIT.Cdn
{
	public abstract class SubscriptionEventMiddleware : MiddlewareOperation, ISubscriptionEventMiddleware
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
			try
			{
				Validate();
				OnValidating();

				foreach (var dependency in DependencyInjections)
					Recipients = dependency.QueryRecipients(Recipients);

				OnInvoke();

				foreach (var dependency in DependencyInjections)
					dependency.Invoke(Recipients);

				Invoked();
			}
			catch (ValidationException)
			{
				Rollback();
				throw;
			}
			catch (Exception ex)
			{
				Rollback();

				throw TomPITException.Unwrap(this, ex);
			}
		}

		protected virtual void OnInvoke()
		{

		}

		public void Commit()
		{
			OnCommit();
		}

		protected internal override void OnCommitting()
		{
			foreach (var dependency in DependencyInjections)
				dependency.Commit();
		}
		
		protected internal override void OnRollbacking()
		{
			foreach (var dependency in DependencyInjections)
				dependency.Rollback();
		}

		protected internal override void OnValidating()
		{
			foreach (var dependency in DependencyInjections)
				dependency.Validate();
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

		protected List<IUser> ResolveUsers(Guid role)
		{
			var membership = Context.Tenant.GetService<IAuthorizationService>().QueryMembershipForRole(role);
			var result = new List<IUser>();

			foreach(var m in membership)
			{
				var user = Context.Services.Identity.GetUser(m.User);

				if (user != null)
					result.Add(user);
			}

			return result;
		}
	}
}
