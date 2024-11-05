using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Designers;
using TomPIT.Ide;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Designers.ActionResults;
using TomPIT.Ide.Designers.Toolbar;
using TomPIT.Management.ComponentModel;
using TomPIT.Management.Dom;
using TomPIT.Management.Items;

namespace TomPIT.Management.Designers
{
	internal class MicroServicesDesigner : CollectionDesigner<MicroServicesElement>
	{
		public MicroServicesDesigner(MicroServicesElement element) : base(element)
		{

		}

		protected override IDesignerActionResult Add(IItemDescriptor d)
		{
			if (string.Compare(d.Id, MicroServicesCollection.MicroService, true) == 0)
				return CreateMicroService();

			return Result.EmptyResult(this);
		}

		protected override IDesignerActionResult Remove(JObject data)
		{
			var id = data.Optional("id", Guid.Empty);

			if (id == Guid.Empty)
				throw IdeException.ExpectedParameter(this, IdeEvents.DesignerAction, "id");

			var user = Owner.Existing.FirstOrDefault(f => f.Token == id);

			Environment.Context.Tenant.GetService<IMicroServiceManagementService>().Delete(user.Token);

			return Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);
		}

		private IDesignerActionResult CreateMicroService()
		{
			var existing = Owner.Existing;
			var name = Environment.Context.Tenant.GetService<INamingService>().Create("MicroService", existing.Select(f => f.Name), true);
			var ts = Environment.Context.Tenant.GetService<IMicroServiceTemplateService>().Query();
			var template = ts.Count == 0 ? Guid.Empty : ts[0].Token;
			var id = Guid.NewGuid();

			Environment.Context.Tenant.GetService<IMicroServiceManagementService>().Insert(id, name, DomQuery.Closest<IResourceGroupScope>(Owner).ResourceGroup.Token, template, null, null);
			var ms = Environment.Context.Tenant.GetService<IMicroServiceService>().Select(id);

			var r = Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);

			r.MessageKind = InformationKind.Success;
			r.Message = string.Format(SR.MicroServiceCreateSuccess, name);
			r.Title = SR.DevCreateMicroService;
			r.ExplorerPath = string.Format("{0}/{1}", DomQuery.Path(Element), ms.Token);

			return r;
		}

		protected override bool OnCreateToolbarAction(IDesignerToolbarAction action)
		{
			return action.Id != Undo.ActionId && action.Id != Ide.Designers.Toolbar.Clear.ActionId;
		}

		public override bool SupportsReorder
		{
			get { return false; }
		}
	}
}
