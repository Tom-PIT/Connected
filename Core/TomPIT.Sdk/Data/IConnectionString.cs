using System;

namespace TomPIT.Data
{
	public interface IConnectionString
	{
		Guid DataProvider { get; }
		string Value { get; }
	}
}
