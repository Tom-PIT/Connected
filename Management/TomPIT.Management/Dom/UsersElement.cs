using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations.Design;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Dom;
using TomPIT.Management.Designers;
using TomPIT.Management.Items;
using TomPIT.Security;

namespace TomPIT.Management.Dom
{
	public class UsersElement : DomElement
	{
		public const string FolderId = "Users";
		private UsersDesigner _designer = null;
		private ExistingUsers _users = null;
		private PropertyInfo _property = null;

		private class ExistingUsers
		{
			private List<IUser> _items = null;

			[Items(typeof(UsersCollection))]
			[Browsable(false)]
			public List<IUser> Items
			{
				get
				{
					if (_items == null)
						_items = new List<IUser>();

					return _items;
				}
			}
		}
		public UsersElement(IDomElement parent) : base(parent)
		{
			Id = FolderId;
			Glyph = "fal fa-users";
			Title = SR.DomUsers;

			((Behavior)Behavior).AutoExpand = true;
		}

		public override object Component => Users;
		public override bool HasChildren { get { return Users.Items.Count > 0; } }
		public override int ChildrenCount { get { return Users.Items.Count; } }
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
				Items.Add(new UserElement(this, i));
		}

		public override void LoadChildren(string id)
		{
			var user = Environment.Context.Tenant.GetService<IUserService>().Select(id);

			Items.Add(new UserElement(this, user));
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new UsersDesigner(this);

				return _designer;
			}
		}

		private ExistingUsers Users
		{
			get
			{
				if (_users == null)
				{
					_users = new ExistingUsers();

					var ds = Environment.Context.Tenant.GetService<IUserService>().Query().OrderBy(f => f.DisplayName());

					foreach (var i in ds)
						_users.Items.Add(i);
				}

				return _users;
			}
		}

		public List<IUser> Existing { get { return Users.Items; } }
	}
}
