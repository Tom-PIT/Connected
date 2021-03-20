using System;
using System.Collections.Generic;
using System.Reflection;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Runtime;

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

		public static PropertyInfo AuthorizationProperty(object instance)
		{
			return PropertyAttribute<AuthorizationPropertyAttribute>(instance);
		}
		public static PropertyInfo PropertyAttribute<T>(object instance) where T :Attribute
		{
			var props = Properties(instance, false, false);

			if (props == null || props.Length == 0)
				return null;

			foreach(var property in props)
			{
				if (property.FindAttribute<T>() != null)
					return property;
			}

			return null;
		}

		public static PropertyInfo[] Properties(object instance, bool writableOnly, bool filterByEnvironment)
		{
			var mode = Shell.GetService<IRuntimeService>().Mode;
			PropertyInfo[] properties = null;

			properties = instance.GetType().GetProperties();

			if (properties == null)
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

			properties = temp.ToArray();

			if (filterByEnvironment)
			{
				switch (mode)
				{
					case EnvironmentMode.Design:
						return FilterDesignProperties(properties);
					case EnvironmentMode.Runtime:
						return FilterRuntimeProperties(properties);
					default:
						throw new NotSupportedException();
				}
			}

			return properties;
		}

		private static PropertyInfo[] FilterDesignProperties(PropertyInfo[] properties)
		{
			var r = new List<PropertyInfo>();

			foreach (var i in properties)
			{
				var env = i.FindAttribute<EnvironmentVisibilityAttribute>();

				if (env == null || ((env.Visibility & EnvironmentMode.Design) == EnvironmentMode.Design))
					r.Add(i);
			}

			return r.ToArray();
		}

		private static PropertyInfo[] FilterRuntimeProperties(PropertyInfo[] properties)
		{
			var r = new List<PropertyInfo>();

			foreach (var i in properties)
			{
				var env = i.FindAttribute<EnvironmentVisibilityAttribute>();

				if (env != null && ((env.Visibility & EnvironmentMode.Runtime) == EnvironmentMode.Runtime))
					r.Add(i);
			}

			return r.ToArray();
		}
	}
}