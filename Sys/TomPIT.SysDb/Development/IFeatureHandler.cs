using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Features;

namespace TomPIT.SysDb.Development
{
	public interface IFeatureHandler
	{
		List<IFeature> Query();
		void Insert(IMicroService microService, string name, Guid token);
		void Update(IFeature feature, string name);
		void Delete(IFeature feature);

		IFeature Select(IMicroService microService, Guid token);
		IFeature Select(IMicroService microService, string name);
	}
}
