using System;

namespace TomPIT.Design
{
	public interface IPullRequestFolder
	{
		string Name { get; }
		Guid Id { get; }
		Guid Parent { get; }
	}
}
