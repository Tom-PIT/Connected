using System;
using System.Collections.Generic;
using TomPIT.Data;

namespace TomPIT.Middleware
{
	internal class MiddlewareTransaction : MiddlewareObject, IMiddlewareTransaction, IMiddlewareConnectionBag
	{
		private Stack<IMiddlewareTransactionClient> _operations = null;
		private Stack<IDataConnection> _connections = null;

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

			while (Connections.Count > 0)
			{
				try
				{
					Connections.Pop().Commit();
				}
				catch (Exception ex)
				{
					Context.Services.Diagnostic.Error("Api", nameof(MiddlewareTransaction), ex.Message);
				}
			}

			//while (Operations.Count > 0)
			//{
			//	try
			//	{
			//		Operations.Pop().CommitTransaction();
			//	}
			//	catch (Exception ex)
			//	{
			//		Context.Services.Diagnostic.Error("Middleware", nameof(MiddlewareTransaction), ex.Message);
			//	}
			//}

			//State = MiddlewareTransactionState.Completed;
		}

		public void Notify(IMiddlewareTransactionClient operation)
		{
			Operations.Push(operation);
		}

		public void Rollback()
		{
			State = MiddlewareTransactionState.Reverting;

			while (Connections.Count > 0)
			{
				try
				{
					Connections.Pop().Rollback();
				}
				catch (Exception ex)
				{
					Context.Services.Diagnostic.Error("Api", nameof(MiddlewareTransaction), ex.Message);
				}
			}

			while (Operations.Count > 0)
			{
				try
				{
					Operations.Pop().RollbackTransaction();
				}
				catch (Exception ex)
				{
					Context.Services.Diagnostic.Error("Api", nameof(MiddlewareTransaction), ex.Message);
				}
			}

			State = MiddlewareTransactionState.Completed;
		}

		public void Push(IDataConnection connection)
		{
			if (!Connections.Contains(connection))
				Connections.Push(connection);
		}

		private Stack<IDataConnection> Connections
		{
			get
			{
				if (_connections == null)
					_connections = new Stack<IDataConnection>();

				return _connections;
			}
		}

		public void Complete()
		{
			while (Operations.Count > 0)
			{
				try
				{
					Operations.Pop().CommitTransaction();
				}
				catch (Exception ex)
				{
					Context.Services.Diagnostic.Error("Middleware", nameof(MiddlewareTransaction), ex.Message);
				}
			}

			State = MiddlewareTransactionState.Completed;
		}
	}
}
