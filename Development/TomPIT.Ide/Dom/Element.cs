using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations;
using TomPIT.Design;
using TomPIT.Ide;

namespace TomPIT.Dom
{
	public abstract class Element : DomElement
	{
		public Element(IEnvironment environment, IDomElement parent) : base(environment, parent)
		{
		}

		protected override List<IDomElement> Children(object instance)
		{
			var r = new List<IDomElement>();

			if (instance == null)
				return r;

			if (instance.GetType().IsCollection())
				FillCollectionProperties(instance, r);
			else
			{
				FillObjectProperties(instance, r);

				r = r.OrderBy(f => f.Title).ToList();
			}

			return r;
		}

		private void FillCollectionProperties(object instance, List<IDomElement> properties)
		{
			var en = instance as IEnumerable;

			if (en == null)
				return;

			var enm = en.GetEnumerator();
			int idx = 0;

			while (enm.MoveNext())
			{
				var element = CreatePropertyElement(this, enm.Current, null, idx);

				if (element != null)
					properties.Add(element);
				else
					properties.Add(new ReflectionElement(Environment, this, enm.Current, null, idx)
					{
						SortChildren = false
					});

				idx++;
			}
		}

		public override IDomDesigner PropertyDesigner(string propertyName)
		{
			IDomDesigner r = null;
			var propertySource = this as IPropertySource;

			if (propertySource == null)
				r = PropertyDesigner(Component, propertyName);
			else
			{
				foreach (var i in propertySource.PropertySources)
				{
					r = PropertyDesigner(i, propertyName);

					if (r != null)
						break;
				}
			}

			if (r == null)
				throw IdeException.NoPropertyDesigner(this, IdeEvents.DesignerSection, propertyName);

			return r;
		}

		private void FillObjectProperties(object instance, List<IDomElement> properties)
		{
			var props = DomQuery.Properties(instance, false, true);
			var filtered = new List<PropertyInfo>();
			var suppressed = instance.GetType().FindAttribute<SuppressPropertiesAttribute>();
			string[] suppressedProps = null;

			if (suppressed != null)
				suppressedProps = suppressed.Properties.Split(',');

			foreach (var i in props)
			{
				if (!i.IsBrowsable())
					continue;
				else if (!i.ChildrenBrowsable())
					continue;

				if (suppressedProps != null && suppressedProps.Contains(i.Name))
					continue;

				filtered.Add(i);
			}

			var list = new List<PropertyInfo>();

			foreach (var i in filtered)
			{
				if (i.IsDesignable())
					list.Add(i);
			}

			foreach (var i in list)
			{
				var element = CreatePropertyElement(this, instance, i, 0);

				if (element != null)
					properties.Add(element);
				else
					properties.Add(new ReflectionElement(Environment, this, instance, i, 0));
			}
		}

		private IDomElement CreatePropertyElement(IDomElement parent, object instance, PropertyInfo pi, int index)
		{
			var args = new ReflectorCreateArgs
			{
				Environment = Environment,
				Instance = instance,
				Property = pi,
				Parent = this,
				Index = index
			};

			if (pi != null)
			{
				var element = pi.AttributeLookup<DomElementAttribute>();

				if (element != null)
				{
					return element.Type == null
						? Type.GetType(element.TypeName).CreateInstance<DomElement>(new object[] { args })
						: element.Type.CreateInstance<DomElement>(new object[] { args });
				}
			}

			if (instance != null)
			{
				var element = instance.GetType().FindAttribute<DomElementAttribute>();

				if (element != null)
				{
					return element.Type == null
						? Type.GetType(element.TypeName).CreateInstance<DomElement>(new object[] { args })
						: element.Type.CreateInstance<DomElement>(new object[] { args });
				}
			}

			return null;
		}

		private IDomDesigner CreatePropertyDesignerInstance(object component, string propertyName, DomDesignerAttribute designer)
		{
			var e = new PropertyElement(Environment, this, component, propertyName);

			var type = designer.Type ?? Type.GetType(designer.TypeName);

			var instance = type.CreateInstance<IDomDesigner>(new object[] { Environment, e });

			if (instance == null)
				throw IdeException.InvalidPropertyDesigner(this, IdeEvents.DesignerSection, propertyName, type.Name);

			return instance;
		}

		private IDomDesigner PropertyDesigner(object instance, string propertyName)
		{
			if (instance == null)
				return null;

			var pi = instance.GetType().GetProperty(propertyName);

			if (pi == null)
				return null;

			var att = pi.AttributeLookup<DomDesignerAttribute>();

			if (att != null)
				return CreatePropertyDesignerInstance(instance, propertyName, att);
			else
			{
				att = pi.PropertyType.FindAttribute<DomDesignerAttribute>();

				if (att != null)
					return CreatePropertyDesignerInstance(instance, propertyName, att);
				else
				{
					var value = pi.GetValue(instance);

					if (value != null)
					{
						att = value.GetType().FindAttribute<DomDesignerAttribute>();

						if (att != null)
							return CreatePropertyDesignerInstance(instance, propertyName, att);
					}
				}
			}

			return null;
		}

		protected IDesignerService DesignerService { get { return Connection.GetService<IDesignerService>(); } }
	}
}