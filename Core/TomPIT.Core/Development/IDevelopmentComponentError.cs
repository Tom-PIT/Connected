using System;

namespace TomPIT.Development
{
	public interface IDevelopmentComponentError : IDevelopmentError
	{
		Guid MicroService { get; }
		string ComponentName { get; }
		string ComponentCategory { get; }
	}
}
