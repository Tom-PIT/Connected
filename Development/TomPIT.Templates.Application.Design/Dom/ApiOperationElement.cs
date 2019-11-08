using System.Collections.Generic;
using System.Reflection;
using TomPIT.ComponentModel.Apis;
using TomPIT.Ide.Dom;
using TomPIT.Ide.Dom.ComponentModel;
using TomPIT.MicroServices.Security;
using TomPIT.Reflection;
using TomPIT.Security;

namespace TomPIT.MicroServices.Design.Dom
{
	internal class ApiOperationElement : ElementPermissionElement
	{
		private List<string> _claims = null;
		private IPermissionDescriptor _descriptor = null;

		public ApiOperationElement(ReflectorCreateArgs e) : base(e)
		{
			SetGlyph();
		}

		public ApiOperationElement(IDomElement parent, object instance) : base(parent, instance)
		{
			SetGlyph();
		}

		public ApiOperationElement(IDomElement parent, object instance, PropertyInfo property, int index) : base(parent, instance, property, index)
		{
			SetGlyph();
		}

		private void SetGlyph()
		{
			if (Operation.Scope == ComponentModel.ElementScope.Public)
				Glyph = "fal fa-file-code text-success";
			else
				Glyph = "fal fa-file-code text-secondary";
		}
		private IApiOperation Operation => Component as IApiOperation;
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
					_descriptor = new ApiOperationPermissionDescriptor();

				return _descriptor;
			}
		}

		public override string PermissionComponent => ConfigurationElement.Closest<IApiConfiguration>().Component.ToString();
		public override bool SupportsInherit => true;
	}
}
