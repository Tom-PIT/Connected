using System;

namespace TomPIT.Globalization
{
	public interface ITimezoneProvider
	{
		DateTime FromUtc(DateTime value);
		DateTime ToUtc(DateTime value);

		TimeZoneInfo Timezone { get; }
	}
}
