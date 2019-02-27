using System;

namespace TomPIT.Development
{
	public enum LockStatus
	{
		Commit = 0,
		Lock = 1
	}

	public enum LockVerb
	{
		None = 0,
		Add = 1,
		Edit = 2,
		Delete = 3
	}

	public interface ICommit
	{
		DateTime Created { get; }
		Guid User { get; }
		string Comment { get; }
		Guid Service { get; }
	}
}
