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

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public abstract class AuthorizationPolicyAttribute : Attribute
	{
		private enum EnumOperation
		{
			HigherThan = 1,
			AtLeast = 2,
		}

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

		public void Authorize(IMiddlewareContext context, object instance)
		{
			ReflectionExtensions.SetPropertyValue(Model, nameof(Model.Context), context);

			Model.AuthorizationTarget = instance;

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

				throw new ForbiddenException(sb.ToString());
			}

			Context.Grant();
		}

		protected virtual void OnAuthorize(List<PolicyAuthorizationResult> results)
		{

		}

		protected bool AuthorizeAny(object primaryKey, params object[] claims)
		{
			if (primaryKey == null || string.IsNullOrWhiteSpace(primaryKey.ToString()))
				throw new RuntimeException(GetType().Name, SR.ErrPrimaryKeyNull);

			foreach (var claim in claims)
			{
				if (claim == null || string.IsNullOrWhiteSpace(claim.ToString()))
					continue;

				var criteria = ResolveClaim(claim);

				if (Context.Services.Authorization.Authorize(criteria, primaryKey.ToString(), PermissionDescriptor))
					return true;
			}

			return false;
		}

		protected bool AuthorizeAll(object primaryKey, params object[] claims)
		{
			if (primaryKey == null || string.IsNullOrWhiteSpace(primaryKey.ToString()))
				throw new RuntimeException(GetType().Name, SR.ErrPrimaryKeyNull);

			foreach (var claim in claims)
			{
				if (claim == null || string.IsNullOrWhiteSpace(claim.ToString()))
					continue;

				var criteria = ResolveClaim(claim);

				if (!Context.Services.Authorization.Authorize(criteria, primaryKey.ToString(), PermissionDescriptor))
					return false;
			}

			return true;
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
					target.Authorize(Context, Model.AuthorizationTarget);
					return true;
				}
				catch (ForbiddenException)
				{

				}
			}

			return false;
		}

		protected static object[] AtLeast(object value)
		{
			return AddEnumValues(value, EnumOperation.AtLeast);
		}
		protected static object[] HigherThan(object value)
		{
			return AddEnumValues(value, EnumOperation.HigherThan);
		}

		private static object[] AddEnumValues(object value, EnumOperation operation)
		{
			if (!value.GetType().IsEnum)
				return null;

			var names = Enum.GetNames(value.GetType());
			var result = new List<object>();
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
	}
}
