using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TomPIT.ComponentModel.UI;
using TomPIT.Design;
using TomPIT.Designers;
using TomPIT.Dom;
using TomPIT.Environment;
using TomPIT.Security;

namespace TomPIT.Management.Dom
{
	internal class UrlSecurityElement : Element, IPermissionElement
	{
		public const string FolderId = "Url";
		private List<string> _items = null;
		private List<IView> _views = null;
		private List<string> _claims = null;
		private IPermissionDescriptor _descriptor = null;
		private IDomDesigner _designer = null;

		public UrlSecurityElement(IDomElement parent) : base(parent)
		{
			Id = FolderId;
			Title = "Url";
		}

		public override bool HasChildren { get { return Urls.Count > 0; } }
		public override int ChildrenCount { get { return Urls.Count; } }

		public override void LoadChildren()
		{
			Urls.Sort();

			foreach (var item in Urls)
				Items.Add(new UrlElement(this, item));
		}

		public override void LoadChildren(string id)
		{
			var url = Urls.FirstOrDefault(f => string.Compare(f, id, true) == 0);

			if (url != null)
				Items.Add(new UrlElement(this, url));
		}

		public List<IView> Views
		{
			get
			{
				if(_views == null)
				{
					_views = new List<IView>();

					var resourceGroups = Connection.GetService<IResourceGroupService>().Query().Select(f => f.Name).ToList();
					var views = Connection.GetService<ComponentModel.IComponentService>().QueryConfigurations(resourceGroups, "View");

					foreach(var view in views)
					{
						if (view is IView iv)
							_views.Add(iv);
					}
				}

				return _views;
			}
		}
		private List<string> Urls
		{
			get
			{
				if(_items==null)
				{
					_items = new List<string>();

					foreach(var view in Views)
					{
						if (!(view is IView iview))
							continue;

						if (string.IsNullOrWhiteSpace(iview.Url))
							continue;

						var root = iview.Url.Split('/')[0];

						if (_items.Contains(root.ToLowerInvariant()))
							continue;

						_items.Add(root.ToLowerInvariant());
					}
				}

				return _items;
			}
		}

		public List<string> Claims
		{
			get
			{
				if (_claims == null)
					_claims = new List<string>
						  {
								TomPIT.Claims.DefaultAccessUrl
						  };

				return _claims;
			}
		}

		public string PrimaryKey => 0.ToString();

		public IPermissionDescriptor PermissionDescriptor
		{
			get
			{
				if (_descriptor == null)
					_descriptor = new DefaultUrlPermissionDescriptor();

				return _descriptor;
			}
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new PermissionsDesigner(this);

				return _designer;
			}
		}

		public bool SupportsInherit => false;

		public Guid ResourceGroup => Connection.GetService<IResourceGroupService>().Default.Token;

		public string PermissionComponent => null;
	}
}
