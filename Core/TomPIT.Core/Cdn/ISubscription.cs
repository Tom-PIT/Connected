using System;

namespace TomPIT.Cdn
{
	public interface ISubscription
	{
		Guid Handler { get; }
		string Topic { get; }
		string PrimaryKey { get; }
	}
}
