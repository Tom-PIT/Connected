using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using TomPIT.Annotations;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.UI;
using TomPIT.Data;
using TomPIT.Services.Context;
using TomPIT.UI;

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

		protected void RenderPartial(string partialName)
		{
			if (Shell.HttpContext == null)
				throw new RuntimeException(SR.ErrHttpContextNull);

			var engine = Shell.HttpContext.RequestServices.GetService(typeof(IViewEngine))as IViewEngine;

			engine.Context = Shell.HttpContext;
			engine.RenderPartial(Context, partialName);
		}
	}
}
