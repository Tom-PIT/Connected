using System;
using System.Collections.Immutable;

namespace TomPIT.BigData.Partitions
{
	internal interface ITimezoneService
	{
		ITimezone Select(Guid token);
		ITimezone Select(string name);
		ImmutableList<ITimezone> Query();

		void NotifyChanged(Guid token);
		void NotifyRemoved(Guid token);
	}
}
