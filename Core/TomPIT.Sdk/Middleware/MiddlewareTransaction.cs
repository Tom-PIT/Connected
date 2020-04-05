using System;
using System.Collections.Concurrent;
using System.Linq;

namespace TomPIT.Middleware
{
	internal class MiddlewareTransaction : MiddlewareObject, IMiddlewareTransaction
	{
		#region Members

		private ConcurrentStack<IMiddlewareTransactionClient> _operations = null;

		#endregion

		#region Constructors

		public MiddlewareTransaction(IMiddlewareContext context) : base(context)
		{
		}

		#endregion

		#region Properties

		public Guid Id { get; set; }

		public MiddlewareTransactionState State { get; private set; } = MiddlewareTransactionState.Active;

		private ConcurrentStack<IMiddlewareTransactionClient> Operations
		{
			get
			{
				if (_operations == null)
					_operations = new ConcurrentStack<IMiddlewareTransactionClient>();

				return _operations;
			}
		}

		#endregion

		#region Methods

		public void Notify(IMiddlewareTransactionClient operation)
		{
			if (operation == null || Operations.Contains(operation))
			{
				return;
			}

			if (State != MiddlewareTransactionState.Active)
			{
				return;
			}

			Operations.Push(operation);
		}


		public void Commit()
		{
			State = MiddlewareTransactionState.Committing;

			var mc = Context as MiddlewareContext;

			// Commit all DB transactions
			foreach (var connection in mc.Connections.DataConnections)
			{
				connection.Commit();
			}

			// Close all DB connections
			mc.CloseConnections();

			// Commit Transactions
			while (Operations.Count > 0)
			{
				Operations.TryPop(out IMiddlewareTransactionClient op);
				op?.CommitTransaction();
			}

			// The end
			State = MiddlewareTransactionState.Completed;
		}

		public void Rollback()
		{
			State = MiddlewareTransactionState.Reverting;

			var mc = Context as MiddlewareContext;

			// Rollback all DB transactions
			foreach (var connection in mc.Connections.DataConnections)
			{
				connection.Rollback();
			}

			// Close all DB connections
			mc.CloseConnections();

			// Commit Transactions
			while (Operations.Count > 0)
			{
				Operations.TryPop(out IMiddlewareTransactionClient op);
				op?.RollbackTransaction();
			}

			// The end
			State = MiddlewareTransactionState.Completed;
		}

		#endregion
	}
}
