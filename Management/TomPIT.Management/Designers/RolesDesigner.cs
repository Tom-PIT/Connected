using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.ActionResults;
using TomPIT.Actions;
using TomPIT.Annotations;
using TomPIT.Design;
using TomPIT.Dom;
using TomPIT.Ide;
using TomPIT.Items;
using TomPIT.Security;

namespace TomPIT.Designers
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

			Connection.GetService<IRoleManagementService>().Delete(user.Token);

			return Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);
		}

		protected override IDesignerActionResult Clear(JObject data)
		{
			foreach (var i in Owner.Existing)
				Connection.GetService<IRoleManagementService>().Delete(i.Token);

			return Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);
		}

		private IDesignerActionResult CreateRole()
		{
			var existing = Owner.Existing;
			var name = Connection.GetService<INamingService>().Create("Role", existing.Select(f => f.Name), true);

			var id = Connection.GetService<IRoleManagementService>().Insert(name);
			var role = Connection.GetService<IRoleService>().Select(id);

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
