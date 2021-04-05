using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations.Design;
using TomPIT.Connectivity;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Designers;
using TomPIT.Design.Ide.Dom;
using TomPIT.Design.Ide.Properties;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Dom.ComponentModel;
using TomPIT.Reflection;
using TomPIT.Runtime;

namespace TomPIT.Ide.Dom
{
	public abstract class DomElement : IDomElement
	{
		private IDomElementBehavior _behavior = null;
		private bool _sortChildren = true;
		private List<IVerb> _verbs = null;
		private IDomDesigner _designer = null;
		private bool _designerResolved = false;
		private IDomElementMetaData _metaData = null;

		protected DomElement(IEnvironment environment, IDomElement parent)
		{
			Parent = parent;
			Environment = environment;
		}

		protected DomElement(IDomElement parent)
		{
			Parent = parent;
			Environment = parent?.Environment;
		}

		public IDomElement Parent { get; private set; }
		public IEnvironment Environment { get; private set; }
		private List<IDomElement> _items = null;

		public string Id { get; set; }

		public string Title { get; set; }

		public virtual bool HasChildren { get; }
		public virtual int ChildrenCount { get; }

		public virtual ITransactionHandler Transaction { get; } = null;
		public string Glyph { get; set; }

		public virtual void LoadChildren(string id)
		{

		}

		public virtual void LoadChildren()
		{

		}

		public List<IDomElement> Items
		{
			get
			{
				if (_items == null)
					_items = new List<IDomElement>();

				return _items;
			}
		}

		public IDomElementBehavior Behavior
		{
			get
			{
				if (_behavior == null)
					_behavior = new Behavior(Environment);

				return _behavior;
			}
		}

		public virtual IDomDesigner Designer
		{
			get
			{
				if (_designer == null && !_designerResolved)
				{
					_designerResolved = true;

					if (Component == null)
						return _designer;

					var designers = Component.GetType().FindAttributes<DomDesignerAttribute>();

					if (designers == null || designers.Count == 0)
						return _designer;

					var mode = Shell.GetService<IRuntimeService>().Mode;

					foreach (var i in designers)
					{
						if ((i.Mode & mode) == mode)
						{
							_designer = DomQuery.CreateDesigner(this, i);

							break;
						}
					}
				}

				return _designer;
			}
		}

		public virtual object Component { get; } = null;
		public virtual PropertyInfo Property { get; } = null;

		public object Value
		{
			get
			{
				if (Property == null)
					return Component;

				return Property.GetValue(Component);
			}
		}

		protected T GetComponent<T>() { return (T)Component; }
		protected virtual List<IDomElement> Children(object instance)
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
		public virtual bool SortChildren { get { return _sortChildren; } set { _sortChildren = value; } }

		protected ITenant Tenant { get { return Environment.Context.Tenant; } }
		protected bool IsDesignTime { get { return (Shell.GetService<IRuntimeService>().Mode & EnvironmentMode.Design) == EnvironmentMode.Design; } }

		public virtual List<IVerb> Verbs
		{
			get
			{
				if (_verbs == null)
					_verbs = new List<IVerb>();

				return _verbs;
			}
		}

		public IDomElementMetaData MetaData
		{
			get
			{
				if (_metaData == null)
					_metaData = new ElementMetaData();

				return _metaData;
			}
		}

		private void FillCollectionProperties(object instance, List<IDomElement> properties)
		{
			var en = instance as IEnumerable;

			if (en == null)
				return;

			var enm = en.GetEnumerator();
			var idx = 0;
			var sorted = new List<IDomElement>();

			while (enm.MoveNext())
			{
				var element = CreatePropertyElement(enm.Current, null, idx);

				if (element != null)
					sorted.Add(element);
				else
					sorted.Add(new ReflectionElement(this, enm.Current, null, idx)
					{
						SortChildren = false
					});

				idx++;
			}

			if (SortChildren)
				sorted = sorted.OrderBy(f => f.Title).ToList();

			properties.AddRange(sorted);
		}

		public virtual IDomDesigner PropertyDesigner(string propertyName)
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
			var props = ReflectionExtensions.Properties(instance, false);
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
				var element = CreatePropertyElement(instance, i, 0);

				if (element != null)
					properties.Add(element);
				else
					properties.Add(new ReflectionElement(this, instance, i, 0));
			}
		}

		private IDomElement CreatePropertyElement(object instance, PropertyInfo pi, int index)
		{
			return new DomElementActivator(this, instance, pi, index).CreateInstance();
		}

		private IDomDesigner CreatePropertyDesignerInstance(object component, string propertyName, DomDesignerAttribute designer)
		{
			var e = new PropertyElement(this, component, propertyName);

			var type = designer.Type ?? Type.GetType(designer.TypeName);

			var instance = type.CreateInstance<IDomDesigner>(new object[] { e });

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

			var att = pi.ResolveDesigner();

			if (att != null)
				return CreatePropertyDesignerInstance(instance, propertyName, att);
			else
			{
				att = pi.PropertyType.ResolveDesigner();

				if (att != null)
					return CreatePropertyDesignerInstance(instance, propertyName, att);
				else
				{
					var value = pi.GetValue(instance);

					if (value != null)
					{
						att = value.GetType().ResolveDesigner();

						if (att != null)
							return CreatePropertyDesignerInstance(instance, propertyName, att);
					}
				}
			}

			return null;
		}

		protected IDesignerService DesignerService { get { return Tenant.GetService<IDesignerService>(); } }

		protected string WithFileExtension(string value)
		{
			object target = Property;

			if (target == null)
				target = Component;

			if (target == null)
				return value;

			var extension = target.GetType().FindAttribute<FileNameExtensionAttribute>();

			if (extension != null)
				return $"{value}.{extension.Extension}";

			var syntax = target.GetType().FindAttribute<SyntaxAttribute>();

			if (syntax == null)
				return value;

			if (string.Compare(syntax.Syntax, SyntaxAttribute.CSharp, true) == 0)
				return $"{value}.csx";
			else if (string.Compare(syntax.Syntax, SyntaxAttribute.Css, true) == 0)
				return $"{value}.css";
			else if (string.Compare(syntax.Syntax, SyntaxAttribute.Javascript, true) == 0)
				return $"{value}.jsm";
			else if (string.Compare(syntax.Syntax, SyntaxAttribute.Json, true) == 0)
				return $"{value}.json";
			else if (string.Compare(syntax.Syntax, SyntaxAttribute.Less, true) == 0)
				return $"{value}.less";
			else if (string.Compare(syntax.Syntax, SyntaxAttribute.Razor, true) == 0)
				return $"{value}.cshtml";
			else if (string.Compare(syntax.Syntax, SyntaxAttribute.Sql, true) == 0)
				return $"{value}.sql";
			else
				return value;
		}
	}
}
