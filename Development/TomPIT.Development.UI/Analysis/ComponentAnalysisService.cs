using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;

namespace TomPIT.Development.Analysis
{
	internal class ComponentAnalysisService : TenantObject, IComponentAnalysisService
	{
		public ComponentAnalysisService(ITenant tenant) : base(tenant)
		{
		}

		public List<IComponentDevelopmentState> Query(DateTime timeStamp)
		{
			throw new NotImplementedException();
		}

		public void Reset(IComponentDevelopmentState state)
		{
			throw new NotImplementedException();
		}
	}
}
