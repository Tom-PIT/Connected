using System;

namespace TomPIT.Development
{
	public interface IComponentHistory
	{
		DateTime Created { get; }
		Guid Blob { get; }
		string Name { get; }
		Guid User { get; }
		Guid Commit { get; }
		Guid Component { get; }
	}
}
