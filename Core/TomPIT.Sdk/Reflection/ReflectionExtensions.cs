using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using TomPIT.Annotations;

namespace TomPIT.Reflection
{
	public static class ReflectionExtensions
	{
		public static void SetPropertyValue(object instance, string propertyName, object value)
		{
			var property = instance.GetType().GetProperty(propertyName);

			if (property == null)
				return;

			if (!property.CanWrite)
			{
				if (property.DeclaringType == null)
					return;

				property = property.DeclaringType.GetProperty(propertyName);
			}

			if (property == null || property.SetMethod == null)
				return;

			property.SetMethod.Invoke(instance, new object[] { value });
		}


		public static PropertyInfo CacheKeyProperty(object instance)
		{
			return PropertyAttribute<CacheKeyAttribute>(instance);
		}

		public static string ResolveCacheKey(object instance)
		{
			var properties = instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			var key = new StringBuilder();

			foreach (var property in properties)
			{
				var att = property.FindAttribute<CacheKeyAttribute>();

				if (att == null)
					continue;

				var value = property.GetValue(instance);

				if (key.Length > 0)
					key.Append('/');

				if (value == null)
					continue;

				key.Append(Types.Convert<string>(value, CultureInfo.InvariantCulture));
			}

			return key.ToString();
		}

		public static PropertyInfo AuthorizationProperty(object instance)
		{
			return PropertyAttribute<AuthorizationPropertyAttribute>(instance);
		}
		public static PropertyInfo PropertyAttribute<T>(object instance) where T :Attribute
		{
			var props = Properties(instance, false);

			if (props == null || props.Length == 0)
				return null;

			foreach(var property in props)
			{
				if (property.FindAttribute<T>() != null)
					return property;
			}

			return null;
		}

		public static PropertyInfo[] Properties(object instance, bool writableOnly)
		{
			if( instance.GetType().GetProperties() is not PropertyInfo[] properties)
				return null;

			var temp = new List<PropertyInfo>();

			foreach (var i in properties)
			{
				var getMethod = i.GetGetMethod();
				var setMethod = i.GetSetMethod();

				if (writableOnly && setMethod == null)
					continue;

				if (getMethod == null)
					continue;

				if ((getMethod != null && getMethod.IsStatic) || (setMethod != null && setMethod.IsStatic))
					continue;

				if (setMethod != null && !setMethod.IsPublic)
					continue;

				temp.Add(i);
			}

			return temp.ToArray();
		}
	}
}