using System;
using System.Collections.Generic;
using TomPIT.Application.Security;
using TomPIT.ComponentModel.Apis;
using TomPIT.Design;
using TomPIT.Designers;
using TomPIT.Dom;
using TomPIT.Ide;
using TomPIT.Security;

namespace TomPIT.Application.Design.Dom
{
	public class SecurityApiOperationElement : TomPIT.Dom.Element, IPermissionElement
	{
		private IDomDesigner _designer = null;
		private List<string> _claims = null;
		private IPermissionDescriptor _descriptor = null;

		public SecurityApiOperationElement(IEnvironment environment, IDomElement parent, IApiOperation operation) : base(environment, parent)
		{
			Operation = operation;

			Id = Operation.Id.ToString();
			Title = Operation.Name;
		}

		public override bool HasChildren => false;
		private IApiOperation Operation { get; }

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
						TomPIT.Claims.Invoke
					};

				return _claims;
			}
		}

		public string PrimaryKey => Operation.Id.ToString();

		public IPermissionDescriptor PermissionDescriptor
		{
			get
			{
				if (_descriptor == null)
					_descriptor = new ApiOperationPermissionDescriptor();

				return _descriptor;
			}
		}

		public bool SupportsInherit => true;

		public Guid ResourceGroup => DomQuery.Closest<IMicroServiceScope>(this).MicroService.ResourceGroup;

		public string PermissionComponent => Operation.Closest<IApi>().Component.ToString();
	}
}