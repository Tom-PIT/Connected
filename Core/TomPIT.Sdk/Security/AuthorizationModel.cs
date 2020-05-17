using System.Reflection;
using TomPIT.Annotations;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Security
{
	public abstract class AuthorizationModel : MiddlewareObject, IAuthorizationModel
	{
		public object Instance { get; set; }

		public T GetValueFromInstance<T>(string propertyName)
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
