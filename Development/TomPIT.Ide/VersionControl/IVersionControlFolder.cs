using System;

namespace TomPIT.Ide.VersionControl
{
	public interface IVersionControlFolder
	{
		Guid Token { get; }
		string Name { get; }
		Guid Parent { get; }
	}
}
