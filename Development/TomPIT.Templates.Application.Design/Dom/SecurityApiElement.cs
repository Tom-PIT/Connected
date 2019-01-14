using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Application.Security;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Design;
using TomPIT.Designers;
using TomPIT.Dom;
using TomPIT.Ide;
using TomPIT.Security;

namespace TomPIT.Application.Design.Dom
{
	internal class SecurityApiElement : TomPIT.Dom.Element, IPermissionElement
	{
		private IDomDesigner _designer = null;
		private List<string> _claims = null;
		private IPermissionDescriptor _descriptor = null;
		private IApi _api = null;

		public SecurityApiElement(IEnvironment environment, IDomElement parent, IComponent component) : base(environment, parent)
		{
			Api = component;

			Id = Api.Token.ToString();
			Title = Api.Name;
		}

		public override bool HasChildren => true;
		public override int ChildrenCount => ApiConfiguration.Operations.Count(f => f.Scope == ElementScope.Public);
		private IComponent Api { get; }

		public override void LoadChildren()
		{
			foreach (var i in ApiConfiguration.Operations.Where(f => f.Scope == ElementScope.Public))
				Items.Add(new SecurityApiOperationElement(Environment, this, i));
		}

		public override void LoadChildren(string id)
		{
			var operation = ApiConfiguration.Operations.FirstOrDefault(f => f.Id == id.AsGuid());

			if (operation != null)
				Items.Add(new SecurityApiOperationElement(Environment, this, operation));
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
						TomPIT.Claims.Invoke
					};

				return _claims;
			}
		}

		public string PrimaryKey => Api.Token.ToString();

		public IPermissionDescriptor PermissionDescriptor
		{
			get
			{
				if (_descriptor == null)
					_descriptor = new ApiPermissionDescriptor();

				return _descriptor;
			}
		}

		private IApi ApiConfiguration
		{
			get
			{
				if (_api == null)
					_api = Connection.GetService<IComponentService>().SelectConfiguration(Api.Token) as IApi;

				return _api;
			}
		}

		public bool SupportsInherit => false;

		public Guid ResourceGroup => DomQuery.Closest<IMicroServiceScope>(this).MicroService.ResourceGroup;

		public string PermissionComponent => null;
	}
}