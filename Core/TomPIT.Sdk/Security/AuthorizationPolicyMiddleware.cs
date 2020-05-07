using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TomPIT.Annotations;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Security
{
	public abstract class AuthorizationPolicyMiddleware : MiddlewareComponent, IAuthorizationPolicyMiddleware
	{
		private List<IAuthorizationPolicyClaim> _claims = null;
		private List<string> _policies = null;
		public List<IAuthorizationPolicyClaim> Claims
		{
			get
			{
				if (_claims == null)
					_claims = new List<IAuthorizationPolicyClaim>();

				return _claims;
			}
		}

		protected List<string> Policies
		{
			get
			{
				if (_policies == null)
					_policies = new List<string>();

				return _policies;
			}
		}

		public List<IAuthorizationPolicyEntity> QueryEntities(IAuthorizationPolicyClaim claim)
		{
			return OnQueryEntities(claim);
		}

		protected virtual List<IAuthorizationPolicyEntity> OnQueryEntities(IAuthorizationPolicyClaim claim)
		{
			return new List<IAuthorizationPolicyEntity>();
		}

		private object _instance;
		protected object Instance
		{
			get { return _instance; }
			private set
			{
				_instance = value;

				if (_instance != null && _instance is IMiddlewareObject mo)
					ReflectionExtensions.SetPropertyValue(this, nameof(Context), mo.Context);
			}
		}

		public void Authorize(object instance, string policy)
		{
			Instance = instance;

			ValidatePolicy(policy);

			var messages = new List<PolicyAuthorizationResult>();

			OnAuthorize(policy, messages);

			if (messages.Count > 0)
			{
				var sb = new StringBuilder();

				foreach (var message in messages)
					sb.AppendLine(message.Message);

				throw new ForbiddenException(sb.ToString());
			}
		}

		protected virtual void OnAuthorize(string policy, List<PolicyAuthorizationResult> results)
		{
			return;
		}

		protected virtual void ValidatePolicy(string policy)
		{
			if (!Policies.Contains(policy, StringComparer.OrdinalIgnoreCase))
				throw new ForbiddenException($"{SR.AuthorizationPolicyNotSupported} ({policy})");
		}

		protected bool AuthorizeAny(string primaryKey, List<IAuthorizationPolicyClaim> claims)
		{
			return AuthorizeAny(primaryKey, claims.Select(f => f.Name).ToArray());
		}
		protected bool AuthorizeAny(string primaryKey, params string[] claims)
		{
			foreach (var claim in claims)
			{
				if (Context.Services.Authorization.Authorize(claim, primaryKey))
					return true;
			}

			return false;
		}

		protected bool AuthorizeAll(string primaryKey, List<IAuthorizationPolicyClaim> claims)
		{
			return AuthorizeAll(primaryKey, claims.Select(f => f.Name).ToArray());
		}
		protected bool AuthorizeAll(string primaryKey, params string[] claims)
		{
			foreach (var claim in claims)
			{
				if (!Context.Services.Authorization.Authorize(claim, primaryKey))
					return false;
			}

			return true;
		}

		protected T GetValue<T>(string propertyName)
		{
			var properties = Instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
			/*
			 * First check if attribute is defined on any of the properties. 
			 * This have a higher priority than a property name
			 */
			foreach (var property in properties)
			{
				var attribute = property.FindAttribute<AuthorizationPropertyAttribute>();

				if (attribute != null && string.Compare(attribute.PropertyName, propertyName, true) == 0)
					return Types.Convert<T>(property.GetValue(Instance));
			}
			/*
			 * Attribute not defined let's find a property
			 */
			foreach (var property in properties)
			{

				if (string.Compare(property.Name, propertyName, true) == 0)
					return Types.Convert<T>(property.GetValue(Instance));
			}
			/*
			 * Property must be defined so we're gonna throw exception
			 */
			throw new ForbiddenException($"{SR.AuthorizationPropertyNotFound} ({propertyName})");
		}
	}
}
