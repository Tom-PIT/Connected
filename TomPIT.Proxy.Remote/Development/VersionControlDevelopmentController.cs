using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.ComponentModel;
using TomPIT.Development;
using TomPIT.Proxy.Development;

namespace TomPIT.Proxy.Remote.Development;
internal class VersionControlDevelopmentController : IVersionControlController
{
    private const string Controller = "VersionControl";
    public void Commit(Guid user, List<Guid> components, string comment)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Commit"), new
        {
            user,
            comment,
            components
        });
    }

    public void DeleteCommit(Guid token)
    {
        Connection.Post(Connection.CreateUrl(Controller, "DeleteCommit"), new
        {
            token
        });
    }

    public void DeleteHistory(Guid component)
    {
        Connection.Post(Connection.CreateUrl(Controller, "DeleteHistory"), new
        {
            component
        });
    }

    public void Lock(Guid user, Guid component, Guid blob, LockVerb verb)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Lock"), new
        {
            component,
            user,
            blob,
            verb
        });
    }

    public ImmutableList<ICommit> LookupCommits(List<Guid> tokens)
    {
        return Connection.Post<List<Commit>>(Connection.CreateUrl(Controller, "QueryCommits"), new
        {
            tokens
        }).ToImmutableList<ICommit>();
    }

    public ImmutableList<IComponent> QueryChanges(Guid microService, Guid user)
    {
        return Connection.Post<List<Component>>(Connection.CreateUrl(Controller, "QueryChanges"), new
        {
            microService,
            user
        }).ToImmutableList<IComponent>();
    }

    public ImmutableList<IComponentHistory> QueryCheckout(Guid microService)
    {
        return Connection.Post<List<ComponentHistory>>(Connection.CreateUrl(Controller, "QueryCheckout"), new
        {
            microService
        }).ToImmutableList<IComponentHistory>();
    }

    public ImmutableList<IComponent> QueryCommitComponents(Guid commit)
    {
        return Connection.Post<List<Component>>(Connection.CreateUrl(Controller, "QueryCommitComponents"), new
        {
            commit
        }).ToImmutableList<IComponent>();
    }

    public ImmutableList<IComponentHistory> QueryCommitDetails(Guid commit)
    {
        return Connection.Post<List<ComponentHistory>>(Connection.CreateUrl(Controller, "QueryCommitDetails"), new
        {
            commit
        }).ToImmutableList<IComponentHistory>();
    }

    public ImmutableList<ICommit> QueryCommits(Guid microService, Guid user)
    {
        return Connection.Post<List<Commit>>(Connection.CreateUrl(Controller, "QueryCommits")).ToImmutableList<ICommit>();
    }

    public ImmutableList<ICommit> QueryCommitsForComponent(Guid microService, Guid component)
    {
        return Connection.Post<List<Commit>>(Connection.CreateUrl(Controller, "QueryCommitsForComponent"), new
        {
            microService,
            component
        }).ToImmutableList<ICommit>();
    }

    public ImmutableList<IComponentHistory> QueryHistory(Guid component)
    {
        return Connection.Post<List<ComponentHistory>>(Connection.CreateUrl(Controller, "QueryHistory"), new
        {
            component
        }).ToImmutableList<IComponentHistory>();
    }

    public ImmutableList<IComponentHistory> QueryMicroServiceHistory(Guid microService)
    {
        return Connection.Post<List<ComponentHistory>>(Connection.CreateUrl(Controller, "QueryMicroServiceHistory"), new
        {
            microService
        }).ToImmutableList<IComponentHistory>();
    }

    public ICommit SelectCommit(Guid token)
    {
        return Connection.Post<Commit>(Connection.CreateUrl(Controller, "SelectCommit"), new
        {
            token
        });
    }

    public IComponentHistory SelectCommitDetail(Guid commit, Guid component)
    {
        return Connection.Post<ComponentHistory>(Connection.CreateUrl(Controller, "SelectCommitDetail"), new
        {
            commit,
            component
        });
    }

    public ILockInfo SelectLockInfo(Guid user, Guid component)
    {
        return Connection.Post<LockInfo>(Connection.CreateUrl(Controller, "SelectLockInfo"), new
        {
            component,
            user
        });
    }

    public IComponentHistory SelectNonCommited(Guid component)
    {
        return Connection.Post<ComponentHistory>(Connection.CreateUrl(Controller, "SelectNonCommited"), new
        {
            component
        });
    }

    public void Undo(Guid component)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Undo"), new
        {
            component
        });
    }
}
