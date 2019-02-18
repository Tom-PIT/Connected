using System;

namespace TomPIT.ComponentModel
{
	public enum MicroServiceStatus
	{
		Development = 1,
		Staging = 2,
		Production = 3
	}

	public interface IMicroService
	{
		string Name { get; }
		string Url { get; }
		Guid Token { get; }
		MicroServiceStatus Status { get; }
		Guid ResourceGroup { get; }
		Guid Template { get; }
		Guid Package { get; }
	}
}
