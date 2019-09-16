using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Features;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Features
{
	public class FeatureSet : ComponentConfiguration, IFeatureSetConfiguration
	{
		private ListItems<IFeature> _features = null;

		[Items(DesignUtils.FeaturesItems)]
		public ListItems<IFeature> Features
		{
			get
			{
				if (_features == null)
					_features = new ListItems<IFeature> { Parent = this };

				return _features;
			}
		}
	}
}
