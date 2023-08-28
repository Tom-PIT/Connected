using System;
using System.ComponentModel;
using System.Threading.Tasks;
using TomPIT.Security;

namespace TomPIT.Middleware
{
    public abstract class MiddlewareOperation : MiddlewareComponent, IMiddlewareOperation
    {
        [Obsolete("Please use async method")]
        protected void Rollback()
        {
            AsyncUtils.RunSync(RollbackAsync);
        }

        protected override void OnContextChanged()
        {
            if (Context is MiddlewareContext middleware)
                middleware.Transactions.Register(this);
        }

        protected async Task RollbackAsync()
        {
            if (Context is MiddlewareContext middleware)
                await middleware.Transactions.Rollback();
        }

        internal async Task CommitOperation()
        {
            OnCommit();
            OnCommitting();

            await Task.CompletedTask;
        }

        internal async Task RollbackOperation()
        {
            OnRollbacking();
            OnRollback();

            await Task.CompletedTask;
        }

        [Obsolete("Please use async method")]
        protected virtual void OnCommit()
        {
            AsyncUtils.RunSync(OnCommitAsync);
        }

        [Obsolete("Please use async method")]
        protected virtual void OnRollback()
        {
            AsyncUtils.RunSync(OnRollbackAsync);
        }

        protected virtual async Task OnCommitAsync()
        {
            await Task.CompletedTask;
        }

        protected virtual async Task OnRollbackAsync()
        {
            await Task.CompletedTask;
        }

        [Obsolete("Please use async method")]
        protected internal void Invoked()
        {
            AsyncUtils.RunSync(InvokedAsync);
        }

        protected internal async Task InvokedAsync()
        {
            if (Context is MiddlewareContext middleware)
                middleware.Transactions.Commit(this);
        }

        [Obsolete("Please use async method")]
        protected internal virtual void OnCommitting()
        {
            AsyncUtils.RunSync(OnCommittingAsync);
        }

        protected internal virtual async Task OnCommittingAsync()
        {
            await Task.CompletedTask;
        }

        [Obsolete("Please use async method")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected internal virtual void OnRollbacking()
        {
            AsyncUtils.RunSync(OnRollbackingAsync);
        }


        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected internal virtual async Task OnRollbackingAsync()
        {
            await Task.CompletedTask;
        }

        [Obsolete("Please use async method")]
        protected internal virtual void OnAuthorizing()
        {
            AsyncUtils.RunSync(OnAuthorizingAsync);
        }

        protected internal virtual async Task OnAuthorizingAsync()
        {
            await Task.CompletedTask;
        }

        [Obsolete("Please use async method")]
        protected internal virtual void OnValidating()
        {
            AsyncUtils.RunSync(OnValidatingAsync);
        }

        protected internal virtual async Task OnValidatingAsync()
        {
            await Task.CompletedTask;
        }

        [Obsolete("Please use async method")]
        protected internal virtual void AuthorizePolicies()
        {
            AsyncUtils.RunSync(AuthorizePoliciesAsync);
        }

        protected internal virtual async Task AuthorizePoliciesAsync()
        {
            if (Context is not IElevationContext elevationContext || elevationContext.AuthorizationOwner != this)
                return;

            await Context.Tenant.GetService<IAuthorizationService>().AuthorizePoliciesAsync(Context, this);

            if (Context is IElevationContext postElevationContext && postElevationContext.State == ElevationContextState.Revoked)
                postElevationContext.State = ElevationContextState.Granted;
        }
    }
}
