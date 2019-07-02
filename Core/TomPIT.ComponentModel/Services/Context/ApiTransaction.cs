using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;

namespace TomPIT.Services.Context
{
	internal class ApiTransaction : IApiTransaction
	{
		private Stack<IOperationBase> _operations = null;

		public Guid Id { get; set; }
		public string Name { get; set; }

		public ApiTransaction(IExecutionContext context)
		{
			Context = context;
		}

		private IExecutionContext Context { get; }

		private Stack<IOperationBase> Operations
		{
			get
			{
				if (_operations == null)
					_operations = new Stack<IOperationBase>();

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
				catch(Exception ex)
				{
					Context.Services.Diagnostic.Error("Api", nameof(ApiTransaction), ex.Message);
				}
			}
		}

		public void Notify(IOperationBase operation)
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
				catch(Exception ex)
				{
					Context.Services.Diagnostic.Error("Api", nameof(ApiTransaction), ex.Message);
				}
			}
		}
	}
}
