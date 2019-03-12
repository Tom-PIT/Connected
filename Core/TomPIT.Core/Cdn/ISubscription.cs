using System;

namespace TomPIT.Cdn
{
	public interface ISubscription
	{
		Guid Token { get; }
		Guid Handler { get; }
		string Topic { get; }
		string PrimaryKey { get; }
	}
}
