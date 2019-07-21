using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TomPIT.ActionResults;
using TomPIT.Actions;
using TomPIT.Annotations;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Designers
{
	public class CollectionDesigner<E> : DomDesigner<E>, ICollectionDesigner where E : IDomElement
	{
		private const string DefaultCollectionItem = "C402BBDC8FB44DD990241A13F9039956";

		private List<IItemDescriptor> _items = null;
		private List<IItemDescriptor> _descriptors = null;

		public CollectionDesigner(E element) : base(element)
		{
		}

		protected override void OnCreateToolbar(IDesignerToolbar toolbar)
		{
			var addItems = new AddItems(Environment);

			foreach (var i in Descriptors)
			{
				var item = new ToolbarAction(Environment)
				{
					Id = i.Id,
					Text = i.Text
				};

				addItems.Items.Add(item);
			}

			if (OnCreateToolbarAction(addItems))
				Toolbar.Items.Add(addItems);

			var search = new Actions.Search(Environment);

			if (OnCreateToolbarAction(search))
				Toolbar.Items.Add(search);

			var clear = new Clear(Environment);

			if (OnCreateToolbarAction(clear))
				Toolbar.Items.Add(clear);

		}

		protected virtual bool OnCreateToolbarAction(IDesignerToolbarAction action)
		{
			return true;
		}

		public override string View { get { return "~/Views/Ide/Designers/Collection.cshtml"; } }
		public override object ViewModel { get { return this; } }

		public virtual List<IItemDescriptor> Items
		{
			get
			{
				if (_items == null)
				{
					_items = new List<IItemDescriptor>();

					if (Component is IEnumerable arr)
					{
						var en = arr.GetEnumerator();
						var idx = 0;

						while (en.MoveNext())
						{
							var d = new ItemDescriptor
							{
								Text = en.Current.ToString(),
								Id = DomQuery.Key(en.Current, idx.AsString()),
								Value = en.Current
							};

							_items.Add(d);

							idx++;
						}
					}
				}

				if (Element.SortChildren)
					_items = _items.OrderBy(f => f.Text).ToList();

				return _items;
			}
		}


		public virtual List<IItemDescriptor> Descriptors
		{
			get
			{
				if (_descriptors == null)
				{
					_descriptors = DomQuery.Items(Element);

					if (_descriptors == null)
						_descriptors = DefaultCollectionList(Element.Property);
				}

				return _descriptors;
			}
		}

		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			var d = Descriptors.FirstOrDefault(f => string.Compare(action, f.Id, true) == 0);

			if (d != null)
				return Add(d);
			else if (string.Compare(action, "move", true) == 0)
				return Move(data);
			else if (string.Compare(action, "clear", true) == 0)
				return Clear(data);
			else if (string.Compare(action, "remove", true) == 0)
				return Remove(data);
			else
				throw IdeException.DesignerActionNotSupported(this, IdeEvents.DesignerAction, action);
		}

		protected virtual IDesignerActionResult Remove(JObject data)
		{
			var idx = data.Optional("index", 0);
			var mi = Component.GetType().GetMethod("Remove");

			if (mi == null)
				throw IdeException.MissingRemoveMethod(this, IdeEvents.DesignerAction, Component.GetType().Name);

			var indexer = Component.GetType().GetProperty("Item");

			if (indexer == null)
				throw IdeException.MissingIndexer(this, IdeEvents.DesignerAction, Component.GetType().Name);

			var item = indexer.GetValue(Component, new object[] { idx });

			mi.Invoke(Component, new object[] { item });

			Environment.Commit(Component, Element.Property == null ? string.Empty : Element.Property.Name, null);

			return Result.SectionResult(this, EnvironmentSection.Explorer | EnvironmentSection.Designer);
		}

		protected virtual IDesignerActionResult Clear(JObject data)
		{
			var mi = Component.GetType().GetMethod("Clear");

			if (mi == null)
				throw IdeException.MissingClearMethod(this, IdeEvents.DesignerAction, Component.GetType().Name);

			mi.Invoke(Component, null);

			Environment.Commit(Component, Element.Property == null ? string.Empty : Element.Property.Name, null);

			return Result.SectionResult(this, EnvironmentSection.Explorer | EnvironmentSection.Designer);
		}

		protected virtual IDesignerActionResult Move(JObject data)
		{
			var oldIndex = data.Optional("oldIndex", 0);
			var index = data.Optional("index", 0);

			var remove = Component.GetType().GetMethod("Remove");

			if (remove == null)
				throw IdeException.MissingRemoveMethod(this, IdeEvents.DesignerAction, Component.GetType().Name);

			var indexer = Component.GetType().GetProperty("Item");

			if (indexer == null)
				throw IdeException.MissingIndexer(this, IdeEvents.DesignerAction, Component.GetType().Name);

			var insert = Component.GetType().GetMethod("Insert");

			if (insert == null)
				throw IdeException.MissingInsertMethod(this, IdeEvents.DesignerAction, Component.GetType().Name);

			var item = indexer.GetValue(Component, new object[] { oldIndex });

			remove.Invoke(Component, new object[] { item });
			insert.Invoke(Component, new object[] { index, item });

			Environment.Commit(Component, Element.Property == null ? string.Empty : Element.Property.Name, null);

			return Result.SectionResult(this, EnvironmentSection.Explorer);
		}

		protected virtual IDesignerActionResult Add(IItemDescriptor d)
		{
			var items = Component as IEnumerable;
			var df = d.Type.CreateInstance();

			if (df == null)
				throw IdeException.CannotCreateInstance(this, IdeEvents.DesignerAction, d.Type);

			MethodInfo mi = Component.GetType().GetMethod("Add");

			if (mi == null)
				throw IdeException.MissingAddMethod(this, IdeEvents.DesignerAction, Component.GetType().Name);

			Connection.GetService<INamingService>().Create(df, items);

			IdeExtensions.ProcessComponentCreating(Environment.Context, df);

			mi.Invoke(Component, new object[] { df });

			IdeExtensions.ProcessComponentCreated(Environment.Context, df);

			Environment.Commit(Component, Element.Property == null ? string.Empty : Element.Property.Name, null);

			var r = Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);

			var name = df.ToString();
			var id = DomQuery.Key(df, string.Empty);

			r.MessageKind = InformationKind.Success;

			if (!string.IsNullOrWhiteSpace(name))
			{
				if (!string.IsNullOrWhiteSpace(id))
				{
					r.Message = string.Format("{0} created. Click here to edit.", name);
					r.ExplorerPath = string.Format("{0}/{1}", DomQuery.Path(Element), id);
				}
				else
					r.Message = string.Format("{0} created.", name);
			}

			return r;
		}

		public List<IItemDescriptor> DefaultCollectionList(PropertyInfo property)
		{
			var r = new List<IItemDescriptor>();

			if (property == null)
				return r;

			if (!property.PropertyType.IsCollection())
				return r;

			Type t = null;

			if (property.PropertyType.IsGenericType)
			{
				var gt = property.PropertyType.GetGenericArguments();

				if (gt == null || gt.Length == 0)
					return r;

				t = gt[0];
			}
			else
				t = property.PropertyType.GetElementType();

			if (t == null || t.IsInterface)
				return r;

			var c = t.GetConstructor(new Type[0]);

			if (c != null)
				r.Add(new ItemDescriptor(t.Name, DefaultCollectionItem, t));

			return r;
		}

		public virtual bool SupportsReorder { get { return !Element.SortChildren; } }

		public virtual string ItemTemplateView => null;

		public override bool IsPropertyEditable(string propertyName)
		{
			if (string.Compare("Capacity", propertyName, true) == 0)
				return false;

			return base.IsPropertyEditable(propertyName);
		}
	}
}
