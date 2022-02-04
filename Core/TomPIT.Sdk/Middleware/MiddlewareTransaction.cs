using System;
using System.Collections.Concurrent;
using System.Linq;
using TomPIT.Diagnostics;
using TomPIT.Exceptions;

namespace TomPIT.Middleware
{
	internal class MiddlewareTransaction : MiddlewareObject, IMiddlewareTransaction
	{
		private ConcurrentStack<IMiddlewareTransactionClient> _operations;

		public MiddlewareTransaction(IMiddlewareContext context) : base(context)
		{
		}

		public MiddlewareTransactionState State { get; private set; } = MiddlewareTransactionState.Active;

		private ConcurrentStack<IMiddlewareTransactionClient> Operations => _operations ??= new ConcurrentStack<IMiddlewareTransactionClient>();

        public bool IsDirty { get; set; }

        public void Notify(IMiddlewareTransactionClient operation)
		{
			if (operation == null || Operations.Contains(operation))
				return;

			Operations.Push(operation);
		}

		public void Commit()
		{
			State = MiddlewareTransactionState.Committing;

			var context = Context as MiddlewareContext;

			foreach (var connection in context.Connections.DataConnections)
				connection.Commit();

			context.CloseConnections();

			while (!Operations.IsEmpty)
			{
				try
				{
					if (Operations.TryPop(out IMiddlewareTransactionClient op))
						op?.CommitTransaction();
				}
				catch (TomPITException ex)
				{
					ex.LogError(context, LogCategories.Middleware);
				}
			}

			context.Services.Cache.Flush();

			State = MiddlewareTransactionState.Completed;
		}

		public void Rollback()
		{
			State = MiddlewareTransactionState.Reverting;

			var context = Context as MiddlewareContext;
			context.MarkUnstable();

			foreach (var connection in context.Connections.DataConnections)
				connection.Rollback();

			context.CloseConnections();

			while (!Operations.IsEmpty)
			{
				try
				{
					if (Operations.TryPop(out IMiddlewareTransactionClient op))
						op?.RollbackTransaction();
				}
				catch (TomPITException ex)
				{
					ex.LogError(context, LogCategories.Middleware);
				}
			}

			State = MiddlewareTransactionState.Completed;
		}
	}
}