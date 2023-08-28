using System;
using System.Threading.Tasks;
using TomPIT.Middleware;

namespace TomPIT.Data.Storage;

public enum MiddlewareTransactionState
{
    Active = 1,
    Committing = 2,
    Reverting = 3,
    Completed = 4
}

public interface ITransactionContext
{
    MiddlewareTransactionState State { get; }

    bool IsDirty { get; set; }

    event EventHandler? StateChanged;

    void Register(IMiddlewareOperation operation);

    Task Rollback();

    Task Commit(IMiddlewareOperation sender);
}
