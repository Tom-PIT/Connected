using System.Collections;
using System.Reflection;
using TomPIT.Annotations;
using TomPIT.Exceptions;
using TomPIT.Reflection;

namespace TomPIT.Middleware.Interop
{
	public abstract class MiddlewareProxy : MiddlewareObject, IMiddlewareProxy
	{
		public bool ContainsValue<T>(string propertyName)
		{
			try
			{
				var value = GetValue<T>(propertyName);

				if (value == null)
					return true;

				if (value.GetType().IsCollection() && ((IEnumerable)value).IsEmpty())
					return false;

				return Types.Compare(value, default);
			}
			catch
			{
				return false;
			}

		}

		public T GetValue<T>(string propertyName)
		{
			var property = GetProxyProperty(propertyName);

			/*
			 * Property must be defined so we're gonna throw exception
			 */
			if (property == null)
				throw new ForbiddenException($"{SR.ProxyPropertyNotFound} ({propertyName})");

			return Types.Convert<T>(property.GetValue(ProxyTarget));
		}

		public bool IsDefined(string propertyName)
		{
			return GetProxyProperty(propertyName) != null;
		}

		private PropertyInfo GetProxyProperty(string propertyName)
		{
			var properties = ProxyTarget.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
			/*
			 * First check if attribute is defined on any of the properties. 
			 * This have a higher priority than a property name
			 */
			foreach (var property in properties)
			{
				var attribute = property.FindAttribute<ProxyPropertyAttribute>();

				if (attribute != null && string.Compare(attribute.PropertyName, propertyName, true) == 0)
					return property;

				if (TryAuthorizatonPropertyAttribute(property, propertyName))
					return property;
			}
			/*
			 * Attribute not defined let's find a property
			 */
			foreach (var property in properties)
			{

				if (string.Compare(property.Name, propertyName, true) == 0)
					return property;
			}

			return null;
		}

		/// <summary>
		/// This is temporary because AuthorizationPropertyAttribute is obsolete.
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		private bool TryAuthorizatonPropertyAttribute(PropertyInfo property, string propertyName)
		{
			var attribute = property.FindAttribute<AuthorizationPropertyAttribute>();

			if (attribute != null && string.Compare(attribute.PropertyName, propertyName, true) == 0)
				return true;

			return false;
		}

		protected virtual object ProxyTarget => this;
	}
}
