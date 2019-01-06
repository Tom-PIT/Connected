using System;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Features;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Application.Dom
{
	internal class FeatureElement : TransactionElement, IFeatureScope
	{
		public FeatureElement(IEnvironment environment, IDomElement parent, IFeature feature) : base(environment, parent)
		{
			Feature = feature;

			Id = feature.Token.ToString();
			Title = Feature.Name;
		}

		public override bool HasChildren => true;
		private IFeature Feature { get; }

		public override object Component => Feature;

		Guid IFeatureScope.Feature => Feature.Token;

		public override bool Commit(object component, string property, string attribute)
		{
			Connection.GetService<IFeatureDevelopmentService>().Update(Environment.Context.MicroService(), Feature.Token, Feature.Name);

			return true;
		}

		public override void LoadChildren()
		{
			Items.Add(new PresentationElement(Environment, this));
			Items.Add(new ModelElement(Environment, this));
			Items.Add(new DataElement(Environment, this));
			Items.Add(new ComponentsElement(Environment, this));
		}

		public override void LoadChildren(string id)
		{
			if (string.Compare(id, PresentationElement.ElementId, true) == 0)
				Items.Add(new PresentationElement(Environment, this));
			else if (string.Compare(id, ModelElement.ElementId, true) == 0)
				Items.Add(new ModelElement(Environment, this));
			else if (string.Compare(id, ComponentsElement.ElementId, true) == 0)
				Items.Add(new ComponentsElement(Environment, this));
			else if (string.Compare(id, DataElement.ElementId, true) == 0)
				Items.Add(new DataElement(Environment, this));
		}
	}
}
