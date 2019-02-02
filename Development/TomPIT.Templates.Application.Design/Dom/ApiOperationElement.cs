using System.Collections.Generic;
using System.Reflection;
using TomPIT.Application.Security;
using TomPIT.ComponentModel.Apis;
using TomPIT.Dom;
using TomPIT.Security;

namespace TomPIT.Application.Design.Dom
{
	internal class ApiOperationElement : ElementPermissionElement
	{
		private List<string> _claims = null;
		private IPermissionDescriptor _descriptor = null;

		public ApiOperationElement(ReflectorCreateArgs e) : base(e)
		{
		}

		public ApiOperationElement(IDomElement parent, object instance) : base(parent, instance)
		{
		}

		public ApiOperationElement(IDomElement parent, object instance, PropertyInfo property, int index) : base(parent, instance, property, index)
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
					_descriptor = new ApiOperationPermissionDescriptor();

				return _descriptor;
			}
		}

		public override string PermissionComponent => ConfigurationElement.Closest<IApi>().Component.ToString();
		public override bool SupportsInherit => true;
	}
}
