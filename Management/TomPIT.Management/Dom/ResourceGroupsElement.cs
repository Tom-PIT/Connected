using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations.Design;
using TomPIT.Environment;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Dom;
using TomPIT.Ide.Environment;
using TomPIT.Management.Designers;
using TomPIT.Management.Environment;
using TomPIT.Management.Items;

namespace TomPIT.Management.Dom
{
	public class ResourceGroupsElement : DomElement
	{
		private ExistingResourceGroups _ds = null;
		public const string FolderId = "ResourceGroups";
		private ResourceGroupsDesigner _designer = null;
		private PropertyInfo _property = null;

		private class ExistingResourceGroups
		{
			private List<IResourceGroup> _items = null;

			[Items(typeof(ResourceGroupsCollection))]
			[Browsable(false)]
			public List<IResourceGroup> Items
			{
				get
				{
					if (_items == null)
						_items = new List<IResourceGroup>();

					return _items;
				}
			}
		}

		public ResourceGroupsElement(IEnvironment environment) : base(environment, null)
		{
			Id = FolderId;
			Glyph = "fal fa-object-group";
			Title = SR.DomResourceGroups;

			((Behavior)Behavior).AutoExpand = true;
		}

		public override object Component => ResourceGroups;
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
				Items.Add(new ResourceGroupElement(this, i));
		}

		public override void LoadChildren(string id)
		{
			var d = Existing.FirstOrDefault(f => f.Token == new Guid(id));

			if (d != null)
				Items.Add(new ResourceGroupElement(this, d));
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new ResourceGroupsDesigner(this);

				return _designer;
			}
		}

		private ExistingResourceGroups ResourceGroups
		{
			get
			{
				if (_ds == null)
				{
					_ds = new ExistingResourceGroups();

					var items = Environment.Context.Tenant.GetService<IResourceGroupManagementService>().Query();

					if (items != null)
						items = items.OrderBy(f => f.Name).ToList();

					foreach (var i in items)
						_ds.Items.Add(i);
				}

				return _ds;
			}
		}

		public List<IResourceGroup> Existing { get { return ResourceGroups.Items; } }

	}
}
