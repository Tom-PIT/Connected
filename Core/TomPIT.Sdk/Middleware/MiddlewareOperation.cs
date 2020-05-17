using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TomPIT.Annotations;

namespace TomPIT.Middleware
{
	public abstract class MiddlewareOperation : MiddlewareComponent, IMiddlewareOperation, IMiddlewareTransactionClient
	{
		[SkipValidation]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public IMiddlewareTransaction Transaction
		{
			get
			{
				if (Context is MiddlewareContext mc)
				{
					var transaction = mc.Transaction;

					if (transaction != null && transaction is MiddlewareTransaction mt)
						mt.Notify(this);

					return transaction;
				}

				return null;
			}
			internal set
			{
				if (Context is MiddlewareContext mc)
					mc.Transaction = value;

				if (value is MiddlewareTransaction transaction)
					transaction.Notify(this);
			}
		}

		//protected void RenderPartial(string partialName)
		//{
		//	if (Shell.HttpContext == null)
		//		throw new RuntimeException(SR.ErrHttpContextNull);

		//	var engine = Shell.HttpContext.RequestServices.GetService(typeof(IViewEngine)) as IViewEngine;

		//	engine.Context = Shell.HttpContext;
		//	engine.RenderPartial(Context as IMicroServiceContext, partialName);
		//}

		protected void Rollback()
		{
			if (Transaction is MiddlewareTransaction t)
				t.Rollback();
		}

		void IMiddlewareTransactionClient.CommitTransaction()
		{
			OnCommit();
			OnCommitting();
		}

		void IMiddlewareTransactionClient.RollbackTransaction()
		{
			OnRollbacking();
			OnRollback();
		}

		protected virtual void OnCommit()
		{

		}

		protected virtual void OnRollback()
		{

		}

		protected internal void Invoked()
		{
			var mc = Context as MiddlewareContext;

			if (mc?.Owner == null)
			{
				if (!(Transaction is MiddlewareTransaction transaction))
					return;

				transaction.Commit();
			}
		}

		protected internal virtual void OnCommitting()
		{
		}
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected internal virtual void OnRollbacking()
		{
		}

		protected internal virtual void OnAuthorizing()
		{
		}

		protected internal virtual void OnValidating()
		{
		}

		protected internal virtual void AuthorizePolicies()
		{
			var attributes = GetType().GetCustomAttributes(true);

			var targets = new List<AuthorizationPolicyAttribute>();

			foreach (var attribute in attributes)
			{
				if (!(attribute is AuthorizationPolicyAttribute policy) || policy.MiddlewareStage == AuthorizationMiddlewareStage.Result)
					continue;

				targets.Add(policy);
			}

			Exception firstFail = null;
			bool onePassed = false;

			foreach (var attribute in targets.OrderByDescending(f => f.Priority))
			{
				try
				{
					if (attribute.Behavior == AuthorizationPolicyBehavior.Optional && onePassed)
						continue;

					attribute.Authorize(Context, this);

					onePassed = true;
				}
				catch (Exception ex)
				{
					if (attribute.Behavior == AuthorizationPolicyBehavior.Mandatory)
						throw ex;

					firstFail = ex;
				}
			}

			if (!onePassed && firstFail != null)
				throw firstFail;
		}
	}
}
