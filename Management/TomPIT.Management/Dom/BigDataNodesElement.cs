using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations.Design;
using TomPIT.BigData;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Designers;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Dom;
using TomPIT.Management.BigData;
using TomPIT.Management.Designers;
using TomPIT.Management.Items;

namespace TomPIT.Management.Dom
{
	public class BigDataNodesElement : DomElement
	{
		private ExistingResourceGroups _ds = null;
		public const string FolderId = "BigDataNodes";
		private BigDataNodesDesigner _designer = null;
		private PropertyInfo _property = null;

		private class ExistingResourceGroups
		{
			private List<INode> _items = null;

			[Items(typeof(BigDataNodesCollection))]
			[Browsable(false)]
			public List<INode> Items
			{
				get
				{
					if (_items == null)
						_items = new List<INode>();

					return _items;
				}
			}
		}

		public BigDataNodesElement(IEnvironment environment, IDomElement parent) : base(environment, parent)
		{
			Id = FolderId;
			Glyph = "fal fa-database";
			Title = "Nodes";

			((Behavior)Behavior).AutoExpand = true;
		}

		public override object Component => Nodes;
		public override bool HasChildren { get { return Existing.Count > 0; } }
		public override int ChildrenCount => Existing.Count;
		public override PropertyInfo Property
		{
			get
			{
				if (_property == null)
					_property = Component.GetType().GetProperty("Items");

				return _property;
			}
		}

		public override void LoadChildren()
		{
			foreach (var i in Existing)
				Items.Add(new BigDataNodeElement(this, i));
		}

		public override void LoadChildren(string id)
		{
			var d = Existing.FirstOrDefault(f => f.Token == new Guid(id));

			if (d != null)
				Items.Add(new BigDataNodeElement(this, d));
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new BigDataNodesDesigner(this);

				return _designer;
			}
		}

		private ExistingResourceGroups Nodes
		{
			get
			{
				if (_ds == null)
				{
					_ds = new ExistingResourceGroups();

					var items = Environment.Context.Tenant.GetService<IBigDataManagementService>().QueryNodes();

					if (items != null)
						items = items.OrderBy(f => f.Name).ToList();

					foreach (var i in items)
						_ds.Items.Add(i);
				}

				return _ds;
			}
		}

		public List<INode> Existing { get { return Nodes.Items; } }

	}
}
