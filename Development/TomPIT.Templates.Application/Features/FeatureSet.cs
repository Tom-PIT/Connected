using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Features;

namespace TomPIT.Application.Features
{
	public class FeatureSet : ComponentConfiguration, IFeatureSet
	{
		private ListItems<IFeature> _features = null;

		[Items("TomPIT.Application.Design.Items.FeaturesCollection, TomPIT.Application.Design")]
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
