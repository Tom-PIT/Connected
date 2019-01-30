using System.Collections.Generic;
using TomPIT.Application.Security;
using TomPIT.ComponentModel;
using TomPIT.Dom;
using TomPIT.Ide;
using TomPIT.Security;

namespace TomPIT.Application.Design.Dom
{
	public class ViewElement : ComponentPermissionElement
	{
		private List<string> _claims = null;
		private IPermissionDescriptor _descriptor = null;

		public ViewElement(IEnvironment environment, IDomElement parent, IComponent component) : base(environment, parent, component)
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
