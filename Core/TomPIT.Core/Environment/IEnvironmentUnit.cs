using System;

namespace TomPIT.Environment
{
	public interface IEnvironmentUnit
	{
		string Name { get; }
		Guid Token { get; }
		Guid Parent { get; }
		int Ordinal { get; }
	}
}
