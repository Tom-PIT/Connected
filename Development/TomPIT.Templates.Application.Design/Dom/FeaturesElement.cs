using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations;
using TomPIT.Application.Design;
using TomPIT.Application.Items;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Features;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Application.Dom
{
	internal class FeaturesElement : TomPIT.Dom.Element
	{
		public const string FolderId = "Features";
		private FeaturesDesigner _designer = null;
		private ExistingFeatures _features = null;
		private PropertyInfo _property = null;

		private class ExistingFeatures
		{
			private List<IFeature> _items = null;

			[Items(typeof(FeaturesCollection))]
			[Browsable(false)]
			public List<IFeature> Items
			{
				get
				{
					if (_items == null)
						_items = new List<IFeature>();

					return _items;
				}
			}
		}
		public FeaturesElement(IEnvironment environment) : base(environment, null)
		{
			Id = FolderId;
			Glyph = "fal fa-cubes";
			Title = SR.DomFeatures;

			((Behavior)Behavior).AutoExpand = true;
		}

		public override object Component => Features;
		public override bool HasChildren { get { return Features.Items.Count > 0; } }
		public override int ChildrenCount { get { return Features.Items.Count; } }
		public override PropertyInfo Property
		{
			get
			{
				if (_property == null)
					_property = Component.GetType().GetProperty("Items");

				return _property;
			}
		}

		public override void LoadChildren()
		{
			foreach (var i in Existing)
				Items.Add(new FeatureElement(Environment, this, i));
		}

		public override void LoadChildren(string id)
		{
			var feature = Connection.GetService<IFeatureService>().Select(Environment.Context.MicroService(), id.AsGuid());

			Items.Add(new FeatureElement(Environment, this, feature));
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new FeaturesDesigner(Environment, this);

				return _designer;
			}
		}

		private ExistingFeatures Features
		{
			get
			{
				if (_features == null)
				{
					_features = new ExistingFeatures();

					var ds = Connection.GetService<IFeatureService>().Query(Environment.Context.MicroService()).OrderBy(f => f.Name);

					foreach (var i in ds)
						_features.Items.Add(i);
				}

				return _features;
			}
		}

		public List<IFeature> Existing { get { return Features.Items; } }
	}
}
