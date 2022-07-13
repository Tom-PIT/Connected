using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Design.Designers;
using TomPIT.Design.Ide.Designers;
using TomPIT.Design.Ide.Dom;
using TomPIT.Environment;
using TomPIT.Ide.Dom;
using TomPIT.Security;
using TomPIT.Security.PermissionDescriptors;

namespace TomPIT.Management.Dom
{
	internal class UrlSecurityElement : DomElement, IPermissionElement
	{
		public const string FolderId = "Url";
		private List<string> _items = null;
		private List<IViewConfiguration> _views = null;
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

		public List<IViewConfiguration> Views
		{
			get
			{
				if (_views == null)
				{
					_views = new List<IViewConfiguration>();

					var resourceGroups = Environment.Context.Tenant.GetService<IResourceGroupService>().Query().Select(f => f.Name).ToList();
					var views = Environment.Context.Tenant.GetService<IComponentService>().QueryConfigurations(resourceGroups, ComponentCategories.View);

					foreach (var view in views)
					{
						if (view is IViewConfiguration iv)
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
				if (_items == null)
				{
					_items = new List<string>();

					foreach (var view in Views)
					{
						if (!(view is IViewConfiguration iview))
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

		public Guid ResourceGroup
        {
			get
			{
				var token = Environment.Context.Tenant.GetService<IResourceGroupService>().Default.Token;

				return token;
			}
		}
		public string PermissionComponent => null;
	}
}
