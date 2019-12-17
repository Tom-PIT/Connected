using System;
using TomPIT.Exceptions;
using TomPIT.UI;

namespace TomPIT.Middleware
{
	public abstract class MiddlewareOperation : MiddlewareComponent, IMiddlewareOperation, IMiddlewareTransactionClient
	{
		protected MiddlewareOperation()
		{
		}

		protected MiddlewareOperation(IMiddlewareTransaction transaction)
		{
			AttachTransaction(transaction);
		}

		protected IMiddlewareTransaction Transaction { get; private set; }

		internal void AttachTransaction(IMiddlewareOperation sender)
		{
			if (sender is MiddlewareOperation o)
				AttachTransaction(o.Transaction);
		}

		private void AttachTransaction(IMiddlewareTransaction transaction)
		{
			Transaction = transaction;

			if (transaction is MiddlewareTransaction t)
				t.Notify(this);

			IsCommitable = false;
		}

		public IMiddlewareTransaction Begin()
		{
			if (Transaction != null)
				return Transaction;

			Transaction = new MiddlewareTransaction(Context)
			{
				Id = Guid.NewGuid()
			};

			return Transaction;
		}

		protected bool IsCommitable { get; private set; } = true;

		protected void RenderPartial(string partialName)
		{
			if (Shell.HttpContext == null)
				throw new RuntimeException(SR.ErrHttpContextNull);

			var engine = Shell.HttpContext.RequestServices.GetService(typeof(IViewEngine)) as IViewEngine;

			engine.Context = Shell.HttpContext;
			engine.RenderPartial(Context as IMicroServiceContext, partialName);
		}

		protected void Commit()
		{
			if (!IsCommitable)
				return;

			if (Transaction is MiddlewareTransaction t)
				t.Commit();
		}

		protected void Rollback()
		{
			if (!IsCommitable)
				return;

			if (Transaction is MiddlewareTransaction t)
				t.Rollback();
		}

		void IMiddlewareTransactionClient.CommitTransaction()
		{
			OnCommit();
		}

		void IMiddlewareTransactionClient.RollbackTransaction()
		{
			OnRollback();
		}

		protected virtual void OnCommit()
		{

		}

		protected virtual void OnRollback()
		{

		}
	}
}
