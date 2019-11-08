using System;
using TomPIT.Exceptions;
using TomPIT.UI;

namespace TomPIT.Middleware
{
	public abstract class MiddlewareOperation : MiddlewareComponent, IMiddlewareOperation
	{
		protected MiddlewareOperation()
		{
		}

		protected MiddlewareOperation(IMiddlewareTransaction transaction)
		{
			Transaction = transaction;

			Transaction.Notify(this);
		}

		protected IMiddlewareTransaction Transaction { get; }

		public IMiddlewareTransaction BeginTransaction()
		{
			return new MiddlewareTransaction(Context)
			{
				Id = Guid.NewGuid()
			};
		}

		public IMiddlewareTransaction BeginTransaction(string name)
		{
			return new MiddlewareTransaction(Context)
			{
				Id = Guid.NewGuid(),
				Name = name
			};
		}

		public void Commit()
		{
			OnCommit();
		}

		protected virtual void OnCommit()
		{

		}

		public void Rollback()
		{
			OnRollback();
		}

		protected virtual void OnRollback()
		{

		}

		protected void RenderPartial(string partialName)
		{
			if (Shell.HttpContext == null)
				throw new RuntimeException(SR.ErrHttpContextNull);

			var engine = Shell.HttpContext.RequestServices.GetService(typeof(IViewEngine)) as IViewEngine;

			engine.Context = Shell.HttpContext;
			engine.RenderPartial(Context as IMicroServiceContext, partialName);
		}
	}
}
