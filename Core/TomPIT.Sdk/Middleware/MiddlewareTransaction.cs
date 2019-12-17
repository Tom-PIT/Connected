using System;
using System.Collections.Generic;

namespace TomPIT.Middleware
{
	internal class MiddlewareTransaction : MiddlewareObject, IMiddlewareTransaction
	{
		private Stack<IMiddlewareTransactionClient> _operations = null;

		public Guid Id { get; set; }

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
		}

		public void Notify(IMiddlewareTransactionClient operation)
		{
			Operations.Push(operation);
		}

		public void Rollback()
		{
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
		}
	}
}
