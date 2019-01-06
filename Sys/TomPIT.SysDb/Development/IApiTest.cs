using System;

namespace TomPIT.SysDb.Development
{
	public interface IApiTest
	{
		string Title { get; }
		string Description { get; }
		Guid Identifier { get; }
		string Api { get; }
		string Tags { get; }
	}
}
