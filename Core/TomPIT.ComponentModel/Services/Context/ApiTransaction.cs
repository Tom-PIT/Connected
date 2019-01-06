using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;

namespace TomPIT.Services.Context
{
	internal class ApiTransaction : IApiTransaction
	{
		private Stack<IApiOperation> _operations = null;

		public Guid Id { get; set; }
		public string Name { get; set; }

		public ApiTransaction(IExecutionContext context)
		{
			Context = context;
		}

		private IExecutionContext Context { get; }

		private Stack<IApiOperation> Operations
		{
			get
			{
				if (_operations == null)
					_operations = new Stack<IApiOperation>();

				return _operations;
			}
		}

		public void Commit()
		{
			var apiInvoke = new ApiInvoke(Context);

			while (Operations.Count > 0)
				apiInvoke.Commit(Operations.Pop(), this);
		}

		public void Notify(IApiOperation operation)
		{
			Operations.Push(operation);
		}

		public void Rollback()
		{
			var apiInvoke = new ApiInvoke(Context);

			while (Operations.Count > 0)
				apiInvoke.Rollback(Operations.Pop(), this);
		}
	}
}
