using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.ComponentModel;
using TomPIT.Development;

namespace TomPIT.Proxy.Development
{
    public interface IVersionControlController
    {
        ImmutableList<IComponent> QueryChanges(Guid microService, Guid user);
        ImmutableList<ICommit> QueryCommits(Guid microService, Guid user);
        ImmutableList<ICommit> LookupCommits(List<Guid> tokens);
        ImmutableList<ICommit> QueryCommitsForComponent(Guid microService, Guid component);
        IComponentHistory SelectNonCommited(Guid component);
        ImmutableList<IComponentHistory> QueryHistory(Guid component);
        ImmutableList<IComponentHistory> QueryMicroServiceHistory(Guid microService);
        ImmutableList<IComponentHistory> QueryCheckout(Guid microService);
        ImmutableList<IComponentHistory> QueryCommitDetails(Guid commit);
        ICommit SelectCommit(Guid token);
        IComponentHistory SelectCommitDetail(Guid commit, Guid component);
        ImmutableList<IComponent> QueryCommitComponents(Guid commit);
        void Commit(Guid user, List<Guid> components, string comment);
        void DeleteCommit(Guid token);
        ILockInfo SelectLockInfo(Guid user, Guid component);
        void Lock(Guid user, Guid component, Guid blob, LockVerb verb);
        void Undo(Guid component);
        void DeleteHistory(Guid component);
    }
}
