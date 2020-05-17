using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Development;

namespace TomPIT.Ide.VersionControl
{
	public interface IVersionControlService
	{
		void Lock(Guid component, LockVerb verb);
		void Commit(List<Guid> components, string comment);
		void Undo(List<Guid> components);

		List<IComponent> Changes(Guid microService);
		List<IComponent> Changes();
		List<IComponent> Changes(Guid microService, Guid user);
		List<ICommit> QueryCommits(Guid microService);
		List<ICommit> QueryCommits(Guid microService, Guid user);
		List<ICommit> QueryCommitsForComponent(Guid microService, Guid component);
		List<IComponentHistory> QueryHistory(Guid component);
		List<IComponent> QueryCommitComponents(Guid commit);
		List<IComponentHistory> QueryCommitDetails(Guid commit);

		void Rollback(Guid commit, Guid component);
		void Rollback(Guid commit);

		List<IVersionControlDescriptor> GetChanges();
		IVersionControlDiffDescriptor GetDiff(Guid component, Guid blob);
	}
}
