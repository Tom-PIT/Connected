using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.Ide.ComponentModel;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Properties;
using TomPIT.Reflection;

namespace TomPIT.Ide.Dom.ComponentModel
{
	public class ReflectionElement : TransactionElement, IPropertySource
	{
		private List<IDomElement> _props = null;
		private IDomDesigner _designer = null;
		private List<object> _propertySources = null;

		public ReflectionElement(ReflectorCreateArgs e) : this(e.Parent, e.Instance, e.Property, e.Index)
		{

		}

		public ReflectionElement(IDomElement parent, object instance) : this(parent, instance, null, 0)
		{
		}

		public ReflectionElement(IDomElement parent, object instance, PropertyInfo property, int index) : base(parent)
		{
			Instance = instance;
			Component = instance;
			Property = property;
			Index = index;

			Id = DomQuery.Key(Value, string.Empty);

			Title = WithFileExtension(property == null ? instance.ToString() : property.Name);

			Initialize();

			if (string.IsNullOrWhiteSpace(Glyph))
			{
				if (IsCollection)
					Glyph = "fal fa-bars";
				else
					Glyph = "fal fa-wrench";
			}

			if (property != null && IsCollection)
			{
				var vd = property.FindAttribute<CollectionDesignerAttribute>();

				if (vd != null && !vd.Sort)
					SortChildren = false;
			}
			else
				SortChildren = Value == null || !Value.GetType().IsCollection();
		}

		/// <summary>
		/// This property needs to exist because of inheritance chain. Component property is usualy
		/// overriden and since Initialize is called from the constructor there is no way for inherited objects
		/// to find out what kind of component is attached.
		/// </summary>
		protected object Instance { get; }

		private bool IsCollection
		{
			get
			{
				if (Property != null)
					return Property.IsCollection();

				if (Component == null)
					return false;

				return Component.GetType().IsCollection();
			}
		}

		public int Index { get; }
		public override object Component { get; }
		public override PropertyInfo Property { get; }
		public override bool HasChildren { get { return Properties.Count > 0; } }
		public override int ChildrenCount { get { return IsCollection ? Properties.Count : 0; } }

		public override void LoadChildren()
		{
			foreach (var i in Properties)
				Items.Add(i);
		}

		public override void LoadChildren(string id)
		{
			var t = Properties.FirstOrDefault(f => string.Compare(f.Id, id, true) == 0);

			if (t != null)
				Items.Add(t);
		}

		private void Initialize()
		{
			GlyphAttribute att = null;

			if (Property != null)
			{
				att = Property.FindAttribute<GlyphAttribute>();

				if (att == null)
					att = Property.PropertyType.FindAttribute<GlyphAttribute>();
			}

			if (att != null)
				Glyph = att.Glyph;
		}

		protected virtual List<IDomElement> Properties
		{
			get
			{
				if (_props == null)
					_props = Children(Value);

				return _props;
			}
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
				{
					if (Property != null)
					{
						var att = Property.ResolveDesigner();

						if (att != null)
							_designer = DomQuery.CreateDesigner(this, att);
					}

					if (_designer == null && Value != null)
					{
						var att = Value.GetType().ResolveDesigner();

						if (att != null)
							_designer = DomQuery.CreateDesigner(this, att);
					}

					if (_designer == null && Value != null && IsDesignTime)
					{
						var de = Value.GetType().FindAttribute<System.ComponentModel.DefaultEventAttribute>();

						if (de != null)
						{
							_designer = PropertyDesigner(de.Name);

							if (_designer != null && string.IsNullOrWhiteSpace(Environment.Selection.Property))
								Environment.RequestBody["property"] = de.Name;
						}
					}

					if (_designer == null)
						_designer = this.SystemDesigner(Property);
				}

				return _designer;
			}
		}

		public virtual object[] PropertySources
		{
			get
			{
				if (_propertySources == null)
				{
					_propertySources = new List<object>
					{
						Value
					};
				}

				return _propertySources.ToArray();
			}
		}

		public override bool Commit(object component, string property, string attribute)
		{
			if (!(component is IElement e))
				return base.Commit(component, property, attribute);

			var config = e.Configuration();

			if (config != null)
			{
				Tenant.GetService<IComponentDevelopmentService>().Update(config);

				return true;
			}

			return base.Commit(component, property, attribute);
		}
	}
}
