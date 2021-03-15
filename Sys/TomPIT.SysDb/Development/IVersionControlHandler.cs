using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Development;
using TomPIT.Security;

namespace TomPIT.SysDb.Development
{
	public interface IVersionControlHandler
	{
		void InsertCommit(Guid token, IMicroService service, IUser user, DateTime created, string comment, List<IComponent> components);

		ICommit SelectCommit(Guid token);

		List<ICommit> QueryCommits(); 
		List<ICommit> QueryCommits(IMicroService service);
		List<ICommit> QueryCommits(IMicroService service, IComponent component);
		List<ICommit> QueryCommits(IMicroService service, IUser user);

		List<IComponent> QueryCommitComponents(ICommit commit);
		void DeleteCommit(ICommit commit);
		void DeleteHistory(IComponentHistory history);

		void InsertComponentHistory(IUser user, IComponent component, string name, LockVerb verb, DateTime created, Guid blob);
		void UndoComponentHistory(IComponent component);
		List<IComponentHistory> QueryHistory(IComponent component);
		List<IComponentHistory> QueryMicroServiceHistory(IMicroService microService);
		List<IComponentHistory> QueryCommitDetails(ICommit commit);
		IComponentHistory SelectCommitDetail(ICommit commit, Guid component);
		IComponentHistory SelectNonCommited(Guid component);
	}
}
