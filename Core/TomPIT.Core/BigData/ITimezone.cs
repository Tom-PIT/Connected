using System;

namespace TomPIT.BigData
{
	public interface ITimezone
	{
		string Name { get; }
		int Offset { get; }
		Guid Token { get; }
	}
}
