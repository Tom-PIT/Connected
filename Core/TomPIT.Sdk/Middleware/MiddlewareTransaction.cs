using System;
using System.Collections.Generic;

namespace TomPIT.Middleware
{
	internal class MiddlewareTransaction : MiddlewareObject, IMiddlewareTransaction
	{
		private Stack<IMiddlewareTransactionClient> _operations = null;

		public Guid Id { get; set; }
		public MiddlewareTransactionState State { get; private set; } = MiddlewareTransactionState.Active;

		public MiddlewareTransaction(IMiddlewareContext context) : base(context)
		{
		}

		private Stack<IMiddlewareTransactionClient> Operations
		{
			get
			{
				if (_operations == null)
					_operations = new Stack<IMiddlewareTransactionClient>();

				return _operations;
			}
		}

		public void Commit()
		{
			State = MiddlewareTransactionState.Committing;

			var mc = Context as MiddlewareContext;

			foreach (var connection in mc.Connections.DataConnections)
			{
				/*try
				{
					connection.Commit();
				}
				catch (Exception ex)
				{
					Context.Services.Diagnostic.Error("Api", nameof(MiddlewareTransaction), ex.Message);
				}*/
				connection.Commit();
			}

			mc.CloseConnections();

			while (Operations.Count > 0)
			{
				/*try
				{
					Operations.Pop().CommitTransaction();
				}
				catch (Exception ex)
				{
					Context.Services.Diagnostic.Error("Middleware", nameof(MiddlewareTransaction), ex.Message);
				}*/
				Operations.Pop().CommitTransaction();
			}

			State = MiddlewareTransactionState.Completed;
		}

		public void Notify(IMiddlewareTransactionClient operation)
		{
			if (Operations.Contains(operation))
				return;

			Operations.Push(operation);
		}

		public void Rollback()
		{
			State = MiddlewareTransactionState.Reverting;

			var mc = Context as MiddlewareContext;

			foreach (var connection in mc.Connections.DataConnections)
			{
				/*try
				{
					connection.Rollback();
				}
				catch (Exception ex)
				{
					Context.Services.Diagnostic.Error("Api", nameof(MiddlewareTransaction), ex.Message);
				}*/
				connection.Rollback();
			}

			mc.CloseConnections();

			while (Operations.Count > 0)
			{
				/*try
				{
					Operations.Pop().RollbackTransaction();
				}
				catch (Exception ex)
				{
					Context.Services.Diagnostic.Error("Api", nameof(MiddlewareTransaction), ex.Message);
				}*/
				Operations.Pop().RollbackTransaction();
			}

			State = MiddlewareTransactionState.Completed;
		}
	}
}
