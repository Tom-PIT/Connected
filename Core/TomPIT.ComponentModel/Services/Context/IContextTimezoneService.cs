using System;

namespace TomPIT.Services.Context
{
	public interface IContextTimezoneService
	{
		DateTime FromUtc(DateTime value);
		DateTime ToUtc(DateTime value);

		TimeZoneInfo Timezone { get; }
	}
}
