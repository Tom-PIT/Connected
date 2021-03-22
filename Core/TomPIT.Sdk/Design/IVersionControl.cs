using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Development;

namespace TomPIT.Design
{
	public enum ChangeQueryMode
	{
		MetaData = 1,
		Full = 2
	}
	public interface IVersionControl
	{
		void Lock(Guid component, LockVerb verb);
		void DeleteCommit(Guid token);
		void Commit(List<Guid> components, string comment);
		void Undo(List<Guid> components);

		List<IComponent> Changes(Guid microService);
		List<IComponent> Changes();
		List<IComponent> Changes(Guid microService, Guid user);
		ICommit SelectCommit(Guid token); 
		List<ICommit> QueryCommits();
		List<ICommit> LookupCommits(List<Guid> tokens);
		List<ICommit> QueryCommits(Guid microService);
		List<ICommit> QueryCommits(Guid microService, Guid user);
		List<ICommit> QueryCommitsForComponent(Guid microService, Guid component);
		List<IComponentHistory> QueryHistory(Guid component);
		List<IComponentHistory> QueryMicroServiceHistory(Guid microService);
		List<IComponent> QueryCommitComponents(Guid commit);
		List<IComponentHistory> QueryCommitDetails(Guid commit);
		IComponentHistory SelectCommitDetail(Guid commit, Guid component);
		IComponentHistory SelectNonCommited(Guid component);

		void Rollback(Guid commit, Guid component);
		void Rollback(Guid commit);

		IChangeDescriptor GetChanges(ChangeQueryMode mode);
		IChangeDescriptor GetCommitChanges(ChangeQueryMode mode, Guid commit);
		IChangeDescriptor GetChanges(ChangeQueryMode mode, Guid user);
		IDiffDescriptor GetDiff(Guid component, Guid id);
		IDiffDescriptor GetDiff(Guid component, Guid id, Guid commit);

		void DeleteHistory(Guid component);
	}
}
