using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations.Design;
using TomPIT.Design.Ide.Designers;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Dom;
using TomPIT.Management.Designers;
using TomPIT.Management.Items;
using TomPIT.Security;

namespace TomPIT.Management.Dom
{
	public class RolesElement : DomElement
	{
		public const string FolderId = "Roles";
		private RolesDesigner _designer = null;
		private ExistingRoles _roles = null;
		private PropertyInfo _property = null;

		private class ExistingRoles
		{
			private List<IRole> _items = null;

			[Items(typeof(RolesCollection))]
			[Browsable(false)]
			public List<IRole> Items
			{
				get
				{
					if (_items == null)
						_items = new List<IRole>();

					return _items;
				}
			}
		}
		public RolesElement(IDomElement parent) : base(parent)
		{
			Id = FolderId;
			Glyph = "fal fa-users";
			Title = "Roles";

			((Behavior)Behavior).AutoExpand = true;
		}

		public override object Component => Roles;
		public override bool HasChildren { get { return Roles.Items.Count > 0; } }
		public override int ChildrenCount { get { return Roles.Items.Count; } }
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
				Items.Add(new RoleElement(this, i));
		}

		public override void LoadChildren(string id)
		{
			var role = Environment.Context.Tenant.GetService<IRoleService>().Select(new Guid(id));

			Items.Add(new RoleElement(this, role));
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new RolesDesigner(this);

				return _designer;
			}
		}

		private ExistingRoles Roles
		{
			get
			{
				if (_roles == null)
				{
					_roles = new ExistingRoles();

					var ds = Environment.Context.Tenant.GetService<IRoleService>().Query().Where(f => f.Behavior == RoleBehavior.Explicit && f.Visibility == RoleVisibility.Visible).OrderBy(f => f.Name);

					foreach (var i in ds)
						_roles.Items.Add(i);
				}

				return _roles;
			}
		}

		public List<IRole> Existing { get { return Roles.Items; } }
	}
}
