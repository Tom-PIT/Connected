using System;

namespace TomPIT.BigData
{
	public class TimeZoneArgs : EventArgs
	{
		public TimeZoneArgs(Guid timeZone)
		{
			TimeZone = timeZone;
		}

		public Guid TimeZone { get; }
	}
}
