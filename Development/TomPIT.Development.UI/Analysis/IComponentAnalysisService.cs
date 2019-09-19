using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;

namespace TomPIT.Development.Analysis
{
	internal interface IComponentAnalysisService
	{
		List<IComponentDevelopmentState> Query(DateTime timeStamp);
		void Reset(IComponentDevelopmentState state);
	}
}
