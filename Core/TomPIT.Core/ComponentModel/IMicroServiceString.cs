using System;

namespace TomPIT.ComponentModel
{
	public interface IMicroServiceString
	{
		Guid MicroService { get; }
		Guid Element { get; }
		string Property { get; }
		string Value { get; }
		Guid Language { get; }
	}
}
