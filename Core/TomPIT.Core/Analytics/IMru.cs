using System;

namespace TomPIT.Analytics
{
	public enum AnalyticsEntity
	{
		User = 1
	}
	public interface IMru
	{
		Guid Token { get; }
		int Type { get; }
		string PrimaryKey { get; }
		AnalyticsEntity Entity { get; }
		string EntityPrimaryKey { get; }
		DateTime Date { get; }
	}
}
