using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Security;

public enum AuthorizationPolicyBehavior
{
	Mandatory = 1,
	Optional = 2
}

public enum AuthorizationMiddlewareStage
{
	Before = 1,
	After = 2,
	Result = 3
}

internal enum EnumOperation
{
	HigherThan = 1,
	AtLeast = 2,
}

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public abstract class AuthorizationPolicyAttribute : Attribute
	{
		private IAuthorizationModel _model = null;
		public AuthorizationPolicyBehavior Behavior { get; set; } = AuthorizationPolicyBehavior.Mandatory;
		public int Priority { get; set; }
		public string Method { get; set; }
		protected IMiddlewareContext Context => Model.Context;

		public AuthorizationMiddlewareStage MiddlewareStage { get; set; } = AuthorizationMiddlewareStage.Before;

		protected virtual string PermissionDescriptor => GetType().Name;

		protected T GetModel<T>() where T : IAuthorizationModel
		{
			if (!(Model is T))
				throw new RuntimeException($"{SR.ErrInvalidAuthorizationModel} (?{typeof(T).Name}:{Model.GetType().Name})");

			return (T)Model;
		}

		public void Revoke()
		{
			if (Context is IElevationContext elevation)
				elevation.State = ElevationContextState.Revoked;
		}

		private static bool IsFullControl(IMiddlewareContext context)
        {
			return context.Tenant.GetService<IAuthorizationService>().IsInRole(context.Services.Identity.User?.Token ?? default, "Full Control");
        }
		protected IAuthorizationModel Model
		{
			get
			{
				if (_model == null)
					_model = OnCreateModel();

				return _model;
			}
			private set { _model = value; }
		}

		protected abstract IAuthorizationModel OnCreateModel();

		protected T CreatePolicy<T>() where T : AuthorizationPolicyAttribute
		{
			var instance = TypeExtensions.CreateInstance<T>(typeof(T));

			ReflectionExtensions.SetPropertyValue(Model, nameof(Model.Context), Context);

			if (instance.Model != null)
				instance.Model.Proxy = Model.Proxy;

			return instance;
		}

		public bool TryAuthorize(IMiddlewareContext context, object instance)
		{
			try
			{
				Authorize(context, instance);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public void Authorize(IMiddlewareContext context, object instance)
		{
			ReflectionExtensions.SetPropertyValue(Model, nameof(Model.Context), context);

			Model.Proxy = instance;

			if(IsFullControl(context))
            {
				Grant(context);
				return;
			}

			var grantAttributes = ResolveGrantAttributes();

			if (AuthorizeGrant(grantAttributes, AuthorizationMiddlewareStage.Before))
				return;

			var messages = new List<PolicyAuthorizationResult>();

			OnAuthorize(messages);

			if (messages.Count > 0)
			{
				if (AuthorizeGrant(grantAttributes, AuthorizationMiddlewareStage.After))
					return;

				var sb = new StringBuilder();

				foreach (var message in messages)
					sb.AppendLine(message.Message);

				if (Model.Context.Services.Identity.IsAuthenticated)
					throw new ForbiddenException(sb.ToString());
				else
					throw new UnauthorizedException(sb.ToString());
			}

			Grant(context);
		}

		protected virtual void OnAuthorize(List<PolicyAuthorizationResult> results)
		{

		}

		protected bool AuthorizeAny(object primaryKey, Guid user, params object[] claims)
		{
			if (primaryKey == null || string.IsNullOrWhiteSpace(primaryKey.ToString()))
				throw new RuntimeException(GetType().Name, SR.ErrPrimaryKeyNull);

			foreach (var claim in claims)
			{
				if (claim == null || string.IsNullOrWhiteSpace(claim.ToString()))
					continue;

				var criteria = ResolveClaim(claim);

				if (Context.Services.Authorization.Authorize(criteria, primaryKey.ToString(), PermissionDescriptor, user))
					return true;
			}

			return false;

		}
		protected bool AuthorizeAny(object primaryKey, params object[] claims)
		{
			var user = Guid.Empty;

			if (Context.Services.Identity.IsAuthenticated)
				user = Context.Services.Identity.User.Token;

			return AuthorizeAny(primaryKey, user, claims);
		}

		protected bool AuthorizeAll(object primaryKey, Guid user, params object[] claims)
		{
			if (primaryKey == null || string.IsNullOrWhiteSpace(primaryKey.ToString()))
				throw new RuntimeException(GetType().Name, SR.ErrPrimaryKeyNull);

			foreach (var claim in claims)
			{
				if (claim == null || string.IsNullOrWhiteSpace(claim.ToString()))
					continue;

				var criteria = ResolveClaim(claim);

				if (!Context.Services.Authorization.Authorize(criteria, primaryKey.ToString(), PermissionDescriptor, user))
					return false;
			}

			return true;
		}

		protected bool AuthorizeAll(object primaryKey, params object[] claims)
		{
			var user = Guid.Empty;

			if (Context.Services.Identity.IsAuthenticated)
				user = Context.Services.Identity.User.Token;

			return AuthorizeAll(primaryKey, user, claims);
		}

		private string ResolveClaim(object value)
		{
			if (value == null)
				return null;

			var result = value.ToString();

			if (value.GetType().IsEnum)
				result = Enum.GetName(value.GetType(), value);

			if (string.IsNullOrWhiteSpace(result))
				result = value.ToString();

			return result;
		}

		private List<AuthorizationPolicyAttribute> ResolveGrantAttributes()
		{
			var result = new List<AuthorizationPolicyAttribute>();
			var attributes = GetType().GetCustomAttributes(true);

			foreach (var attribute in attributes)
			{
				if (attribute is AuthorizationPolicyAttribute grant)
					result.Add(grant);
			}

			return result;
		}

		private bool AuthorizeGrant(List<AuthorizationPolicyAttribute> attributes, AuthorizationMiddlewareStage middleware)
		{
			var targets = attributes.Where(f => f.MiddlewareStage == middleware).OrderBy(f => f.Priority);

			foreach (var target in targets)
			{
				try
				{
					target.Model = Model;
					target.Authorize(Context, Model.Proxy);
					return true;
				}
				catch (ForbiddenException)
				{

				}
			}

			return false;
		}

		protected static string[] AtLeast(object value)
		{
			return SelectEnumValues(value, EnumOperation.AtLeast);
		}
		protected static string[] HigherThan(object value)
		{
			return SelectEnumValues(value, EnumOperation.HigherThan);
		}

		internal static string[] SelectEnumValues(object value, EnumOperation operation)
		{
			if (!value.GetType().IsEnum)
				return null;

			var names = Enum.GetNames(value.GetType());
			var result = new List<string>();
			var underlyingType = Enum.GetUnderlyingType(value.GetType());
			var rawValue = Convert.ChangeType(value, underlyingType);
			var underlyingValue = Convert.ToDecimal(rawValue);

			foreach (var name in names)
			{
				var val = Convert.ChangeType(Enum.Parse(value.GetType(), name, false), underlyingType);

				switch (operation)
				{
					case EnumOperation.HigherThan:
						if (Convert.ToDecimal(val) > underlyingValue)
							result.Add(name);
						break;
					case EnumOperation.AtLeast:
						if (Convert.ToDecimal(val) >= underlyingValue)
							result.Add(name);
						break;
				}
			}

			return result.ToArray();
		}

		private void Grant(IMiddlewareContext context)
        {
			if (context is IElevationContext elevationContext)
				elevationContext.State = ElevationContextState.Granted;
		}

		public override string ToString()
		{
			return PermissionDescriptor;
		}
	}
}
