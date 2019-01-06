using System;

namespace TomPIT.ComponentModel
{
	public interface IApiTest
	{
		string Api { get; }
		string Title { get; }
		string Description { get; }
		string Tags { get; }
		Guid Identifier { get; }
	}
}
