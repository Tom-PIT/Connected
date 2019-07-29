using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using TomPIT.ActionResults;
using TomPIT.Actions;
using TomPIT.Annotations;
using TomPIT.Design;
using TomPIT.Designers;
using TomPIT.Environment;
using TomPIT.Management.BigData;
using TomPIT.Management.Dom;
using TomPIT.Management.Items;

namespace TomPIT.Management.Designers
{
	internal class BigDataNodesDesigner : CollectionDesigner<BigDataNodesElement>
	{
		public BigDataNodesDesigner(BigDataNodesElement element) : base(element)
		{

		}

		protected override IDesignerActionResult Add(IItemDescriptor d)
		{
			if (string.Compare(d.Id, BigDataNodesCollection.Token, true) == 0)
				return CreateNode();

			return Result.EmptyResult(this);
		}

		protected override IDesignerActionResult Remove(JObject data)
		{
			var id = data.Optional("id", Guid.Empty);

			if (id == Guid.Empty)
				throw IdeException.ExpectedParameter(this, IdeEvents.DesignerAction, "id");

			var user = Owner.Existing.FirstOrDefault(f => f.Token == id);

			Connection.GetService<IBigDataManagementService>().DeleteNode(user.Token);

			return Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);
		}

		private IDesignerActionResult CreateNode()
		{
			var existing = Owner.Existing;
			var name = Connection.GetService<INamingService>().Create("Node", existing.Select(f => f.Name), true);

			var id = Connection.GetService<IBigDataManagementService>().InsertNode(name, string.Empty, string.Empty);
			var node = Connection.GetService<IBigDataManagementService>().SelectNode(id);

			var r = Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);

			r.MessageKind = InformationKind.Success;
			r.Message = string.Format(SR.BigDataNodeCreateSuccess, name);
			r.Title = SR.DevCreateBigDataNode;
			r.ExplorerPath = string.Format("{0}/{1}", DomQuery.Path(Element), node.Token);

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
