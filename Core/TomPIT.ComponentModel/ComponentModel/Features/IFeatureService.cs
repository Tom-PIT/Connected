using System;
using System.Collections.Generic;

namespace TomPIT.ComponentModel.Features
{
	public delegate void FeatureChangedHandler(object sender, FeatureEventArgs e);

	public interface IFeatureService
	{
		event FeatureChangedHandler FeatureChanged;

		IFeature Select(Guid microService, Guid feature);
		IFeature Select(Guid microService, string name);
		List<IFeature> Query(Guid microService);
	}
}
