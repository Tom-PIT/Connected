using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.Events;

namespace TomPIT.Services
{
	public interface IOperationBase
	{
		IApiTransaction BeginTransaction();
		IApiTransaction BeginTransaction(string name);

		void Commit();
		void Rollback();
	}

	public interface IOperation<TReturnValue> : IOperationBase
	{
		TReturnValue Invoke();
	}

	public interface IOperation : IOperationBase
	{
		void Invoke();
	}

	public interface IAsyncOperation
	{
		bool Cancel { get; set; }

		IEventCallback Callback { get; }
	}
}