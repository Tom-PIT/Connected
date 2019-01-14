using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Application.Security;
using TomPIT.Application.UI;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Designers;
using TomPIT.Dom;
using TomPIT.Ide;
using TomPIT.Security;

namespace TomPIT.Application.Design.Dom
{
	internal class SecurityAreaElement : TomPIT.Dom.Element, IPermissionElement
	{
		private IDomDesigner _designer = null;
		private List<string> _claims = null;
		private IPermissionDescriptor _descriptor = null;
		private List<View> _views = null;

		public SecurityAreaElement(IEnvironment environment, IDomElement parent, IComponent component) : base(environment, parent)
		{
			Area = component;

			Id = Area.Token.ToString();
			Title = Area.Name;
		}

		public override bool HasChildren => Views.Count > 0;
		public override int ChildrenCount => Views.Count;
		private IComponent Area { get; }

		public override void LoadChildren()
		{
			foreach (var i in Views)
				Items.Add(new SecurityViewElement(Environment, this, i));
		}

		public override void LoadChildren(string id)
		{
			var view = Views.FirstOrDefault(f => f.Component == id.AsGuid());

			if (view != null)
				Items.Add(new SecurityViewElement(Environment, this, view));
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new PermissionsDesigner(Environment, this);

				return _designer;
			}
		}

		public List<string> Claims
		{
			get
			{
				if (_claims == null)
					_claims = new List<string>
					{
						TomPIT.Claims.AccessUserInterface
					};

				return _claims;
			}
		}

		public string PrimaryKey => Area.Token.ToString();

		public IPermissionDescriptor PermissionDescriptor
		{
			get
			{
				if (_descriptor == null)
					_descriptor = new AreaPermissionDescriptor();

				return _descriptor;
			}
		}

		private List<View> Views
		{
			get
			{
				if (_views == null)
				{
					var ms = DomQuery.Closest<IMicroServiceScope>(this).MicroService.Token;
					var components = Connection.GetService<IComponentService>().QueryComponents(ms, "View");
					var config = Connection.GetService<IComponentService>().QueryConfigurations(components);

					_views = new List<View>();

					foreach (var i in config)
					{
						if (!(i is View v) || v.Area != Area.Token)
							continue;

						_views.Add(v);
					}
				}

				return _views;
			}
		}

		public bool SupportsInherit => false;

		public Guid ResourceGroup => DomQuery.Closest<IMicroServiceScope>(this).MicroService.ResourceGroup;

		public string PermissionComponent => null;
	}
}