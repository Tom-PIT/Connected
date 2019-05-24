using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Dom;
using TomPIT.Reporting.Security;
using TomPIT.Security;

namespace TomPIT.Reporting.Design.Dom
{
	internal class ViewElement : ComponentPermissionElement
	{
		private List<string> _claims = null;
		private IPermissionDescriptor _descriptor = null;

		public ViewElement(IDomElement parent, IComponent component) : base(parent, component)
		{
		}

		public override List<string> Claims
		{
			get
			{
				if (_claims == null)
				{
					_claims = new List<string>
					{
						TomPIT.Claims.AccessUserInterface
					};
				}

				return _claims;
			}
		}

		public override IPermissionDescriptor PermissionDescriptor
		{
			get
			{
				if (_descriptor == null)
					_descriptor = new ViewPermissionDescriptor();

				return _descriptor;
			}
		}

		public override bool SupportsInherit => true;
	}
}
