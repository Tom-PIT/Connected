using System.ComponentModel;
using TomPIT.Security;

namespace TomPIT.Middleware
{
	public abstract class MiddlewareOperation : MiddlewareComponent, IMiddlewareOperation
	{
		protected override void OnContextChanged()
		{
			if (Context is MiddlewareContext middleware)
				middleware.Transactions.Register(this);
		}

		protected void Rollback()
		{
			if (Context is MiddlewareContext middleware)
				middleware.Transactions.Rollback();
		}

		internal void CommitOperation()
		{
			OnCommit();
			OnCommitting();
		}

		internal void RollbackOperation()
		{
			OnRollbacking();
			OnRollback();
		}

		protected virtual void OnCommit()
		{
		}

		protected virtual void OnRollback()
		{
		}

		protected internal void Invoked()
		{
			if (Context is MiddlewareContext middleware)
				middleware.Transactions.Commit(this);
		}

		protected internal virtual void OnCommitting()
		{
		}


		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected internal virtual void OnRollbacking()
		{
		}

		protected internal virtual void OnAuthorizing()
		{
		}

		protected internal virtual void OnValidating()
		{
		}

		protected internal virtual void AuthorizePolicies()
		{
			if (Context is not IElevationContext elevationContext || elevationContext.AuthorizationOwner != this)
				return;

			AsyncUtils.RunSync(() => Context.Tenant.GetService<IAuthorizationService>().AuthorizePoliciesAsync(Context, this));

			if (Context is IElevationContext postElevationContext && postElevationContext.State == ElevationContextState.Revoked)
				postElevationContext.State = ElevationContextState.Granted;
		}
	}
}
