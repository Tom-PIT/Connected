using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using TomPIT.ActionResults;
using TomPIT.Actions;
using TomPIT.Annotations;
using TomPIT.Application.Dom;
using TomPIT.Application.Items;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Features;
using TomPIT.Design;
using TomPIT.Designers;
using TomPIT.Ide;

namespace TomPIT.Application.Design
{
	internal class FeaturesDesigner : CollectionDesigner<FeaturesElement>
	{
		public FeaturesDesigner(IEnvironment environment, FeaturesElement element) : base(environment, element)
		{

		}

		protected override IDesignerActionResult Add(IItemDescriptor d)
		{
			if (string.Compare(d.Id, FeaturesCollection.Feature, true) == 0)
				return CreateFeature();

			return Result.EmptyResult(this);
		}

		protected override IDesignerActionResult Remove(JObject data)
		{
			var id = data.Optional("id", Guid.Empty);

			if (id == Guid.Empty)
				throw IdeException.ExpectedParameter(this, IdeEvents.DesignerAction, "id");

			var feature = Owner.Existing.FirstOrDefault(f => f.Token == id);

			Connection.GetService<IFeatureDevelopmentService>().Delete(Environment.Context.MicroService(), feature.Token);

			return Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);
		}

		protected override IDesignerActionResult Clear(JObject data)
		{
			foreach (var i in Owner.Existing)
				Connection.GetService<IFeatureDevelopmentService>().Delete(Environment.Context.MicroService(), i.Token);

			return Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);
		}

		private IDesignerActionResult CreateFeature()
		{
			var existing = Owner.Existing;
			var name = Connection.GetService<INamingService>().Create("Feature", existing.Select(f => f.Name), true);

			var id = Connection.GetService<IFeatureDevelopmentService>().Insert(Environment.Context.MicroService(), name);
			var feature = Connection.GetService<IFeatureService>().Select(Environment.Context.MicroService(), id);

			var r = Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);

			r.MessageKind = InformationKind.Success;
			r.Message = string.Format(SR.DevColCreateSuccessful, name);
			r.Title = SR.DevAddFeature;
			r.ExplorerPath = string.Format("{0}/{1}", DomQuery.Path(Element), feature.Token);

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
