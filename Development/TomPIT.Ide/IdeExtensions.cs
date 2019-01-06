using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Designers;
using TomPIT.Dom;
using TomPIT.Exceptions;
using TomPIT.Ide;

namespace TomPIT
{
	public static class IdeExtensions
	{
		public static void RemoveWhere<T>(this ListItems<T> source, Func<T, bool> predicate) where T : class
		{
			var where = source.Where(predicate);

			if (where.Count() == 0)
				return;

			var arr = where.ToArray();

			for (int i = 0; i < arr.Length; i++)
				source.Remove(arr[i]);
		}

		public static bool ChildrenBrowsable(this PropertyInfo pi)
		{
			if (pi == null)
				return true;

			var browsable = pi.FindAttribute<ChildrenBrowsableAttribute>();

			return browsable == null || browsable.Browsable;
		}

		public static bool IsSuppressed(this Type type, PropertyInfo pi)
		{
			var suppresses = type.FindAttribute<SuppressPropertiesAttribute>();
			string[] tokens = null;

			if (suppresses != null && !string.IsNullOrWhiteSpace(suppresses.Properties))
			{
				tokens = suppresses.Properties.Split(',');

				if (tokens.Contains(pi.Name))
					return true;
			}

			return false;
		}

		public static bool Commit(this IEnvironment environment, object component, string property, string attribute)
		{
			if (environment.Selection.Transaction == null)
				return false;

			var current = environment.Selection.Transaction;
			var r = false;

			while (!r)
			{
				if (current == null)
					break;

				r = current.Commit(component, property, attribute);

				if (r)
					break;

				current = ParentHandler(current);
			}

			return r;
		}

		private static ITransactionHandler ParentHandler(this ITransactionHandler handler)
		{
			var current = handler.Element.Parent;

			while (current != null)
			{
				if (current.Transaction != null)
					return current.Transaction;

				current = current.Parent;
			}

			return null;
		}

		public static Guid MicroService(this IDomElement element)
		{
			if (element == null)
				return Guid.Empty;

			if (element is IMicroServiceScope)
				return ((IMicroServiceScope)element).MicroService.Token;

			return MicroService(element.Parent);
		}

		public static string SelectedPath(this IEnvironment environment)
		{
			if (environment.Dom is DomBase)
				return ((DomBase)environment.Dom).SelectedPath;

			return null;
		}

		public static IDomElement Selected(this IEnvironment environment)
		{
			if (environment.Dom is DomBase)
				return ((DomBase)environment.Dom).Selected;

			return null;
		}

		public static IDomElement GetDomElement(this IComponent component, IEnvironment environment, IDomElement parent)
		{
			var type = Types.GetType(component.Type);

			if (type == null)
				throw new RuntimeException(SR.ErrCannotCreateComponentInstance);

			var att = type.FindAttribute<DomElementAttribute>();

			if (att == null)
				return new ComponentElement(environment, parent, component);
			else
			{
				var t = att.Type ?? Types.GetType(att.TypeName);
				var constructors = t.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

				foreach (var i in constructors)
				{
					var parameters = i.GetParameters();

					if (parameters.Count() == 3)
						return t.CreateInstance<IDomElement>(new object[] { environment, parent, component });
				}

				return null;
			}
		}

		public static IDomDesigner SystemDesigner(this IDomElement sender, object instance)
		{
			if (instance == null)
				return null;

			return SystemDesigner(sender, instance.GetType());
		}

		public static IDomDesigner SystemDesigner(this IDomElement sender, PropertyInfo property)
		{
			if (property == null)
				return null;

			return SystemDesigner(sender, property.PropertyType);
		}

		private static IDomDesigner SystemDesigner(this IDomElement sender, Type type)
		{
			if (type == null)
				return null;

			if (type.IsCollection())
				return new CollectionDesigner<IDomElement>(sender.Environment, sender);
			else if (type.IsText())
				return new TextDesigner(sender.Environment, sender);
			else
			{
				/*
				 * TODO: try to create instance provider
				 */
				return null;
			}
		}

		public static string ResolvePath(this IEnvironment environment, Guid component, Guid element)
		{
			var c = environment.Context.Connection().GetService<IComponentService>().SelectComponent(component);

			if (c == null)
				throw new TomPITException(SR.ErrComponentNotFound);

			var config = environment.Context.Connection().GetService<IComponentService>().SelectConfiguration(c.Token);

			if (config == null)
				throw new TomPITException(string.Format("{0} ({1})", SR.ErrCannotFindConfiguration, c.Name));

			var ms = environment.Context.Connection().GetService<IMicroServiceService>().Select(c.MicroService);
			var template = environment.Context.Connection().GetService<IMicroServiceTemplateService>().Select(ms.Template);

			var root = template.QueryDomRoot(environment);

			if (root == null)
				return null;

			var target = Traverse(element, root);

			return target != null
				? DomQuery.Path(target)
				: null;
		}

		private static IDomElement Traverse(Guid element, ListItems<IDomElement> elements)
		{
			if (elements == null || elements.Count == 0)
				return null;

			var r = elements.FirstOrDefault(f => string.Compare(f.Id, element.ToString(), true) == 0);

			if (r != null)
				return r;

			foreach (var i in elements)
			{
				i.LoadChildren();

				r = Traverse(element, i.Items);

				if (r != null)
					return r;
			}

			return null;
		}

		public static List<T> Children<T>(this IConfiguration configuration) where T : IElement
		{
			var r = new List<T>();

			var props = configuration.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
			var refs = new List<object>();

			foreach (var i in props)
				Children(configuration, i, r, refs);

			return r;
		}

		private static void Children<T>(object instance, List<T> items, List<object> refs) where T : IElement
		{
			var props = instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (var i in props)
				Children(instance, i, items, refs);
		}

		private static void Children<T>(object configuration, PropertyInfo property, List<T> items, List<object> refs) where T : IElement
		{
			if (property.IsIndexer() || property.IsPrimitive())
				return;

			var value = property.GetValue(configuration);

			if (value == null)
				return;

			if (refs.Contains(value))
				return;

			refs.Add(value);

			if (property.IsCollection())
			{
				if (!(value is IEnumerable en))
					return;

				var enm = en.GetEnumerator();

				while (enm.MoveNext())
				{
					if (enm.Current == null)
						continue;

					if (enm is T)
						items.Add((T)enm);

					Children(enm.Current, items, refs);
				}
			}
			else if (!property.IsPrimitive())
			{
				if (value is T)
					items.Add((T)value);

				Children(value, items, refs);
			}
		}
	}
}
