using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations.Design;
using TomPIT.Design;
using TomPIT.Environment;
using TomPIT.Ide;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Designers.ActionResults;
using TomPIT.Ide.Designers.Toolbar;
using TomPIT.Management.Dom;
using TomPIT.Management.Environment;
using TomPIT.Management.Items;

namespace TomPIT.Management.Designers
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

			Environment.Context.Tenant.GetService<IResourceGroupManagementService>().Delete(user.Token);

			return Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);
		}

		private IDesignerActionResult CreateResourceGroup()
		{
			var existing = Owner.Existing;
			var name = Environment.Context.Tenant.GetService<INamingService>().Create("ResourceGroup", existing.Select(f => f.Name), true);

			var id = Environment.Context.Tenant.GetService<IResourceGroupManagementService>().Insert(name, Guid.Empty, string.Empty);
			var resourceGroup = Environment.Context.Tenant.GetService<IResourceGroupService>().Select(id);

			var r = Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);

			r.MessageKind = InformationKind.Success;
			r.Message = string.Format(SR.ResourceGroupCreateSuccess, name);
			r.Title = SR.DevCreateResourceGroup;
			r.ExplorerPath = string.Format("{0}/{1}", DomQuery.Path(Element), resourceGroup.Token);

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
