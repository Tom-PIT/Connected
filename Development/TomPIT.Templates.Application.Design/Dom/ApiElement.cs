using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Dom.ComponentModel;
using TomPIT.MicroService.Security;
using TomPIT.Security;

namespace TomPIT.MicroServices.Design.Dom
{
	internal class ApiElement : ComponentPermissionElement
	{
		private List<string> _claims = null;
		private IPermissionDescriptor _descriptor = null;

		public ApiElement(IDomElement parent, IComponent component) : base(parent, component)
		{
		}

		public override List<string> Claims
		{
			get
			{
				if (_claims == null)
					_claims = new List<string>
					{
						TomPIT.Claims.Invoke
					};

				return _claims;
			}
		}

		public override IPermissionDescriptor PermissionDescriptor
		{
			get
			{
				if (_descriptor == null)
					_descriptor = new ApiPermissionDescriptor();

				return _descriptor;
			}
		}
	}
}
