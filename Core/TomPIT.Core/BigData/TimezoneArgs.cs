using System;

namespace TomPIT.BigData
{
	public class TimezoneArgs : EventArgs
	{
		public TimezoneArgs(Guid timezone)
		{
			Timezone = timezone;
		}

		public Guid Timezone { get; }
	}
}
