using System;

namespace TomPIT.Development
{
	public interface IMicroServiceBinding
	{
		Guid Service { get; }
		long Commit { get; }
		DateTime Date { get; }
		bool Active { get; }
		string RepositoryName { get; }
		string RepositoryUrl { get; }
		DateTime LastCommit { get; }
		string ServiceName { get; }
	}
}
