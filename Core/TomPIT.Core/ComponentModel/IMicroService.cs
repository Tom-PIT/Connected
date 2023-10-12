using System;

namespace TomPIT.ComponentModel
{
	[Flags]
	public enum MicroServiceStages
	{
		None = 0,
		Development = 1,
		QualityAssurance = 2,
		Staging = 4,
		Production = 8,
		Any = int.MaxValue
	}

	public interface IMicroService
	{
		string Name { get; }
		string Url { get; }
		Guid Token { get; }
		MicroServiceStages SupportedStages { get; }
		Guid ResourceGroup { get; }
		Guid Template { get; }
		string Version { get; }
		string Commit { get; }
	}
}