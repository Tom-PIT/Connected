using System;

namespace TomPIT.Runtime.ApplicationContextServices
{
	public interface ITimezoneService
	{
		DateTime FromUtc(DateTime value);
		DateTime ToUtc(DateTime value);

		TimeZoneInfo Timezone { get; }
	}
}
