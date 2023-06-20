using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using TomPIT.ComponentModel;
using TomPIT.Development;
using TomPIT.Proxy.Development;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local.Development;
internal class VersionControlDevelopmentController : IVersionControlController
{
    public void Commit(Guid user, List<Guid> components, string comment)
    {
        DataModel.Components.Commit(components, user, comment);
    }

    public void DeleteCommit(Guid token)
    {
        DataModel.Components.DeleteCommit(token);
    }

    public void DeleteHistory(Guid component)
    {
        DataModel.VersionControl.DeleteHistory(component);
    }

    public void Lock(Guid user, Guid component, Guid blob, LockVerb verb)
    {
        DataModel.VersionControl.Lock(component, user, verb, blob);
    }

    public ImmutableList<ICommit> LookupCommits(List<Guid> tokens)
    {
        return DataModel.VersionControl.LookupCommits(tokens).ToImmutableList();
    }

    public ImmutableList<IComponent> QueryChanges(Guid microService, Guid user)
    {
        if (user == default)
            return DataModel.Components.QueryLocks(microService).ToImmutableList();
        else
            return DataModel.Components.QueryLocks(microService, user).ToImmutableList();
    }

    public ImmutableList<IComponentHistory> QueryCheckout(Guid microService)
    {
        return DataModel.VersionControl.QueryCheckout(microService).ToImmutableList();
    }

    public ImmutableList<IComponent> QueryCommitComponents(Guid commit)
    {
        return DataModel.VersionControl.QueryCommitComponents(commit).ToImmutableList();
    }

    public ImmutableList<IComponentHistory> QueryCommitDetails(Guid commit)
    {
        return DataModel.VersionControl.QueryCommitDetails(commit).ToImmutableList();
    }

    public ImmutableList<ICommit> QueryCommits(Guid microService, Guid user)
    {
        return DataModel.VersionControl.QueryCommits(microService, user).ToImmutableList();
    }

    public ImmutableList<ICommit> QueryCommitsForComponent(Guid microService, Guid component)
    {
        return DataModel.VersionControl.QueryCommitsForComponent(microService, component).ToImmutableList();
    }

    public ImmutableList<IComponentHistory> QueryHistory(Guid component)
    {
        return DataModel.VersionControl.QueryHistory(component).ToImmutableList();
    }

    public ImmutableList<IComponentHistory> QueryMicroServiceHistory(Guid microService)
    {
        return DataModel.VersionControl.QueryMicroServiceHistory(microService).ToImmutableList();
    }

    public ICommit SelectCommit(Guid token)
    {
        return DataModel.VersionControl.SelectCommit(token);
    }

    public IComponentHistory SelectCommitDetail(Guid commit, Guid component)
    {
        return DataModel.VersionControl.SelectCommitDetail(commit, component);
    }

    public ILockInfo SelectLockInfo(Guid user, Guid component)
    {
        return DataModel.VersionControl.SelectLockInfo(component, user);
    }

    public IComponentHistory SelectNonCommited(Guid component)
    {
        return DataModel.VersionControl.SelectNonCommited(component);
    }

    public void Undo(Guid component)
    {
        DataModel.VersionControl.Undo(component);
    }
}
