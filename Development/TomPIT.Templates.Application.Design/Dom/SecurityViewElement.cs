using System.Collections.Generic;
using TomPIT.Application.Security;
using TomPIT.Application.UI;
using TomPIT.Design;
using TomPIT.Designers;
using TomPIT.Dom;
using TomPIT.Ide;
using TomPIT.Security;

namespace TomPIT.Application.Dom
{
	internal class SecurityViewElement : TomPIT.Dom.Element, IPermissionElement
	{
		private IDomDesigner _designer = null;
		private List<string> _claims = null;
		private IPermissionDescriptor _descriptor = null;

		public SecurityViewElement(IEnvironment environment, IDomElement parent, View view) : base(environment, parent)
		{
			View = view;

			Id = View.Component.ToString();
			Title = View.ComponentName(Environment.Context);
		}

		public override bool HasChildren => false;
		private View View { get; }

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

		public string PrimaryKey => View.Component.ToString();

		public IPermissionDescriptor PermissionDescriptor
		{
			get
			{
				if (_descriptor == null)
					_descriptor = new ViewPermissionDescriptor();

				return _descriptor;
			}
		}

		public bool SupportsInherit => true;
	}
}