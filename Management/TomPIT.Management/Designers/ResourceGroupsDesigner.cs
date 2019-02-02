using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using TomPIT.ActionResults;
using TomPIT.Actions;
using TomPIT.Annotations;
using TomPIT.Design;
using TomPIT.Dom;
using TomPIT.Environment;
using TomPIT.Items;

namespace TomPIT.Designers
{
	internal class ResourceGroupsDesigner : CollectionDesigner<ResourceGroupsElement>
	{
		public ResourceGroupsDesigner(ResourceGroupsElement element) : base(element)
		{

		}

		protected override IDesignerActionResult Add(IItemDescriptor d)
		{
			if (string.Compare(d.Id, ResourceGroupsCollection.ResourceGroup, true) == 0)
				return CreateResourceGroup();

			return Result.EmptyResult(this);
		}

		protected override IDesignerActionResult Remove(JObject data)
		{
			var id = data.Optional("id", Guid.Empty);

			if (id == Guid.Empty)
				throw IdeException.ExpectedParameter(this, IdeEvents.DesignerAction, "id");

			var user = Owner.Existing.FirstOrDefault(f => f.Token == id);

			Connection.GetService<IResourceGroupManagementService>().Delete(user.Token);

			return Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);
		}

		private IDesignerActionResult CreateResourceGroup()
		{
			var existing = Owner.Existing;
			var name = Connection.GetService<INamingService>().Create("ResourceGroup", existing.Select(f => f.Name), true);

			var id = Connection.GetService<IResourceGroupManagementService>().Insert(name, Guid.Empty, string.Empty);
			var resourceGroup = Connection.GetService<IResourceGroupService>().Select(id);

			var r = Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);

			r.MessageKind = InformationKind.Success;
			r.Message = string.Format(SR.ResourceGroupCreateSuccess, name);
			r.Title = SR.DevCreateResourceGroup;
			r.ExplorerPath = string.Format("{0}/{1}", DomQuery.Path(Element), resourceGroup.Token);

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
