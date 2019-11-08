using System;
using System.Collections.Generic;

namespace TomPIT.Middleware
{
	internal class MiddlewareTransaction : MiddlewareObject, IMiddlewareTransaction
	{
		private Stack<IMiddlewareOperation> _operations = null;

		public Guid Id { get; set; }
		public string Name { get; set; }

		public MiddlewareTransaction(IMiddlewareContext context) : base(context)
		{
		}

		private Stack<IMiddlewareOperation> Operations
		{
			get
			{
				if (_operations == null)
					_operations = new Stack<IMiddlewareOperation>();

				return _operations;
			}
		}

		public void Commit()
		{
			while (Operations.Count > 0)
			{
				try
				{
					Operations.Pop().Commit();
				}
				catch (Exception ex)
				{
					Context.Services.Diagnostic.Error("Middleware", nameof(MiddlewareTransaction), ex.Message);
				}
			}
		}

		public void Notify(IMiddlewareOperation operation)
		{
			Operations.Push(operation);
		}

		public void Rollback()
		{
			while (Operations.Count > 0)
			{
				try
				{
					Operations.Pop().Rollback();
				}
				catch (Exception ex)
				{
					Context.Services.Diagnostic.Error("Api", nameof(MiddlewareTransaction), ex.Message);
				}
			}
		}
	}
}
