using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TomPIT.Annotations;
using TomPIT.Design;
using TomPIT.Dom;
using TomPIT.Ide;
using TomPIT.Services;

namespace TomPIT
{
	public static class DomQuery
	{
		public static T Closest<T>(IDomElement element)
		{
			if (element == null)
				return default(T);

			if (element is T || element.GetType().IsAssignableFrom(typeof(T)))
				return (T)element;

			return Closest<T>(element.Parent);
		}

		public static T Key<T>(object instance, T defaultValue)
		{
			if (Types.TryConvert(Key(instance), out T r))
				return r;

			return defaultValue;
		}

		public static object Key(object instance)
		{
			if (instance == null)
				return null;

			var props = Properties(instance, false, false);

			foreach (var i in props)
			{
				var att = i.FindAttribute<KeyPropertyAttribute>();

				if (att != null)
					return i.GetValue(instance);
			}

			if (instance != null)
			{
				var pi = instance.GetType().GetProperty("Id");

				if (pi != null)
					return pi.GetValue(instance);

				return instance.GetType().ShortName();
			}

			return null;
		}

		public static T FindAttribute<T>(this IDomElement node) where T : Attribute
		{
			if (node.Property != null)
				return node.Property.FindAttribute<T>();
			else
				return node.Component.GetType().FindAttribute<T>();
		}

		public static T AttributeLookup<T>(this PropertyInfo property) where T : Attribute
		{
			var r = property.FindAttribute<T>();

			if (r != null)
				return r;

			return property.GetType().FindAttribute<T>();
		}

		public static PropertyInfo Property(IDomElement element, string propertyName, string attribute, out object component)
		{
			var property = Property(element, propertyName, out component);

			if (property == null)
				return null;

			if (string.IsNullOrWhiteSpace(attribute))
				return property;

			component = property.GetValue(component);

			if (component == null)
				return null;

			return component.GetType().GetProperty(attribute);
		}

		public static PropertyInfo Property(IDomElement element, string propertyName, out object component)
		{
			IPropertySource source = null;

			source = element as IPropertySource;

			if (source != null)
			{
				var instances = source.PropertySources;

				if (instances != null)
				{
					foreach (var i in instances)
					{
						var pi = i.GetType().GetProperty(propertyName);

						if (pi != null)
						{
							component = i;
							return pi;
						}
					}
				}

				component = null;

				return null;
			}
			else
			{
				component = element.Value;
				return element.Value.GetType().GetProperty(propertyName);
			}
		}

		public static List<IItemDescriptor> Items(IProperty property)
		{
			var pi = Property(property.Element, property.Name, out object instance);

			return Items(property.Element, pi);
		}

		public static List<IItemDescriptor> Items(IDomElement element)
		{
			return Items(element, element.Property);
		}

		private static List<IItemDescriptor> Items(IDomElement element, PropertyInfo property)
		{
			var att = property.FindAttribute<ItemsAttribute>();

			if (att == null)
				return ItemsByEnum(element, property);

			return ItemsByAttribute(element, property, att);
		}

		private static List<IItemDescriptor> ItemsByAttribute(IDomElement element, PropertyInfo property, ItemsAttribute att)
		{
			var cp = att.Type == null
				? Types.GetType(att.TypeName).CreateInstance<IItemsProvider>()
				: att.Type.CreateInstance<IItemsProvider>();

			if (cp == null)
				return null;

			var suppressedAtt = property.FindAttribute<SuppressPropertyValues>();
			var items = cp.QueryDescriptors(element);

			var r = new List<IItemDescriptor>();

			foreach (var i in items)
				r.Add(new ItemDescriptor
				{
					Text = i.Text,
					Id = i.Id,
					Value = i.Value,
					Type = i.Type
				});

			if (suppressedAtt == null)
				return r;

			r.Clear();

			foreach (var i in items)
			{
				if (IsSuppressed(suppressedAtt, i.Value))
					continue;

				r.Add(new ItemDescriptor(i.Text, i.Value));
			}

			return r;
		}

		public static bool IsSuppressed(SuppressPropertyValues att, object value)
		{
			if (att == null || att.Values == null || att.Values.Length == 0)
				return false;

			foreach (var i in att.Values)
			{
				if (System.Collections.Comparer.Default.Compare(i, value) == 0)
					return true;
			}

			return false;
		}

		private static List<IItemDescriptor> ItemsByEnum(IDomElement element, PropertyInfo property)
		{
			var suppressedAtt = property.FindAttribute<SuppressPropertyValues>();

			if (property.PropertyType.IsEnum)
			{
				var r = new List<IItemDescriptor>();
				var names = Enum.GetNames(property.PropertyType);

				foreach (var i in names)
				{
					if (IsSuppressed(suppressedAtt, Enum.Parse(property.PropertyType, i)))
						continue;

					r.Add(new ItemDescriptor(i, Enum.Parse(property.PropertyType, i)));
				}

				return r.OrderBy(f => f.Text).ToList();
			}

			return null;
		}

		public static bool ImplementsInterface<T>(Type type)
		{
			if (type == typeof(T))
				return true;

			var interfaces = type.GetInterfaces();

			if (interfaces == null || interfaces.Length == 0)
				return false;

			return interfaces.FirstOrDefault(f => f == typeof(T)) != null;
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

				if (env == null || env.Visibility == EnvironmentMode.Design)
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

				if (env != null && env.Visibility == EnvironmentMode.Runtime)
					r.Add(i);
			}

			return r.ToArray();
		}

		public static string Path(IDomElement element)
		{
			var items = new List<string>();
			var sb = new StringBuilder();
			var current = element;

			while (current != null)
			{
				items.Add(current.Id);

				current = current.Parent;
			}

			for (var i = items.Count - 1; i >= 0; i--)
				sb.AppendFormat("{0}/", items[i]);

			return sb.ToString().TrimEnd('/');
		}

		public static IDomDesigner CreateDesigner(IEnvironment environment, IDomElement element, DomDesignerAttribute attribute)
		{
			if (attribute == null)
				return null;

			var type = attribute.Type ?? Types.GetType(attribute.TypeName);
			var instance = type.CreateInstance<IDomDesigner>(new object[] { environment, element });

			if (instance == null)
				throw new TomPITException(string.Format("{0} ({1})", SR.ErrCannotCreateInstance, type.Name));

			return instance;
		}
	}
}
