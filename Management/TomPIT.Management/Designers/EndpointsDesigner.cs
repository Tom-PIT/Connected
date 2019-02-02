using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using TomPIT.ActionResults;
using TomPIT.Actions;
using TomPIT.Annotations;
using TomPIT.Design;
using TomPIT.Dom;
using TomPIT.Environment;
using TomPIT.Ide;
using TomPIT.Items;

namespace TomPIT.Designers
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

			Connection.GetService<IInstanceEndpointManagementService>().Delete(endpoint.Token);

			return Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);
		}

		protected override IDesignerActionResult Clear(JObject data)
		{
			foreach (var i in Owner.Existing)
				Connection.GetService<IInstanceEndpointManagementService>().Delete(i.Token);

			return Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);
		}

		private IDesignerActionResult CreateEndpoint()
		{
			var existing = Owner.Existing;
			var name = Connection.GetService<INamingService>().Create("Endpoint", existing.Select(f => f.Name), true);

			var id = Connection.GetService<IInstanceEndpointManagementService>().Insert(name, InstanceType.Application, string.Empty, string.Empty, InstanceStatus.Disabled, InstanceVerbs.All);
			var endpoint = Connection.GetService<IInstanceEndpointService>().Select(id);
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
