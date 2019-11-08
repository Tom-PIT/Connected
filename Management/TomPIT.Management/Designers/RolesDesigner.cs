using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations.Design;
using TomPIT.Design;
using TomPIT.Ide;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Designers.ActionResults;
using TomPIT.Ide.Designers.Toolbar;
using TomPIT.Management.Dom;
using TomPIT.Management.Items;
using TomPIT.Management.Security;
using TomPIT.Security;

namespace TomPIT.Management.Designers
{
	internal class RolesDesigner : CollectionDesigner<RolesElement>
	{
		public RolesDesigner(RolesElement element) : base(element)
		{

		}

		protected override IDesignerActionResult Add(IItemDescriptor d)
		{
			if (string.Compare(d.Id, RolesCollection.Role, true) == 0)
				return CreateRole();

			return Result.EmptyResult(this);
		}

		protected override IDesignerActionResult Remove(JObject data)
		{
			var id = data.Required<Guid>("id");
			var user = Owner.Existing.FirstOrDefault(f => f.Token == id);

			Environment.Context.Tenant.GetService<IRoleManagementService>().Delete(user.Token);

			return Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);
		}

		protected override IDesignerActionResult Clear(JObject data)
		{
			foreach (var i in Owner.Existing)
				Environment.Context.Tenant.GetService<IRoleManagementService>().Delete(i.Token);

			return Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);
		}

		private IDesignerActionResult CreateRole()
		{
			var existing = Owner.Existing;
			var name = Environment.Context.Tenant.GetService<INamingService>().Create("Role", existing.Select(f => f.Name), true);

			var id = Environment.Context.Tenant.GetService<IRoleManagementService>().Insert(name);
			var role = Environment.Context.Tenant.GetService<IRoleService>().Select(id);

			var r = Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);

			r.MessageKind = InformationKind.Success;
			r.Message = string.Format(SR.RoleCreateSuccess, name);
			r.Title = SR.DevCreateRole;
			r.ExplorerPath = string.Format("{0}/{1}", DomQuery.Path(Element), role.Token);

			return r;
		}

		protected override bool OnCreateToolbarAction(IDesignerToolbarAction action)
		{
			return action.Id != Undo.ActionId;
		}

		public override bool SupportsReorder
		{
			get { return false; }
		}
	}
}
