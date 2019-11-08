using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations.Design;
using TomPIT.Ide;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Dom;
using TomPIT.Management.Designers;
using TomPIT.Management.Items;
using TomPIT.Management.Security;
using TomPIT.Security;

namespace TomPIT.Management.Dom
{
	internal class AuthenticationTokensElement : DomElement
	{
		private ExistingTokens _ds = null;
		private AuthenticationTokensDesigner _designer = null;
		private PropertyInfo _property = null;

		private class ExistingTokens
		{
			private List<IAuthenticationToken> _items = null;

			[Items(typeof(AuthenticationTokensCollection))]
			[Browsable(false)]
			public List<IAuthenticationToken> Items
			{
				get
				{
					if (_items == null)
						_items = new List<IAuthenticationToken>();

					return _items;
				}
			}
		}

		public const string ElementId = "RgAuthTokens";

		public AuthenticationTokensElement(IDomElement parent) : base(parent)
		{
			Glyph = "fal fa-folder";
			Title = "Authentication tokens";
			Id = ElementId;
		}

		public override object Component => Tokens;
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
				Items.Add(new AuthenticationTokenElement(this, i));
		}

		public override void LoadChildren(string id)
		{
			var d = Existing.FirstOrDefault(f => f.Token == new Guid(id));

			if (d != null)
				Items.Add(new AuthenticationTokenElement(this, d));
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new AuthenticationTokensDesigner(this);

				return _designer;
			}
		}

		private ExistingTokens Tokens
		{
			get
			{
				if (_ds == null)
				{
					_ds = new ExistingTokens();

					var items = Environment.Context.Tenant.GetService<IAuthenticationTokenManagementService>().Query(DomQuery.Closest<IResourceGroupScope>(this).ResourceGroup.Name);

					if (items != null)
						items = items.OrderBy(f => f.Key).ToList();

					foreach (var i in items)
						_ds.Items.Add(i);
				}

				return _ds;
			}
		}

		public List<IAuthenticationToken> Existing { get { return Tokens.Items; } }

	}
}
