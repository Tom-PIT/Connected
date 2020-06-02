using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Development;

namespace TomPIT.Ide.VersionControl
{
	public enum ChangeQueryMode
	{
		MetaData = 1,
		Full = 2
	}
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
		IComponentHistory SelectCommitDetail(Guid commit, Guid component);
		IComponentHistory SelectNonCommited(Guid component);

		void Rollback(Guid commit, Guid component);
		void Rollback(Guid commit);

		IChangeDescriptor GetChanges(ChangeQueryMode mode);
		IVersionControlDiffDescriptor GetDiff(Guid component, Guid id);

		List<IRepository> QueryRepositories();
		List<IMicroServiceBinding> QueryActiveBindings();
		IMicroServiceBinding SelectBinding(Guid service, string repository);
		List<IMicroServiceBinding> QueryBindings(Guid service);
		void UpdateBinding(Guid service, string repository, long commit, DateTime date, bool active);
		void DeleteBinding(Guid service, string repository);
		void InsertRepository(string name, string url, string userName, string password);
		void UpdateRepository(string existingName, string name, string url, string userName, string password);
		void DeleteRepository(string name);
		IRepository SelectRepository(string name);
	}
}
