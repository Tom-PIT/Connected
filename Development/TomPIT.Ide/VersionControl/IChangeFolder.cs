using System;

namespace TomPIT.Ide.VersionControl
{
	public interface IChangeFolder
	{
		Guid Id { get; }
		string Name { get; }
		Guid Parent { get; }
	}
}
