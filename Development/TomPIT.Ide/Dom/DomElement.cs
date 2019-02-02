using System.Collections.Generic;
using System.Reflection;
using TomPIT.Connectivity;
using TomPIT.Design;
using TomPIT.Ide;
using TomPIT.Services;

namespace TomPIT.Dom
{
	public abstract class DomElement : IDomElement
	{
		private IDomElementBehavior _behavior = null;
		private bool _sortChildren = true;
		private List<IVerb> _verbs = null;

		protected DomElement(IEnvironment environment, IDomElement parent)
		{
			Parent = parent;
			Environment = environment;
		}

		protected DomElement(IDomElement parent)
		{
			Parent = parent;
			Environment = parent.Environment;
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

		public virtual IDomDesigner Designer { get { return null; } }

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
		protected T GetService<T>() { return Environment.Context.Connection().GetService<T>(); }
		protected abstract List<IDomElement> Children(object instance);
		public abstract IDomDesigner PropertyDesigner(string propertyName);
		public virtual bool SortChildren { get { return _sortChildren; } set { _sortChildren = value; } }

		protected ISysConnection Connection { get { return Environment.Context.Connection(); } }
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

	}
}
