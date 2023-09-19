using System;
using TomPIT.Analytics;

namespace TomPIT.Proxy.Remote
{
	internal class Mru : IMru
	{
		public Guid Token { get; set; }

		public int Type { get; set; }

		public string PrimaryKey { get; set; }

		public AnalyticsEntity Entity { get; set; }

		public string EntityPrimaryKey { get; set; }

		public DateTime Date { get; set; }

		public string Tags { get; set; }
	}
}
