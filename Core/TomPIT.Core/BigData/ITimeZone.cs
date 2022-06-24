using System;

namespace TomPIT.BigData
{
	public interface ITimeZone
	{
		string Name { get; }
		int Offset { get; }
		Guid Token { get; }
	}
}
