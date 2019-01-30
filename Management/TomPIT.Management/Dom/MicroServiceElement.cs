using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Ide;
using TomPIT.Security;

namespace TomPIT.Dom
{
	internal class MicroServiceElement : TransactionElement, IMicroServiceScope, IPermissionElement
	{
		private IDomDesigner _designer = null;
		private IMicroServiceTemplate _template = null;
		private List<string> _claims = null;
		private IPermissionDescriptor _descriptor = null;
		private List<IDomElement> _root = null;

		public MicroServiceElement(IEnvironment environment, IDomElement parent, IMicroService microService) : base(environment, parent)
		{
			MicroService = microService;
			Title = MicroService.Name;
			Id = MicroService.Token.AsString();
		}

		public IMicroService MicroService { get; }
		public override object Component => MicroService;
		public override bool HasChildren { get { return true; } }

		public override void LoadChildren()
		{
			if (Root != null)
			{
				foreach (var i in Root)
					Items.Add(i);
			}
			//Items.Add(new PermissionsElement(Environment, this));
			//Items.Add(new ConnectivityElement(Environment, this));
			//Items.Add(new WorkersElement(Environment, this));
			Items.Add(new ContentElement(Environment, this));
			Items.Add(new PackageElement(Environment, this));
		}

		public override void LoadChildren(string id)
		{
			if (Root == null)
				return;

			var d = Root.FirstOrDefault(f => f.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

			if (d != null)
				Items.Add(d);
			else
			{
				if (id.Equals(ContentElement.DomId, StringComparison.OrdinalIgnoreCase))
					Items.Add(new ContentElement(Environment, this));
				//else if (id.Equals(ConnectivityElement.DomId, StringComparison.OrdinalIgnoreCase))
				//	Items.Add(new ConnectivityElement(Environment, this));
				//else if (id.Equals(WorkersElement.DomId, StringComparison.OrdinalIgnoreCase))
				//	Items.Add(new WorkersElement(Environment, this));
				//else if (id.Equals(PermissionsElement.FolderId, StringComparison.OrdinalIgnoreCase))
				//	Items.Add(new PermissionsElement(Environment, this));
				else if (id.Equals(PackageElement.DomId, StringComparison.OrdinalIgnoreCase))
					Items.Add(new PackageElement(Environment, this));
			}
		}

		public override bool Commit(object component, string property, string attribute)
		{
			Connection.GetService<IMicroServiceManagementService>().Update(MicroService.Token, MicroService.Name,
				MicroService.Status, MicroService.Template, MicroService.ResourceGroup, MicroService.Package);

			return true;
		}

		public List<string> Claims
		{
			get
			{
				if (_claims == null)
					_claims = new List<string>
					{
						TomPIT.Claims.ImplementMicroservice
					};

				return _claims;
			}
		}

		public string PrimaryKey => MicroService.Token.AsString();

		public IPermissionDescriptor PermissionDescriptor
		{
			get
			{
				if (_descriptor == null)
					_descriptor = new MicroServicePermissionDescriptor();

				return _descriptor;
			}
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new Designers.PermissionsDesigner(Environment, this);

				return _designer;
			}
		}

		public bool SupportsInherit => false;

		public Guid ResourceGroup => MicroService.ResourceGroup;

		public string PermissionComponent => null;

		private List<IDomElement> Root
		{
			get
			{
				if (_root == null && Template != null)
					_root = Template.QueryDomRoot(Environment, this, MicroService.Token);

				return _root;
			}
		}

		private IMicroServiceTemplate Template
		{
			get
			{
				if (_template == null)
					_template = Connection.GetService<IMicroServiceTemplateService>().Select(MicroService.Template);

				return _template;
			}
		}
	}
}
