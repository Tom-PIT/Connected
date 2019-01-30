using System.Collections.Generic;
using TomPIT.Application.Security;
using TomPIT.ComponentModel;
using TomPIT.Dom;
using TomPIT.Ide;
using TomPIT.Security;

namespace TomPIT.Application.Design.Dom
{
	internal class AreaElement : ComponentPermissionElement, IComponentScope
	{
		private List<string> _claims = null;
		private IPermissionDescriptor _descriptor = null;

		public AreaElement(IEnvironment environment, IDomElement parent, IComponent component) : base(environment, parent, component)
		{
		}

		public override void LoadChildren()
		{
			Items.Add(new CategoryElement(Environment, this, "View", "Views", "Views", "fal fa-browser"));

			base.LoadChildren();
		}

		public override bool HasChildren => true;

		IComponent IComponentScope.Component => Target as IComponent;

		public override void LoadChildren(string id)
		{
			if (string.Compare(id, "Views", true) == 0)
				Items.Add(new CategoryElement(Environment, this, "View", "Views", "Views", "fal fa-browser"));
			else
				base.LoadChildren(id);
		}

		public override List<string> Claims
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

		public override IPermissionDescriptor PermissionDescriptor
		{
			get
			{
				if (_descriptor == null)
					_descriptor = new AreaPermissionDescriptor();

				return _descriptor;
			}
		}
	}
}
