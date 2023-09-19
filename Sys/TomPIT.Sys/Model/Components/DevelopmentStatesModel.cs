using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.Caching;
using TomPIT.ComponentModel;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Sys.Model.Components
{
	public class DevelopmentStatesModel : SynchronizedRepository<IComponent, Guid>
	{
		public DevelopmentStatesModel(IMemoryCache container) : base(container, "developmentState")
		{
		}


		public void Update(List<IComponentIndexState> states)
		{
			Shell.GetService<IDatabaseService>().Proxy.Development.Components.UpdateStates(states);
		}

		public void Update(List<IComponentAnalyzerState> states)
		{
			Shell.GetService<IDatabaseService>().Proxy.Development.Components.UpdateStates(states);
		}

		public ImmutableArray<IComponentDevelopmentState> Dequeue(int count)
		{
			return ImmutableArray<IComponentDevelopmentState>.Empty;
		}
	}
}
