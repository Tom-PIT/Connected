using System;
using System.Collections.Immutable;

namespace TomPIT.BigData.Partitions
{
	internal interface ITimeZoneService
	{
		ITimeZone Select(Guid token);
		ITimeZone Select(string name);
		ImmutableList<ITimeZone> Query();

		void NotifyChanged(Guid token);
		void NotifyRemoved(Guid token);
	}
}
