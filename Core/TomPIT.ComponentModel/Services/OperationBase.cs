using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using TomPIT.Annotations;
using TomPIT.ComponentModel.Apis;
using TomPIT.Data;
using TomPIT.Services.Context;

namespace TomPIT.Services
{
	public abstract class OperationBase: ProcessHandler, IOperationBase
	{
		protected OperationBase(IDataModelContext context) : base(context)
		{
		}

		protected OperationBase(IDataModelContext context, IApiTransaction transaction):base(context)
		{
			Transaction = transaction;

			Transaction.Notify(this);
		}

		protected IApiTransaction Transaction { get; }

		public IApiTransaction BeginTransaction()
		{
			return new ApiTransaction(Context)
			{
				Id = Guid.NewGuid()
			};
		}

		public IApiTransaction BeginTransaction(string name)
		{
			return new ApiTransaction(Context)
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
	}
}
