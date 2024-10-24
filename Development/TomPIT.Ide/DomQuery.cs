﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TomPIT.Annotations.Design;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Designers;
using TomPIT.Design.Ide.Dom;
using TomPIT.Design.Ide.Properties;
using TomPIT.Design.Ide.Selection;
using TomPIT.Exceptions;
using TomPIT.Ide;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Environment.Providers;
using TomPIT.Reflection;

namespace TomPIT.Ide
{
	public static class DomQuery
	{
		public static IDomElement Root(this IDomElement element)
		{
			var current = element;

			while (current != null)
			{
				if (current.Parent == null)
					return current;

				current = current.Parent;
			}

			return null;
		}

		public static T Closest<T>(this IDomElement element)
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

			var props = ReflectionExtensions.Properties(instance, false);

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

		public static List<T> AttributesLookup<T>(this PropertyInfo property) where T : Attribute
		{
			var r = property.FindAttributes<T>();

			if (r != null)
				return r;

			return property.GetType().FindAttributes<T>();
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

			if (element.Designer != null && element.Designer is IDesignerSelectionProvider sp)
			{
				component = sp.Value;

				if (sp.Value == null)
					return null;

				return sp.Value.GetType().GetProperty(propertyName);
			}

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
			var pi = property.PropertyInfo == null
				? Property(property.Element, property.Name, out object instance)
				: property.PropertyInfo;

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
				? Reflection.TypeExtensions.GetType(att.TypeName).CreateInstance<IItemsProvider>()
				: att.Type.CreateInstance<IItemsProvider>();

			if (cp == null)
				return null;

			var suppressedAtt = property.FindAttribute<SuppressPropertyValues>();
			var items = cp.QueryDescriptors(new ItemsDescriptorArgs(element, property));

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

					r.Add(new ItemDescriptor(i, i));
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

		public static IDomDesigner CreateDesigner(IDomElement element, DomDesignerAttribute attribute)
		{
			if (attribute == null)
				return null;

			var type = attribute.Type ?? Reflection.TypeExtensions.GetType(attribute.TypeName);
			var instance = type.CreateInstance<IDomDesigner>(new object[] { element });

			if (instance == null)
				throw new TomPITException(string.Format("{0} ({1})", SR.ErrCannotCreateInstance, type.Name));

			return instance;
		}

		public static void SetId(this ISelectionProvider selection, string id)
		{
			if (selection is SelectionProvider s)
				s.Id = id;
		}

		public static Tuple<PropertyInfo, object> ResolveProperty(object instance, string propertyPath)
		{
			var tokens = propertyPath.Split('.');

			var pi = instance.GetType().GetProperty(tokens[0]);

			if (tokens.Length == 1)
				return new Tuple<PropertyInfo, object>(pi, instance);

			return ResolveProperty(pi.GetValue(instance), propertyPath.Substring(propertyPath.IndexOf('.')));
		}
	}
}
