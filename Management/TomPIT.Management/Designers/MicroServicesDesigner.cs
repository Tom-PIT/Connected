using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.ActionResults;
using TomPIT.Actions;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Dom;
using TomPIT.Ide;
using TomPIT.Items;

namespace TomPIT.Designers
{
	internal class MicroServicesDesigner : CollectionDesigner<MicroServicesElement>
	{
		public MicroServicesDesigner(IEnvironment environment, MicroServicesElement element) : base(environment, element)
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

			Connection.GetService<IMicroServiceManagementService>().Delete(user.Token);

			return Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);
		}

		private IDesignerActionResult CreateMicroService()
		{
			var existing = Owner.Existing;
			var name = Connection.GetService<INamingService>().Create("MicroService", existing.Select(f => f.Name), true);
			var ts = Connection.GetService<IMicroServiceTemplateService>().Query();
			var template = ts.Count == 0 ? Guid.Empty : ts[0].Token;
			var id = Connection.GetService<IMicroServiceManagementService>().Insert(name, DomQuery.Closest<IResourceGroupScope>(Owner).ResourceGroup.Token, template, MicroServiceStatus.Development);
			var ms = Connection.GetService<IMicroServiceService>().Select(id);

			var r = Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);

			r.MessageKind = InformationKind.Success;
			r.Message = string.Format(SR.MicroServiceCreateSuccess, name);
			r.Title = SR.DevCreateMicroService;
			r.ExplorerPath = string.Format("{0}/{1}", DomQuery.Path(Element), ms.Token);

			return r;
		}

		protected override bool OnCreateToolbarAction(IDesignerToolbarAction action)
		{
			return action.Id != Undo.ActionId && action.Id != Actions.Clear.ActionId;
		}

		public override bool SupportsReorder
		{
			get { return false; }
		}
	}
}
