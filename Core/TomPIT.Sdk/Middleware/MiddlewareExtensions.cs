using System;
using System.Collections.Generic;
using System.Reflection;
using TomPIT.Annotations;
using TomPIT.Connectivity;
using TomPIT.Exceptions;
using TomPIT.Middleware.Services;
using TomPIT.Reflection;
using TomPIT.Serialization;

namespace TomPIT.Middleware
{
	public static class MiddlewareExtensions
	{
		public static ServerUrl CreateUrl(this ITenant tenant, string controller, string action)
		{
			return ServerUrl.Create(tenant.Url, controller, action);
		}

		public static T WithContext<T>(this T operation, IMiddlewareContext context) where T : IMiddlewareOperation
		{
			var property = operation.GetType().GetProperty(nameof(IMiddlewareObject.Context));

			if (property is not null && property.CanWrite)
				property.SetValue(operation, context);

			return operation;
		}

		internal static void SetContext(this IMiddlewareObject target, IMiddlewareContext context)
		{
			ReflectionExtensions.SetPropertyValue(target, nameof(target.Context), context);
		}

		public static void Impersonate(this IMiddlewareContext context, string user)
		{
			var u = context.Services.Identity.GetUser(user);

			if (context.Services.Identity is MiddlewareIdentityService mc)
				mc.ImpersonatedUser = u?.Token.ToString();
		}

		public static void RevokeImpersonation(this IMiddlewareContext context)
		{
			if (context.Services.Identity is MiddlewareIdentityService mc)
				mc.ImpersonatedUser = null;
		}

		public static T Patch<T>(this T component, Dictionary<string, object> properties) where T : IMiddlewareComponent
		{
			return Patch(component, properties, null);
		}
		public static T Patch<T>(this T component, Dictionary<string, object> properties, object initializer) where T : IMiddlewareComponent
		{
			if (initializer != null)
				Serializer.Populate(initializer, component);

			if (properties == null || properties.Count == 0)
				return component;

			foreach (var property in properties)
			{
				var reflected = component.GetType().GetProperty(property.Key, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);

				if (reflected == null)
					throw new RuntimeException($"{SR.ErrPropertyNotFound} ({property.Key})");

				if (!reflected.CanWrite)
					throw new RuntimeException($"SR.ErrPropertyReadOnly ({property.Key})");

				if (reflected.FindAttribute<PatchReadOnlyAttribute>() != null)
					throw new RuntimeException($"{SR.ErrPatchForbidden} ({property.Key})");

				var value = Types.Convert(property.Value, reflected.PropertyType);

				reflected.SetValue(component, value);
			}

			return component;
		}

		public static void LogError(this TomPITException exception, string category)
		{
			if (exception.Logged)
				return;

			using var ctx = new MiddlewareContext();

			LogError(exception, ctx, category);
		}

		public static void LogError(this TomPITException exception, IMiddlewareContext context, string category)
		{
			if (exception.Logged)
				return;

			exception.Logged = true;

			var source = string.IsNullOrWhiteSpace(exception.ScriptPath) ? exception.Source : exception.ScriptPath;

			context.Services.Diagnostic.Error(source, exception.ToString(), category);
		}

		public static void LogWarning(this MiddlewareValidationException exception, string category)
		{
			if (exception.Logged)
				return;

			using var ctx = new MiddlewareContext();

			LogWarning(exception, ctx, category);
		}

		public static void LogWarning(this MiddlewareValidationException exception, IMiddlewareContext context, string category)
		{
			if (exception.Logged)
				return;

			exception.Logged = true;

			context.Services.Diagnostic.Warning(exception.Source, exception.ToString(), category);
		}

		public static List<Type> ResolveImplementedMiddleware(Type type)
		{
			var result = new List<Type>();
			var interfaces = type.GetInterfaces();

			foreach (var i in interfaces)
			{
				if (typeof(IMiddleware).FullName is not string fullName)
					continue;

				if (i.GetInterface(fullName) is not null)
					result.Add(i);
			}

			return result;
		}

	}
}
