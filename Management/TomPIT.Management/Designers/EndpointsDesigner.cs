using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using TomPIT.Annotations.Design;
using TomPIT.Design;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Designers;
using TomPIT.Environment;
using TomPIT.Ide;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Designers.ActionResults;
using TomPIT.Ide.Designers.Toolbar;
using TomPIT.Management.Dom;
using TomPIT.Management.Environment;
using TomPIT.Management.Items;

namespace TomPIT.Management.Designers
{
	internal class EndpointsDesigner : CollectionDesigner<EndpointsElement>
	{
		public EndpointsDesigner(EndpointsElement element) : base(element)
		{

		}

		protected override IDesignerActionResult Add(IItemDescriptor d)
		{
			if (string.Compare(d.Id, EndpointsCollection.Endpoints, true) == 0)
				return CreateEndpoint();

			return Result.EmptyResult(this);
		}

		protected override IDesignerActionResult Remove(JObject data)
		{
			var id = data.Optional("id", Guid.Empty);

			if (id == Guid.Empty)
				throw IdeException.ExpectedParameter(this, IdeEvents.DesignerAction, "id");

			var endpoint = Owner.Existing.FirstOrDefault(f => f.Token == id);

			Environment.Context.Tenant.GetService<IInstanceEndpointManagementService>().Delete(endpoint.Token);

			return Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);
		}

		protected override IDesignerActionResult Clear(JObject data)
		{
			foreach (var i in Owner.Existing)
				Environment.Context.Tenant.GetService<IInstanceEndpointManagementService>().Delete(i.Token);

			return Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);
		}

		private IDesignerActionResult CreateEndpoint()
		{
			var existing = Owner.Existing;
			var name = Environment.Context.Tenant.GetService<INamingService>().Create("Endpoint", existing.Select(f => f.Name), true);

			var id = Environment.Context.Tenant.GetService<IInstanceEndpointManagementService>().Insert(name, InstanceFeatures.Application, string.Empty, string.Empty, InstanceStatus.Disabled, InstanceVerbs.All);
			var endpoint = Environment.Context.Tenant.GetService<IInstanceEndpointService>().Select(id);
			var r = Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);

			r.MessageKind = InformationKind.Success;
			r.Message = string.Format("Instance endpoint created successfully", name);
			r.Title = "Create instance endpoint";
			r.ExplorerPath = string.Format("{0}/{1}", DomQuery.Path(Element), endpoint.Token);

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
