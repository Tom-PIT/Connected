using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Design.Designers;
using TomPIT.Dom;
using TomPIT.Ide;
using TomPIT.Ide.ComponentModel;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Dom;
using TomPIT.Ide.Dom.ComponentModel;
using TomPIT.Management.ComponentModel;
using TomPIT.Security;
using TomPIT.Security.PermissionDescriptors;

namespace TomPIT.Management.Dom
{
	internal class MicroServiceManagementElement : TransactionElement, IMicroServiceScope, IPermissionElement
	{
		private List<IFolder> _folders = null;
		private List<IComponent> _components = null;
		private IDomDesigner _designer = null;
		private List<string> _claims = null;
		private IPermissionDescriptor _descriptor = null;

		public MicroServiceManagementElement(IDomElement parent, IMicroService microService) : base(parent)
		{
			MicroService = microService;
			Title = MicroService.Name;
			Id = MicroService.Token.ToString();
			Glyph = "fal fa-share-alt";

			Template = Environment.Context.Tenant.GetService<IMicroServiceTemplateService>().Select(MicroService.Template);
		}

		public IMicroService MicroService { get; }
		private IMicroServiceTemplate Template { get; }
		public override object Component => MicroService;
		public override bool HasChildren { get { return true; } }

		public override void LoadChildren()
		{
			var refs = Template.References(Environment, MicroService.Token);

			Items.Add(new ReferencesElement(this, refs));

			foreach (var i in Folders.OrderBy(f => f.Name))
				Items.Add(new FolderElement(this, i));

			foreach (var i in Components.OrderBy(f => f.Name))
			{
				if (string.Compare(i.Category, "Reference", true) == 0)
					continue;

				Items.Add(i.GetDomElement(this));
			}

			Items.Add(new PackageElement(Environment, this));
		}

		public override void LoadChildren(string id)
		{
			if (id.Equals(PackageElement.DomId, StringComparison.OrdinalIgnoreCase))
				Items.Add(new PackageElement(Environment, this));

			var folder = Folders.FirstOrDefault(f => f.Token == new Guid(id));

			if (folder != null)
			{
				Items.Add(new FolderElement(this, folder));
				return;
			}

			var component = Components.FirstOrDefault(f => f.Token == new Guid(id));

			if (component != null)
			{
				if (string.Compare(component.Category, "Reference", true) == 0)
					Items.Add(new ReferencesElement(this, component));
				else
					Items.Add(component.GetDomElement(this));
			}
		}

		public override bool Commit(object component, string property, string attribute)
		{
			Environment.Context.Tenant.GetService<IMicroServiceManagementService>().Update(MicroService.Token, MicroService.Name,
				 MicroService.Status, MicroService.Template, MicroService.ResourceGroup, MicroService.Package, MicroService.Plan, MicroService.UpdateStatus, MicroService.CommitStatus);

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

		public string PrimaryKey => MicroService.Token.ToString();

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
					_designer = new PermissionsDesigner(this);

				return _designer;
			}
		}

		public bool SupportsInherit => false;

		public Guid ResourceGroup => MicroService.ResourceGroup;

		public string PermissionComponent => null;

		private List<IFolder> Folders
		{
			get
			{
				if (_folders == null)
					_folders = Environment.Context.Tenant.GetService<IComponentService>().QueryFolders(MicroService.Token, Guid.Empty);

				return _folders;
			}
		}

		private List<IComponent> Components
		{
			get
			{
				if (_components == null)
					_components = Environment.Context.Tenant.GetService<IComponentService>().QueryComponents(MicroService.Token, Guid.Empty);

				return _components;
			}
		}
	}
}
